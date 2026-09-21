# Processamento Assíncrono de Pedidos

Solução para o code challenge "Processamento assíncrono de pedidos" (Grupo Santa Cruz): uma API .NET 8 que recebe pedidos, delega o processamento (integração externa simulada) para um Worker assíncrono e expõe uma interface React para acompanhamento, tudo autenticado via Keycloak.

## Stack

| Camada | Tecnologia |
|---|---|
| API | .NET 8, ASP.NET Core (Controllers), Swagger/OpenAPI |
| Processamento assíncrono | .NET 8 Worker Service (`BackgroundService`) |
| Persistência | Dapper + PostgreSQL |
| Migrações | DbUp (scripts SQL incrementais numerados) |
| Autenticação | Keycloak (SSO/OIDC), JWT Bearer |
| Frontend | React + TypeScript + Vite, `keycloak-js`, `@tanstack/react-query` |
| Testes | xUnit, Moq, FluentAssertions |
| Orquestração local | Docker Compose |

## Arquitetura

Arquitetura hexagonal (Ports & Adapters, conforme [Alistair Cockburn](https://web.archive.org/web/2005/http://alistair.cockburn.us/Hexagonal+architecture)), com quatro camadas obrigatórias e duas composition roots (API e Worker) que compartilham o mesmo núcleo:

```
                ┌────────────────────┐        ┌────────────────────┐
                │  OrderProcessing    │        │  OrderProcessing    │
                │  .Api (host HTTP)   │        │  .Worker (host BG)  │
                └──────────┬──────────┘        └──────────┬──────────┘
                           │  usa                          │  usa
                           ▼                                ▼
                ┌─────────────────────────────────────────────────┐
                │             OrderProcessing.Application          │
                │  Use Cases (CreateOrder, ProcessNextPending...)  │
                │  Ports (IOrderRepository, IOrderIntegrationGw…)  │
                │  DTOs de entrada/saída + Validação (FluentVal.)  │
                └───────────────────────┬───────────────────────┘
                                         │ depende de
                                         ▼
                              ┌─────────────────────┐
                              │ OrderProcessing.Domain │
                              │ Order, OrderItem,      │
                              │ Product, invariantes   │
                              └─────────────────────┘
                                         ▲
                                         │ implementa as ports
                              ┌─────────────────────────────┐
                              │  OrderProcessing.Infrastructure │
                              │  Dapper repositories, Keycloak  │
                              │  JwtBearer, integração simulada │
                              └─────────────────────────────┘
```

- **Domain** não referencia nada (regras e invariantes puras: um pedido não pode ficar vazio, quantidade > 0, transições de status válidas).
- **Application** define os *ports* (interfaces) e os casos de uso; depende só do Domain.
- **Infrastructure** implementa os ports (Dapper, Keycloak, integração simulada); depende de Application/Domain, nunca o contrário.
- **Api** e **Worker** são dois *composition roots* independentes (dois processos deployáveis) que injetam a mesma Application/Infrastructure — a API só valida, persiste e devolve `202 Accepted`; o Worker é quem efetivamente processa.

### Estrutura de pastas

```
backend/
  src/
    OrderProcessing.Api/            → controllers, auth, Swagger, error handling
    OrderProcessing.Application/    → use cases, ports, DTOs, validação
    OrderProcessing.Domain/         → entidades e regras de negócio
    OrderProcessing.Infrastructure/ → Dapper, Keycloak, integração simulada
    OrderProcessing.Worker/         → BackgroundService de processamento
  database/
    scripts/                        → *.sql incrementais e numerados
    Migrator/                       → console app (DbUp) que aplica os scripts
  tests/
    OrderProcessing.Application.UnitTests/
    OrderProcessing.Api.IntegrationTests/
frontend/                           → React + TypeScript (Vite)
deploy/keycloak/realm-export.json   → realm, client e usuários de demonstração
docker-compose.yml
```

## Como executar (Docker Compose)

Pré-requisitos: Docker Desktop.

```bash
docker compose up --build
```

Isso sobe, na ordem correta (via `depends_on` + healthchecks):

1. **postgres** (5432) — banco `orderprocessing`.
2. **keycloak** (8081) — realm `order-processing` importado automaticamente de `deploy/keycloak/realm-export.json`.
3. **migrator** — aplica os scripts SQL uma única vez e finaliza (`docker compose logs migrator` para ver o resultado).
4. **api** (5080) — `http://localhost:5080/swagger`.
5. **worker** — processa os pedidos pendentes em background (sem porta HTTP).
6. **frontend** (5173) — `http://localhost:5173`.

Usuários de demonstração (já cadastrados no realm, conforme pedido no desafio — não há tela de cadastro):

| Usuário | Senha |
|---|---|
| `alice` | `alice123` |
| `bob` | `bob123` |

Cada usuário só vê os próprios pedidos (isolamento pelo claim `sub` do token).

Para obter um token diretamente (sem passar pelo frontend, útil para testar via Swagger/curl):

```bash
curl -X POST "http://localhost:8081/realms/order-processing/protocol/openid-connect/token" \
  -d "client_id=order-processing-web" -d "grant_type=password" \
  -d "username=alice" -d "password=alice123"
```

## Rodando sem Docker (dev local)

```bash
# 1. Infraestrutura mínima
docker compose up -d postgres keycloak

# 2. Migrações
dotnet run --project backend/database/Migrator

# 3. API (perfil Development já aponta para localhost)
dotnet run --project backend/src/OrderProcessing.Api

# 4. Worker
dotnet run --project backend/src/OrderProcessing.Worker

# 5. Frontend
cd frontend && npm install && npm run dev
```

## Fluxo assíncrono de pedidos

1. `POST /api/orders` valida o payload, busca produtos e preços **no catálogo persistido** (nunca no payload), grava `Order` + `OrderItem` em **uma única transação curta** e responde imediatamente `202 Accepted` com `{ orderId, status: "Pending" }`. Nenhum `Task.Run`, nenhuma chamada externa acontece aqui.
2. O **Worker** (processo separado) roda um loop contínuo (`BackgroundService`) que, a cada 2s (configurável), tenta reivindicar um pedido pendente:
   ```sql
   WITH next_order AS (
       SELECT id FROM orders WHERE status = 'Pending'
       ORDER BY created_at_utc FOR UPDATE SKIP LOCKED LIMIT 1
   )
   UPDATE orders SET status = 'Processing', updated_at_utc = now()
   FROM next_order WHERE orders.id = next_order.id
   RETURNING orders.id, ...
   ```
   Essa é uma transação curta: abre conexão, reivindica, lê os itens, calcula o número da tentativa e **comita e fecha a conexão antes de qualquer chamada externa**.
3. Só então o Worker chama o adaptador de integração simulado (`Task.Delay` assíncrono entre 5–10s, configurável, sem bloquear thread) — **sem transação ou conexão aberta** durante a espera.
4. Ao final, uma **nova transação curta** grava a tentativa em `order_processing_attempts` (`OrderId`, `AttemptNumber`, `StartedAt`, `FinishedAt`, `Success`, `ErrorMessage`) e atualiza o status do pedido para `Completed` ou `Failed`.


## Autenticação (Keycloak)

- A landing page do frontend redireciona para o Keycloak (Authorization Code + PKCE) se não houver sessão válida; a API valida o JWT (assinatura, emissor e audiência) contra o realm `order-processing`.
- **Detalhe técnico deliberado**: em Docker, o navegador acessa o Keycloak por `localhost:8081`, mas os containers da API/Worker não alcançam `localhost` (cada container tem seu próprio loopback) — apenas o hostname interno `keycloak:8080`. Para não deixar o emissor do token (`iss`) inconsistente entre quem emite (Keycloak, visto pelo browser) e quem valida (API, dentro da rede Docker), o Keycloak roda com `KC_HOSTNAME=localhost` (fixa o *front-channel*, o que aparece no token) e `KC_HOSTNAME_BACKCHANNEL_DYNAMIC=true` (permite que o *back-channel* — usado só para buscar as chaves JWKS — responda pelo hostname interno). Na API, isso é refletido em `Keycloak:Authority` (usado para validar o `iss`) separado de `Keycloak:MetadataAddress` (usado só para buscar a configuração OIDC/JWKS pela rede interna). Ver `KeycloakAuthenticationExtensions` e `appsettings.json`.

## Catálogo e preços

`GET /api/products` retorna apenas produtos ativos, previamente inseridos por script SQL (`0005_seed_products.sql` — inclui um produto inativo de propósito, para demonstrar o filtro). O preço de cada item do pedido é sempre resolvido a partir do catálogo persistido no momento da criação do pedido; o valor enviado no payload é ignorado (só `productId` e `quantity` são aceitos).

## Banco de dados e migrações

Scripts SQL incrementais e numerados em `backend/database/scripts/`, aplicados por um console app dedicado (`OrderProcessing.Migrator`, usando DbUp) que:

- Controla quais scripts já foram aplicados (tabela `schemaversions`) — reexecutar o migrator é seguro (idempotente).
- Roda como um container que **termina após aplicar as migrações** (`depends_on: condition: service_completed_successfully` no `docker-compose.yml`), garantindo que API e Worker só sobem depois do schema pronto.

## Testes (backend)

```bash
dotnet test backend/OrderProcessing.sln
```

- **`OrderProcessing.Application.UnitTests`** — regras de domínio (`Order`, `OrderItem`) e casos de uso com dependências mockadas (Moq). Cobre os três cenários obrigatórios:
  - criação de pedido válido;
  - rejeição de pedido sem itens / com quantidade inválida / com produto inexistente no catálogo;
  - falha da integração resultando em status `Failed` (e sucesso resultando em `Completed`).
- **`OrderProcessing.Api.IntegrationTests`** — sobe o pipeline HTTP real (`WebApplicationFactory`) com autenticação e repositórios substituídos por fakes em memória (sem depender de Postgres/Keycloak rodando), validando roteamento, autorização, binding e o formato `ProblemDetails`/`ValidationProblemDetails` ponta a ponta.

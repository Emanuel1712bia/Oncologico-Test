import { useState } from "react";
import { useProducts } from "../hooks/useProducts";
import { useCreateOrder } from "../hooks/useCreateOrder";
import { extractErrorMessage } from "../api/problemDetails";

const PAGE_SIZE = 10;

export function OrderForm() {
  const [page, setPage] = useState(1);
  const productsQuery = useProducts(page, PAGE_SIZE);
  const createOrderMutation = useCreateOrder();
  const [quantities, setQuantities] = useState<Record<string, number>>({});
  const [confirmation, setConfirmation] = useState<string | null>(null);

  function setQuantity(productId: string, quantity: number) {
    setQuantities((current) => ({ ...current, [productId]: Math.max(0, quantity) }));
  }

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault();
    setConfirmation(null);

    const items = Object.entries(quantities)
      .filter(([, quantity]) => quantity > 0)
      .map(([productId, quantity]) => ({ productId, quantity }));

    if (items.length === 0) {
      return;
    }

    const result = await createOrderMutation.mutateAsync({ items });
    setConfirmation(`Pedido ${result.orderId} criado com status ${result.status}.`);
    setQuantities({});
  }

  if (productsQuery.isLoading) {
    return <p>Carregando catálogo...</p>;
  }

  if (productsQuery.isError) {
    return <p className="error-message">Não foi possível carregar o catálogo de produtos.</p>;
  }

  const paged = productsQuery.data;
  const products = paged?.items ?? [];
  const totalPages = paged ? Math.max(1, Math.ceil(paged.totalCount / paged.pageSize)) : 1;
  const selectedItemsCount = Object.values(quantities).filter((quantity) => quantity > 0).length;
  const hasSelectedItems = selectedItemsCount > 0;

  return (
    <form onSubmit={handleSubmit} className="order-form">
      <table>
        <thead>
          <tr>
            <th>Medicamento</th>
            <th>Apresentação</th>
            <th>Indicação</th>
            <th>Preço</th>
            <th>Quantidade</th>
          </tr>
        </thead>
        <tbody>
          {products.map((product) => (
            <tr key={product.id}>
              <td>{product.name}</td>
              <td>{product.description}</td>
              <td>{product.indication}</td>
              <td>{product.price.toLocaleString("pt-BR", { style: "currency", currency: "BRL" })}</td>
              <td>
                <input
                  type="number"
                  min={0}
                  value={quantities[product.id] ?? 0}
                  onChange={(event) => setQuantity(product.id, Number(event.target.value))}
                  disabled={createOrderMutation.isPending}
                />
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <div className="pagination">
        <button type="button" disabled={page <= 1} onClick={() => setPage((current) => current - 1)}>
          Anterior
        </button>
        <span>
          Página {page} de {totalPages}
        </span>
        <button type="button" disabled={page >= totalPages} onClick={() => setPage((current) => current + 1)}>
          Próxima
        </button>
      </div>

      <button type="submit" disabled={!hasSelectedItems || createOrderMutation.isPending}>
        {createOrderMutation.isPending
          ? "Enviando pedido..."
          : `Criar pedido${hasSelectedItems ? ` (${selectedItemsCount} ${selectedItemsCount === 1 ? "item" : "itens"})` : ""}`}
      </button>

      {createOrderMutation.isError && (
        <p className="error-message">{extractErrorMessage(createOrderMutation.error)}</p>
      )}

      {confirmation && <p className="success-message">{confirmation}</p>}
    </form>
  );
}

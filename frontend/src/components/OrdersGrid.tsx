import { useState } from "react";
import { useOrders } from "../hooks/useOrders";
import { StatusBadge } from "./StatusBadge";

const PAGE_SIZE = 10;

export function OrdersGrid() {
  const [page, setPage] = useState(1);
  const ordersQuery = useOrders(page, PAGE_SIZE);

  if (ordersQuery.isLoading) {
    return <p>Carregando pedidos...</p>;
  }

  if (ordersQuery.isError) {
    return <p className="error-message">Não foi possível carregar os pedidos.</p>;
  }

  const paged = ordersQuery.data;
  if (!paged || paged.items.length === 0) {
    return <p>Nenhum pedido encontrado.</p>;
  }

  const totalPages = Math.max(1, Math.ceil(paged.totalCount / paged.pageSize));

  return (
    <div>
      <table>
        <thead>
          <tr>
            <th>Pedido</th>
            <th>Itens</th>
            <th>Total</th>
            <th>Status</th>
            <th>Criado em</th>
          </tr>
        </thead>
        <tbody>
          {paged.items.map((order) => (
            <tr key={order.id}>
              <td>{order.id}</td>
              <td>{order.items.map((item) => `${item.quantity}x ${item.productName}`).join(", ")}</td>
              <td>{order.totalAmount.toLocaleString("pt-BR", { style: "currency", currency: "BRL" })}</td>
              <td>
                <StatusBadge status={order.status} />
              </td>
              <td>{new Date(order.createdAtUtc).toLocaleString("pt-BR")}</td>
            </tr>
          ))}
        </tbody>
      </table>

      <div className="pagination">
        <button disabled={page <= 1} onClick={() => setPage((current) => current - 1)}>
          Anterior
        </button>
        <span>
          Página {page} de {totalPages}
        </span>
        <button disabled={page >= totalPages} onClick={() => setPage((current) => current + 1)}>
          Próxima
        </button>
      </div>
    </div>
  );
}

import type { OrderStatus } from "../types/api";

const LABELS: Record<OrderStatus, string> = {
  Pending: "Pendente",
  Processing: "Processando",
  Completed: "Concluído",
  Failed: "Falhou",
};

export function StatusBadge({ status }: { status: OrderStatus }) {
  return <span className={`status-badge status-${status.toLowerCase()}`}>{LABELS[status]}</span>;
}

import { useState } from "react";
import keycloak from "./auth/keycloak";
import { OrderForm } from "./components/OrderForm";
import { OrdersGrid } from "./components/OrdersGrid";

type Tab = "new-order" | "orders";

export default function App() {
  const [tab, setTab] = useState<Tab>("new-order");

  return (
    <div className="app">
      <header className="app-header">
        <h1>Processamento de Pedidos</h1>
        <div className="app-header-user">
          <span>{keycloak.tokenParsed?.preferred_username}</span>
          <button onClick={() => keycloak.logout()}>Sair</button>
        </div>
      </header>

      <nav className="app-tabs">
        <button className={tab === "new-order" ? "active" : ""} onClick={() => setTab("new-order")}>
          Novo Pedido
        </button>
        <button className={tab === "orders" ? "active" : ""} onClick={() => setTab("orders")}>
          Meus Pedidos
        </button>
      </nav>

      <main>{tab === "new-order" ? <OrderForm /> : <OrdersGrid />}</main>
    </div>
  );
}

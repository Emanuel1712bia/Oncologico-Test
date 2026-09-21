import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import keycloak from "./auth/keycloak";
import App from "./App";
import "./index.css";

const queryClient = new QueryClient();
const rootElement = document.getElementById("root")!;

keycloak.onTokenExpired = () => {
  keycloak.updateToken(30);
};

keycloak
  .init({ onLoad: "login-required", pkceMethod: "S256" })
  .then((authenticated) => {
    if (!authenticated) {
      keycloak.login();
      return;
    }

    createRoot(rootElement).render(
      <StrictMode>
        <QueryClientProvider client={queryClient}>
          <App />
        </QueryClientProvider>
      </StrictMode>,
    );
  })
  .catch(() => {
    rootElement.innerHTML = "<p>Não foi possível conectar ao servidor de autenticação.</p>";
  });

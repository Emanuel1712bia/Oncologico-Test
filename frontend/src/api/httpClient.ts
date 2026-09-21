import axios from "axios";
import keycloak from "../auth/keycloak";

const httpClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
});

httpClient.interceptors.request.use(async (config) => {
  await keycloak.updateToken(30);
  config.headers.Authorization = `Bearer ${keycloak.token}`;
  return config;
});

httpClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      await keycloak.login();
    }
    return Promise.reject(error);
  },
);

export default httpClient;

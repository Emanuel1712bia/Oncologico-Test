import httpClient from "./httpClient";
import type { PagedResult, Product } from "../types/api";

export async function listProducts(page: number, pageSize: number): Promise<PagedResult<Product>> {
  const response = await httpClient.get<PagedResult<Product>>("/api/products", {
    params: { page, pageSize },
  });
  return response.data;
}

import { useQuery } from "@tanstack/react-query";
import { listProducts } from "../api/productsApi";

export function useProducts(page: number, pageSize: number) {
  return useQuery({
    queryKey: ["products", page, pageSize],
    queryFn: () => listProducts(page, pageSize),
  });
}

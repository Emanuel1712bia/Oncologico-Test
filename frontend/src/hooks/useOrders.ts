import { useQuery } from "@tanstack/react-query";
import { listOrders } from "../api/ordersApi";
import type { OrderDetailsResponse, PagedResult } from "../types/api";

const ACTIVE_STATUSES = new Set(["Pending", "Processing"]);

export function useOrders(page: number, pageSize: number) {
  return useQuery({
    queryKey: ["orders", page, pageSize],
    queryFn: () => listOrders(page, pageSize),
    refetchInterval: (query) => {
      const data = query.state.data as PagedResult<OrderDetailsResponse> | undefined;
      const hasActiveOrders = data?.items.some((order) => ACTIVE_STATUSES.has(order.status)) ?? false;
      return hasActiveOrders ? 3000 : false;
    },
  });
}

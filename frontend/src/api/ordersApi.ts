import httpClient from "./httpClient";
import type {
  CreateOrderRequest,
  OrderAcceptedResponse,
  OrderDetailsResponse,
  PagedResult,
} from "../types/api";

export async function createOrder(request: CreateOrderRequest): Promise<OrderAcceptedResponse> {
  const response = await httpClient.post<OrderAcceptedResponse>("/api/orders", request);
  return response.data;
}

export async function getOrder(orderId: string): Promise<OrderDetailsResponse> {
  const response = await httpClient.get<OrderDetailsResponse>(`/api/orders/${orderId}`);
  return response.data;
}

export async function listOrders(page: number, pageSize: number): Promise<PagedResult<OrderDetailsResponse>> {
  const response = await httpClient.get<PagedResult<OrderDetailsResponse>>("/api/orders", {
    params: { page, pageSize },
  });
  return response.data;
}

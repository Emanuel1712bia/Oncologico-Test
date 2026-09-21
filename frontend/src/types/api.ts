export type OrderStatus = "Pending" | "Processing" | "Completed" | "Failed";

export interface Product {
  id: string;
  name: string;
  description: string;
  indication: string;
  price: number;
}

export interface CreateOrderItemRequest {
  productId: string;
  quantity: number;
}

export interface CreateOrderRequest {
  items: CreateOrderItemRequest[];
}

export interface OrderAcceptedResponse {
  orderId: string;
  status: OrderStatus;
}

export interface OrderItemResponse {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface OrderDetailsResponse {
  id: string;
  userId: string;
  status: OrderStatus;
  totalAmount: number;
  createdAtUtc: string;
  updatedAtUtc: string;
  items: OrderItemResponse[];
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
  errors?: Record<string, string[]>;
}

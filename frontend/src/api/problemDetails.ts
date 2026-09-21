import { isAxiosError } from "axios";
import type { ProblemDetails } from "../types/api";

export function extractErrorMessage(error: unknown): string {
  if (isAxiosError<ProblemDetails>(error) && error.response?.data) {
    const problem = error.response.data;

    if (problem.errors) {
      return Object.values(problem.errors).flat().join(" ");
    }

    if (problem.detail) {
      return problem.detail;
    }

    if (problem.title) {
      return problem.title;
    }
  }

  return "Ocorreu um erro inesperado. Tente novamente.";
}

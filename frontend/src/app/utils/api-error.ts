import axios from "axios";

interface ProblemDetailsPayload {
  title?: unknown;
  detail?: unknown;
  errors?: unknown;
}

export function getApiErrorMessage(
  error: unknown,
  fallbackMessage: string,
): string {
  if (!axios.isAxiosError(error)) {
    return fallbackMessage;
  }

  const status = error.response?.status ?? 0;
  const payload = asProblemDetails(error.response?.data);
  const detail = asNonEmptyString(payload?.detail);
  if (detail) {
    return detail;
  }

  const validationMessage = firstValidationMessage(payload?.errors);
  if (validationMessage) {
    return validationMessage;
  }

  switch (status) {
    case 0:
      return "The server could not be reached. Check your connection and try again.";
    case 400:
      return "The request could not be processed. Check the information and try again.";
    case 401:
      return "Your session has expired. Please sign in and try again.";
    case 403:
      return "You are not allowed to perform this action.";
    case 404:
      return "The requested item could not be found.";
    case 409:
      return "This request conflicts with the current data.";
    default:
      if (status >= 500) {
        return "The server encountered a problem. Please try again later.";
      }
      return asNonEmptyString(payload?.title) ?? fallbackMessage;
  }
}

export function getApiErrorStatus(error: unknown): number | undefined {
  return axios.isAxiosError(error) ? error.response?.status : undefined;
}

export function getApiErrorResponseData(error: unknown): unknown {
  return axios.isAxiosError(error) ? error.response?.data : undefined;
}

function asProblemDetails(value: unknown): ProblemDetailsPayload | null {
  if (typeof value !== "object" || value === null || Array.isArray(value)) {
    return null;
  }

  return {
    title: "title" in value ? value.title : undefined,
    detail: "detail" in value ? value.detail : undefined,
    errors: "errors" in value ? value.errors : undefined,
  };
}

function firstValidationMessage(errors: unknown): string | null {
  if (typeof errors !== "object" || errors === null || Array.isArray(errors)) {
    return null;
  }

  for (const value of Object.values(errors)) {
    if (typeof value === "string" && value.trim()) {
      return value;
    }

    if (Array.isArray(value)) {
      const message = value.find(
        (item): item is string =>
          typeof item === "string" && item.trim().length > 0,
      );
      if (message) {
        return message;
      }
    }
  }

  return null;
}

function asNonEmptyString(value: unknown): string | null {
  return typeof value === "string" && value.trim() ? value : null;
}

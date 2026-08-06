import { HttpErrorResponse } from "@angular/common/http";

export function buildExploreErrorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    if (error.status === 401 || error.status === 403) {
      return "Sign in to explore the live Visit Sweden catalog.";
    }

    if (error.status === 0) {
      return "The Explore API is not reachable right now.";
    }
  }

  return "We could not load Explore right now.";
}

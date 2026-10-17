import { getApiErrorMessage } from "../../utils/api-error";

export function buildExploreErrorMessage(error: unknown): string {
  return getApiErrorMessage(error, "We could not load Explore right now.");
}

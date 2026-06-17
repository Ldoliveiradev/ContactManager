/**
 * Extracts a human-readable message from an RFC 7807 ProblemDetails error
 * response. The API puts business-rule errors in `detail` and ASP.NET
 * framework errors (e.g. model binding) in `title`, so we prefer `detail`.
 */
export function extractApiError(
  err: { status?: number; error?: { detail?: string; title?: string } } | null,
  fallback: string,
): string {
  return err?.error?.detail ?? err?.error?.title ?? fallback;
}

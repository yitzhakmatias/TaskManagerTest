/**
 * Isolates Vite's `import.meta.env` access to a single module. Jest cannot
 * parse `import.meta` under a CommonJS transform, so tests substitute
 * env.jest.ts for this file via jest.config.ts's moduleNameMapper instead of
 * loading this one.
 */
export const API_BASE_URL: string =
  (import.meta.env.VITE_API_BASE_URL as string | undefined) ?? "http://localhost:5122/api";

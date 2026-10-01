import "server-only";
import { cookies } from "next/headers";

const baseUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5144";
export const accessTokenCookie = "reservae_access_token";

export type CurrentUser = { id: string; name: string | null; email: string | null };

export async function currentUser(): Promise<CurrentUser | null> {
  if (!(await cookies()).has(accessTokenCookie)) return null;
  try {
    const response = await authenticatedFetch("/auth/me");
    return response.ok ? await response.json() as CurrentUser : null;
  } catch {
    return null;
  }
}

export async function authenticatedFetch(path: string, options: RequestInit = {}): Promise<Response> {
  const token = (await cookies()).get(accessTokenCookie)?.value;
  if (!token) return new Response(null, { status: 401 });

  const headers = new Headers(options.headers);
  headers.set("Authorization", `Bearer ${token}`);
  return fetch(`${baseUrl}${path}`, { cache: "no-store", ...options, headers });
}

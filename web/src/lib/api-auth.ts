import "server-only";
import { cookies } from "next/headers";

const baseUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5144";
export const accessTokenCookie = "reservae_access_token";

export async function authenticatedFetch(path: string, options: RequestInit = {}): Promise<Response> {
  const token = (await cookies()).get(accessTokenCookie)?.value;
  if (!token) return new Response(null, { status: 401 });

  const headers = new Headers(options.headers);
  headers.set("Authorization", `Bearer ${token}`);
  return fetch(`${baseUrl}${path}`, { cache: "no-store", ...options, headers });
}

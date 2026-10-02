import "server-only";
import { cookies } from "next/headers";
import { accessTokenCookie, apiBaseUrl } from "./api-auth";

export function safeNextPath(next: string | null | undefined, fallback = "/advertiser") {
  return next?.startsWith("/") && !next.startsWith("//") && !next.includes("\\") ? next : fallback;
}

export async function signIn(email: string, password: string): Promise<"success" | "invalid" | "unavailable"> {
  let response: Response;
  try {
    response = await fetch(`${apiBaseUrl}/auth/login`, {
      method: "POST", headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, password }), cache: "no-store",
    });
  } catch {
    return "unavailable";
  }
  if (!response.ok) return response.status >= 500 ? "unavailable" : "invalid";

  const result = await response.json() as { accessToken?: string; expiresIn?: number };
  if (!result.accessToken) return "unavailable";
  (await cookies()).set(accessTokenCookie, result.accessToken, {
    httpOnly: true, sameSite: "lax",
    secure: process.env.SESSION_COOKIE_SECURE === undefined
      ? process.env.NODE_ENV === "production"
      : process.env.SESSION_COOKIE_SECURE === "true",
    path: "/", maxAge: Math.max(1, result.expiresIn ?? 3600),
  });
  return "success";
}

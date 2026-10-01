"use server";

import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import { accessTokenCookie } from "@/src/lib/api-auth";

export type LoginState = { error: string; email: string };

export async function login(_previous: LoginState, formData: FormData): Promise<LoginState> {
  const email = String(formData.get("email") ?? "").trim();
  const password = String(formData.get("password") ?? "");
  const next = String(formData.get("next") ?? "/advertiser");
  if (!email || !password) return { email, error: "Informe e-mail e senha." };

  let response: Response;
  try {
    response = await fetch(`${process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5144"}/auth/login`, {
      method: "POST", headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, password }), cache: "no-store",
    });
  } catch {
    return { email, error: "Não foi possível conectar à API. Tente novamente." };
  }
  if (!response.ok) return { email, error: "E-mail ou senha inválidos." };

  const result = await response.json() as { accessToken?: string; expiresIn?: number };
  if (!result.accessToken) return { email, error: "A API não retornou uma sessão válida." };
  (await cookies()).set(accessTokenCookie, result.accessToken, {
    httpOnly: true, sameSite: "lax", secure: process.env.NODE_ENV === "production",
    path: "/", maxAge: Math.max(1, result.expiresIn ?? 3600),
  });
  redirect(next.startsWith("/") && !next.startsWith("//") ? next : "/advertiser");
}

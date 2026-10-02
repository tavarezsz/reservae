"use server";

import { redirect } from "next/navigation";
import { safeNextPath, signIn } from "@/src/lib/session";
import { apiBaseUrl } from "@/src/lib/api-auth";

export type RegisterState = { email: string; error: string };

export async function register(_previous: RegisterState, formData: FormData): Promise<RegisterState> {
  const email = String(formData.get("email") ?? "").trim();
  const password = String(formData.get("password") ?? "");
  const confirmation = String(formData.get("confirmation") ?? "");
  const next = safeNextPath(String(formData.get("next") ?? "/advertiser"));

  if (!email || !password || !confirmation) return { email, error: "Preencha todos os campos." };
  if (password !== confirmation) return { email, error: "As senhas não coincidem." };

  let response: Response;
  try {
    response = await fetch(`${apiBaseUrl}/auth/register`, {
      method: "POST", headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, password }), cache: "no-store",
    });
  } catch {
    return { email, error: "Não foi possível conectar à API. Tente novamente." };
  }

  if (!response.ok) {
    if (response.status >= 500) return { email, error: "Não foi possível criar a conta agora. Tente novamente." };
    const details = await response.json().catch(() => null) as { errors?: Record<string, string[]> } | null;
    const codes = Object.keys(details?.errors ?? {});
    if (codes.some(code => code === "DuplicateEmail" || code === "DuplicateUserName"))
      return { email, error: "Este e-mail já está cadastrado." };
    if (codes.some(code => code.startsWith("Password")))
      return { email, error: "A senha precisa ter pelo menos 6 caracteres, incluindo maiúscula, minúscula, número e símbolo." };
    return { email, error: "Não foi possível criar a conta. Confira o e-mail e a senha." };
  }

  const signedIn = await signIn(email, password);
  if (signedIn === "success") redirect(next);
  redirect(`/login?registered=1&next=${encodeURIComponent(next)}`);
}

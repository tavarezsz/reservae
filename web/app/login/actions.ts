"use server";

import { redirect } from "next/navigation";
import { safeNextPath, signIn } from "@/src/lib/session";

export type LoginState = { error: string; email: string };

export async function login(_previous: LoginState, formData: FormData): Promise<LoginState> {
  const email = String(formData.get("email") ?? "").trim();
  const password = String(formData.get("password") ?? "");
  const next = String(formData.get("next") ?? "/advertiser");
  if (!email || !password) return { email, error: "Informe e-mail e senha." };

  const result = await signIn(email, password);
  if (result === "invalid") return { email, error: "E-mail ou senha inválidos." };
  if (result === "unavailable") return { email, error: "Não foi possível entrar agora. Tente novamente." };
  redirect(safeNextPath(next));
}

"use client";

import { useActionState } from "react";
import { login, type LoginState } from "./actions";

const initialState: LoginState = { error: "", email: "" };

export function LoginForm({ next }: { next: string }) {
  const [state, action, pending] = useActionState(login, initialState);
  return <form action={action} className="mt-8 space-y-5">
    <input type="hidden" name="next" value={next} />
    <div><label htmlFor="email" className="text-sm font-bold">E-mail</label><input id="email" name="email" type="email" autoComplete="email" required defaultValue={state.email} className="mt-2 min-h-12 w-full rounded-xl border border-line bg-white px-4" /></div>
    <div><label htmlFor="password" className="text-sm font-bold">Senha</label><input id="password" name="password" type="password" autoComplete="current-password" required className="mt-2 min-h-12 w-full rounded-xl border border-line bg-white px-4" /></div>
    {state.error && <p role="alert" className="rounded-xl bg-error-surface p-4 text-sm text-error">{state.error}</p>}
    <button type="submit" disabled={pending} className="min-h-12 w-full rounded-xl bg-brand px-5 text-sm font-bold text-white disabled:opacity-50">{pending ? "Entrando…" : "Entrar"}</button>
  </form>;
}

"use client";

import { useActionState } from "react";
import { register, type RegisterState } from "./actions";

const initialState: RegisterState = { email: "", error: "" };

export function RegisterForm({ next }: { next: string }) {
  const [state, action, pending] = useActionState(register, initialState);
  return <form action={action} className="mt-8 space-y-5">
    <input type="hidden" name="next" value={next} />
    <div><label htmlFor="register-email" className="text-sm font-bold">E-mail</label><input id="register-email" name="email" type="email" autoComplete="email" required defaultValue={state.email} className="mt-2 min-h-12 w-full rounded-xl border border-line bg-white px-4" /></div>
    <div><label htmlFor="register-password" className="text-sm font-bold">Senha</label><input id="register-password" name="password" type="password" autoComplete="new-password" minLength={6} required aria-describedby="password-help" className="mt-2 min-h-12 w-full rounded-xl border border-line bg-white px-4" /><p id="password-help" className="mt-2 text-xs leading-5 text-muted">Use pelo menos 6 caracteres com maiúscula, minúscula, número e símbolo.</p></div>
    <div><label htmlFor="register-confirmation" className="text-sm font-bold">Confirme a senha</label><input id="register-confirmation" name="confirmation" type="password" autoComplete="new-password" required className="mt-2 min-h-12 w-full rounded-xl border border-line bg-white px-4" /></div>
    {state.error && <p role="alert" className="rounded-xl bg-error-surface p-4 text-sm text-error">{state.error}</p>}
    <button type="submit" disabled={pending} className="min-h-12 w-full rounded-xl bg-brand px-5 text-sm font-bold text-white disabled:opacity-50">{pending ? "Criando conta…" : "Criar conta"}</button>
  </form>;
}

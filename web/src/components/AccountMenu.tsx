import Link from "next/link";
import type { CurrentUser } from "@/src/lib/api-auth";
import { logout } from "@/src/lib/auth-actions";

export function AccountMenu({ user, variant }: { user: CurrentUser | null; variant: "sidebar" | "header" }) {
  const name = user?.name?.trim() || user?.email?.split("@")[0] || "Minha conta";
  const initial = name.charAt(0).toLocaleUpperCase("pt-BR");

  if (variant === "sidebar") return <div className="mt-auto border-t border-sidebar-line pt-5">
    {user ? <div className="space-y-4">
      <div className="flex min-w-0 items-center gap-3"><span aria-hidden="true" className="flex size-10 shrink-0 items-center justify-center rounded-full bg-brand font-bold text-white">{initial}</span><div className="min-w-0"><strong className="block truncate text-sm">{name}</strong><span className="block truncate text-[11px] text-muted">{user.email}</span></div></div>
      <form action={logout}><button type="submit" className="min-h-10 w-full rounded-xl border border-sidebar-line bg-white px-3 text-left text-xs font-bold text-brand hover:bg-sidebar-hover">Sair da conta</button></form>
    </div> : <div className="space-y-2"><p className="text-xs text-muted">Acesse sua conta para reservar e anunciar.</p><Link href="/login" className="flex min-h-10 items-center justify-center rounded-xl bg-brand px-3 text-xs font-bold text-white">Entrar</Link><Link href="/register" className="flex min-h-10 items-center justify-center rounded-xl border border-sidebar-line px-3 text-xs font-bold text-brand">Criar conta</Link></div>}
  </div>;

  return <details className="relative min-[960px]:hidden">
    <summary className="flex min-h-11 cursor-pointer list-none items-center gap-2 rounded-xl px-2 text-xs font-bold text-brand focus-visible:outline-2 focus-visible:outline-focus [&::-webkit-details-marker]:hidden">
      <span aria-hidden="true" className="flex size-9 items-center justify-center rounded-full bg-surface-soft">{user ? initial : "?"}</span>
      <span className="sr-only">{user ? `Conta de ${name}` : "Opções de conta"}</span>
      <span aria-hidden="true">{user ? "Conta" : "Entrar"}</span>
    </summary>
    <div className="absolute right-0 z-40 mt-2 w-64 rounded-2xl border border-line bg-white p-4 shadow-xl">
      {user ? <><strong className="block truncate text-sm">{name}</strong><span className="mt-1 block truncate text-xs text-muted">{user.email}</span><form action={logout} className="mt-4 border-t border-line pt-3"><button type="submit" className="min-h-10 w-full rounded-xl bg-surface-soft px-3 text-left text-sm font-bold text-brand">Sair da conta</button></form></> : <div className="space-y-2"><p className="mb-3 text-xs text-muted">Acesse sua conta para reservar e anunciar.</p><Link href="/login" className="flex min-h-10 items-center justify-center rounded-xl bg-brand px-3 text-sm font-bold text-white">Entrar</Link><Link href="/register" className="flex min-h-10 items-center justify-center rounded-xl border border-line px-3 text-sm font-bold text-brand">Criar conta</Link></div>}
    </div>
  </details>;
}

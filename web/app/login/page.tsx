import type { Metadata } from "next";
import Link from "next/link";
import { SiteShell } from "@/src/components/SiteShell";
import { LoginForm } from "./LoginForm";

export const metadata: Metadata = { title: "Entrar | reservaê" };

export default async function LoginPage({ searchParams }: { searchParams: Promise<{ next?: string; registered?: string }> }) {
  const { next, registered } = await searchParams;
  const returnTo = next ?? "/advertiser";
  return <SiteShell><section className="mx-auto max-w-md py-8"><p className="text-[10px] font-extrabold tracking-[.15em] text-brand">SUA CONTA</p><h1 className="mt-3 text-[32px] font-extrabold">Entrar</h1><p className="mt-2 text-sm text-muted">Use sua conta para gerenciar espaços e reservas.</p>{registered === "1" && <p role="status" className="mt-5 rounded-xl bg-surface-soft p-4 text-sm text-brand">Conta criada. Entre para continuar.</p>}<LoginForm next={returnTo} /><p className="mt-6 text-center text-sm text-muted">Ainda não tem conta? <Link href={`/register?next=${encodeURIComponent(returnTo)}`} className="font-bold text-brand hover:underline">Criar conta</Link></p></section></SiteShell>;
}

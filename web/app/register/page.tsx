import type { Metadata } from "next";
import Link from "next/link";
import { SiteShell } from "@/src/components/SiteShell";
import { RegisterForm } from "./RegisterForm";

export const metadata: Metadata = { title: "Criar conta | reservaê" };

export default async function RegisterPage({ searchParams }: { searchParams: Promise<{ next?: string }> }) {
  const { next } = await searchParams;
  const returnTo = next ?? "/advertiser";
  return <SiteShell><section className="mx-auto max-w-md py-8"><p className="text-[10px] font-extrabold tracking-[.15em] text-brand">SUA CONTA</p><h1 className="mt-3 text-[32px] font-extrabold">Criar conta</h1><p className="mt-2 text-sm text-muted">Cadastre-se para reservar horários e anunciar espaços.</p><RegisterForm next={returnTo} /><p className="mt-6 text-center text-sm text-muted">Já tem uma conta? <Link href={`/login?next=${encodeURIComponent(returnTo)}`} className="font-bold text-brand hover:underline">Entrar</Link></p></section></SiteShell>;
}

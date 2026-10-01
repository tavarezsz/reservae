import type { Metadata } from "next";
import { SiteShell } from "@/src/components/SiteShell";
import { LoginForm } from "./LoginForm";

export const metadata: Metadata = { title: "Entrar | reservaê" };

export default async function LoginPage({ searchParams }: { searchParams: Promise<{ next?: string }> }) {
  const { next } = await searchParams;
  return <SiteShell><section className="mx-auto max-w-md py-8"><p className="text-[10px] font-extrabold tracking-[.15em] text-brand">SUA CONTA</p><h1 className="mt-3 text-[32px] font-extrabold">Entrar</h1><p className="mt-2 text-sm text-muted">Use sua conta para gerenciar espaços e reservas.</p><LoginForm next={next ?? "/advertiser"} /></section></SiteShell>;
}

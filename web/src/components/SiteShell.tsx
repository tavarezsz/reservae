import type { ReactNode } from "react";
import Link from "next/link";
import { Icon } from "./atoms/Icon";

<<<<<<< HEAD
export function SiteShell({
  children,
  active = "explore",
}: {
  children: ReactNode;
  active?: "explore" | "bookings";
}) {
  return (
    <div className="min-h-screen text-ink">
      <a
        href="#conteudo"
        className="sr-only focus:not-sr-only focus:fixed focus:top-3 focus:left-3 focus:z-50 focus:rounded-lg focus:bg-brand focus:p-3 focus:text-white"
      >
        Ir para o conteúdo
      </a>
      <aside
        className="fixed inset-y-0 left-0 hidden w-[232px] flex-col border-r border-sidebar-line bg-sidebar px-6 py-9 min-[960px]:flex"
        aria-label="Navegação principal"
      >
        <Link href="/" className="text-[32px] font-extrabold tracking-[-.06em]">
          reservaê
        </Link>
        <p className="mt-2 text-[11px] text-muted">Um lugar para cada plano.</p>
        <p className="mt-12 mb-4 text-[10px] font-extrabold tracking-[.15em] text-muted">
          SEU RESERVAÊ
        </p>
        <nav className="flex flex-col gap-2">
          <Link
            href="/#espacos"
            aria-current={active === "explore" ? "page" : undefined}
            className={`flex items-center gap-3 rounded-xl px-4 py-3.5 text-sm ${active === "explore" ? "bg-brand font-bold text-white" : "text-muted hover:bg-sidebar-hover"}`}
          >
            <Icon name="grid" />
            Explorar espaços
          </Link>
          <Link
            href="/bookings"
            aria-current={active === "bookings" ? "page" : undefined}
            className={`flex items-center gap-3 rounded-xl px-4 py-3.5 text-sm ${active === "bookings" ? "bg-brand font-bold text-white" : "text-muted hover:bg-sidebar-hover"}`}
          >
            <Icon name="calendar" />
            Minhas reservas
          </Link>
          <Link
            href="/advertiser"
            className="flex items-center gap-3 rounded-xl px-4 py-3.5 text-sm text-muted"
          >
            <Icon name="space" />
            Área do anunciante
          </Link>
        </nav>
        <p className="mt-auto border-t border-sidebar-line pt-5 text-xs leading-5 text-muted">
          Mais espaço.
          <br />
          Mais possibilidades.
        </p>
=======
export function SiteShell({ children }: { children: ReactNode }) {
  return (
    <div className="min-h-screen text-ink">
      <a href="#conteudo" className="sr-only focus:not-sr-only focus:fixed focus:top-3 focus:left-3 focus:z-50 focus:rounded-lg focus:bg-brand focus:p-3 focus:text-white">Ir para o conteúdo</a>
      <aside className="fixed inset-y-0 left-0 hidden w-[232px] flex-col border-r border-sidebar-line bg-sidebar px-6 py-9 min-[960px]:flex" aria-label="Navegação principal">
        <Link href="/" className="text-[32px] font-extrabold tracking-[-.06em]">reservaê</Link>
        <p className="mt-2 text-[11px] text-muted">Um lugar para cada plano.</p>
        <p className="mt-12 mb-4 text-[10px] font-extrabold tracking-[.15em] text-muted">SEU RESERVAÊ</p>
        <nav className="flex flex-col gap-2">
          <Link href="/#espacos" className="flex items-center gap-3 rounded-xl bg-brand px-4 py-3.5 text-sm font-bold text-white"><Icon name="grid" />Explorar espaços</Link>
          <Link href="/bookings" className="flex items-center gap-3 rounded-xl px-4 py-3.5 text-sm text-muted"><Icon name="calendar" />Minhas reservas</Link>
          <Link href="/advertiser" className="flex items-center gap-3 rounded-xl px-4 py-3.5 text-sm text-muted"><Icon name="space" />Área do anunciante</Link>
        </nav>
        <p className="mt-auto border-t border-sidebar-line pt-5 text-xs leading-5 text-muted">Mais espaço.<br />Mais possibilidades.</p>
>>>>>>> 840d084e9830d7739a84810dfc2be73144d4d4b0
      </aside>
      <div className="min-[960px]:ml-[232px]">
        <div className="mx-auto max-w-[1440px] px-5 sm:px-8 min-[960px]:px-10 min-[1280px]:px-12">
          <header className="mb-5 flex min-h-20 items-center justify-between border-b border-line sm:mb-8 min-[960px]:min-h-24">
<<<<<<< HEAD
            <Link
              href="/#espacos"
              className="flex items-center gap-2 text-sm font-bold"
            >
              <Icon name="arrow" className="rotate-180" />
              Explorar
            </Link>
            <Link
              href="/"
              className="text-[27px] font-extrabold tracking-[-.06em]"
            >
              reservaê
            </Link>
          </header>
          <main id="conteudo" className="pb-32 xl:pb-14">
            {children}
          </main>
        </div>
        <footer className="bg-ink px-5 py-7 text-dark-text sm:px-8 min-[960px]:px-10 min-[1280px]:px-12">
          <div className="mx-auto flex max-w-[1344px] flex-wrap items-center justify-between gap-3">
            <span className="text-[25px] font-extrabold text-brand-accent">
              reservaê
            </span>
            <p className="text-[11px]">Um lugar para cada plano.</p>
          </div>
        </footer>
=======
            <Link href="/#espacos" className="flex items-center gap-2 text-sm font-bold"><Icon name="arrow" className="rotate-180" />Explorar</Link>
            <Link href="/" className="text-[27px] font-extrabold tracking-[-.06em]">reservaê</Link>
          </header>
          <main id="conteudo" className="pb-32 xl:pb-14">{children}</main>
        </div>
        <footer className="bg-ink px-5 py-7 text-dark-text sm:px-8 min-[960px]:px-10 min-[1280px]:px-12"><div className="mx-auto flex max-w-[1344px] flex-wrap items-center justify-between gap-3"><span className="text-[25px] font-extrabold text-brand-accent">reservaê</span><p className="text-[11px]">Um lugar para cada plano.</p></div></footer>
>>>>>>> 840d084e9830d7739a84810dfc2be73144d4d4b0
      </div>
    </div>
  );
}

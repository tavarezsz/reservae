import type { ReactNode } from "react";
import Link from "next/link";
import { Icon } from "./atoms/Icon";
import { AccountMenu } from "./AccountMenu";
import { currentUser } from "@/src/lib/api-auth";

export async function SiteShell({
  children,
  active = "explore",
}: {
  children: ReactNode;
  active?: "explore" | "bookings" | "advertiser";
}) {
  const user = await currentUser();
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
        <p className="mt-2 text-[11px] text-muted mb-4">Um lugar para cada plano.</p>
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
            aria-current={active === "advertiser" ? "page" : undefined}
            className={`flex items-center gap-3 rounded-xl px-4 py-3.5 text-sm ${active === "advertiser" ? "bg-brand font-bold text-white" : "text-muted hover:bg-sidebar-hover"}`}
          >
            <Icon name="space" />
            Área do anunciante
          </Link>
        </nav>
        <AccountMenu user={user} variant="sidebar" />
      </aside>
      <div className="min-[960px]:ml-[232px]">
        <div className="mx-auto max-w-[1440px] px-5 sm:px-8 min-[960px]:px-10 min-[1280px]:px-12">
          <header className="mb-5 grid min-h-20 grid-cols-[1fr_auto_1fr] items-center gap-2 border-b border-line sm:mb-8 min-[960px]:min-h-24">
            <Link
              href="/#espacos"
              aria-label="Explorar espaços"
              className="flex items-center gap-2 text-xs font-bold sm:text-sm"
            >
              <Icon name="arrow" className="rotate-180" />
              <span className="hidden sm:inline">Explorar</span>
            </Link>
            <Link
              href="/"
              className="text-[27px] font-extrabold tracking-[-.06em]"
            >
              reservaê
            </Link>
            <div className="justify-self-end"><AccountMenu user={user} variant="header" /></div>
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
      </div>
    </div>
  );
}

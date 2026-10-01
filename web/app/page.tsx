import { HomeContent } from "@/src/components/HomeContent";
import { Icon } from "@/src/components/atoms/Icon";
import Link from "next/link";

const steps = [
  ["Encontre seu espaço", "Lugares para o que você precisa."],
  ["Escolha um horário", "Faixas prontas, sem complicação."],
  ["Confirme sua reserva", "Agora é só aproveitar seu espaço."],
];

export default function Home() {
  return (
    <div className="min-h-screen text-ink">
      <a href="#conteudo" className="sr-only focus:not-sr-only focus:fixed focus:top-3 focus:left-3 focus:z-50 focus:rounded-lg focus:bg-brand focus:p-3 focus:text-white">Ir para o conteúdo</a>
      <aside className="fixed inset-y-0 left-0 hidden w-[232px] flex-col border-r border-sidebar-line bg-sidebar px-6 py-9 min-[960px]:flex" aria-label="Navegação principal">
        <Link href="/" aria-label="reservaê — início" className="text-[32px] font-extrabold tracking-[-.06em]">reservaê</Link>
        <p className="mt-2 text-[11px] text-muted mb-4">Um lugar para cada plano.</p>
        <nav className="flex flex-col gap-2">
          <a href="#espacos" aria-current="page" className="flex items-center gap-3 rounded-xl bg-brand px-4 py-3.5 text-sm font-bold text-white"><Icon name="grid" />Explorar espaços</a>
          <Link href="/bookings" className="flex items-center gap-3 rounded-xl px-4 py-3.5 text-sm text-muted hover:bg-sidebar-hover"><Icon name="calendar" />Minhas reservas</Link>
          <Link href="/advertiser" className="flex items-center gap-3 rounded-xl px-4 py-3.5 text-left text-sm text-muted hover:bg-sidebar-hover"><Icon name="space" />Área do anunciante</Link>
        </nav>
        <p className="mt-auto border-t border-sidebar-line pt-5 text-xs leading-5 text-muted">Mais espaço.<br />Mais possibilidades.</p>
      </aside>
      <div className="min-[960px]:ml-[232px]">
        <div className="mx-auto max-w-[1440px] px-5 sm:px-8 min-[960px]:px-10 min-[1280px]:px-12">
          <header className="mb-5 flex min-h-20 items-center justify-between border-b border-line sm:mb-8 min-[960px]:min-h-24">
            <Link href="/" className="text-[27px] font-extrabold tracking-[-.06em]" aria-label="reservaê — início">reservaê<span className="ml-4 hidden text-xs font-medium tracking-normal text-muted min-[960px]:inline">/ explore</span></Link>
            <Link href="/bookings" className="flex items-center gap-2 text-xs font-bold text-ink hover:text-brand"><Icon name="calendar" className="size-4" />Minhas reservas</Link>
          </header>
          <main id="conteudo" className="pb-10 sm:pb-14">
            <HomeContent />
            <section className="mt-10 rounded-3xl bg-surface-soft p-6 sm:mt-12 sm:p-8" aria-labelledby="how-title">
              <p className="text-[10px] font-extrabold tracking-[.15em] text-muted">SIMPLES DO COMEÇO AO FIM</p>
              <h2 id="how-title" className="mt-3 text-[26px] leading-tight font-extrabold tracking-tight sm:text-[30px]">Seu tempo, bem reservado.</h2>
              <ol className="mt-7 grid gap-6 sm:grid-cols-3 sm:gap-5">
                {steps.map(([title, description], index) => <li key={title} className="flex gap-3 sm:block"><span className="flex size-9 shrink-0 items-center justify-center rounded-full bg-surface-hint text-xs font-extrabold text-brand">0{index + 1}</span><div><h3 className="text-sm font-extrabold sm:mt-3">{title}</h3><p className="mt-1 text-xs leading-5 text-muted">{description}</p></div></li>)}
              </ol>
            </section>
            <section className="mt-10 flex flex-col gap-5 sm:mt-12 min-[1100px]:flex-row min-[1100px]:items-center min-[1100px]:justify-between" aria-labelledby="advertise-title">
              <div><h2 id="advertise-title" className="text-[26px] leading-tight font-extrabold tracking-tight sm:text-[30px]">Seu espaço pode virar<br />o próximo plano de alguém.</h2><p className="mt-3 text-sm leading-6 text-muted">Cadastre seu lugar e defina os horários.</p></div>
              <Link href="/advertiser/spaces/new" className="rounded-xl border border-line bg-surface px-5 py-4 transition hover:border-brand"><p className="text-sm font-bold">Quer anunciar seu espaço?</p><p className="mt-1 text-xs text-brand">Cadastrar espaço →</p></Link>
            </section>
          </main>
        </div>
        <footer className="bg-ink text-dark-text"><div className="mx-auto flex max-w-[1440px] flex-wrap items-center justify-between gap-3 px-5 py-7 sm:px-8 min-[960px]:px-10 min-[1280px]:px-12"><span className="text-[25px] font-extrabold tracking-tight text-brand-accent">reservaê</span><p className="text-[11px]">Um lugar para cada plano.</p></div></footer>
      </div>
    </div>
  );
}

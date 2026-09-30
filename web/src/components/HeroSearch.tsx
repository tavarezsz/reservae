"use client";

import { useState, type FormEvent } from "react";
import { Icon } from "./atoms/Icon";

export function HeroSearch({ term, onTermChange, onSearch }: { term: string; onTermChange: (term: string) => void; onSearch: (term: string) => void }) {
  const [error, setError] = useState("");

  function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const value = term.trim();
    if (value.length === 1) {
      setError("Digite pelo menos 2 caracteres para buscar.");
      return;
    }
    setError("");
    onSearch(value);
    document.getElementById("espacos")?.scrollIntoView({ behavior: "smooth", block: "start" });
  }

  return (
    <section className="grid gap-7 rounded-3xl bg-surface-soft p-6 sm:p-9 min-[1100px]:grid-cols-[1.2fr_1fr] min-[1100px]:items-center min-[1100px]:gap-12 min-[1100px]:p-11" aria-labelledby="hero-title">
      <div>
        <span className="inline-flex rounded-full bg-brand-accent px-3 py-2 text-[10px] font-extrabold tracking-[.13em] text-ink">MAIS ESPAÇO PARA SEUS PLANOS</span>
        <h1 id="hero-title" className="mt-5 text-[34px] leading-[1.12] font-extrabold tracking-[-.045em] text-ink sm:text-[44px]">Seu próximo plano<br />tem lugar aqui.</h1>
        <p className="mt-4 text-sm leading-6 text-muted sm:text-base">Para trabalhar, jogar ou se reunir.<br />Encontre um espaço e reserve seu horário.</p>
      </div>
      <form onSubmit={submit} role="search" className="rounded-2xl border border-line bg-surface p-5 sm:p-6">
        <label htmlFor="space-search" className="mb-3 block text-sm font-extrabold text-ink">Qual é o seu próximo plano?</label>
        <div className="flex items-center gap-3 rounded-xl border border-line bg-canvas px-4 focus-within:border-focus focus-within:ring-2 focus-within:ring-focus/20">
          <Icon name="search" className="shrink-0 text-muted" />
          <input id="space-search" name="term" type="search" maxLength={200} value={term} onChange={(event) => { onTermChange(event.target.value); setError(""); }} placeholder="Busque um espaço ou endereço" aria-invalid={!!error} aria-describedby={error ? "search-error" : undefined} className="min-w-0 flex-1 bg-transparent py-4 text-sm text-ink outline-none placeholder:text-muted" />
        </div>
        {error && <p id="search-error" role="alert" className="mt-2 text-xs text-error">{error}</p>}
        <button type="submit" className="mt-3 flex min-h-12 w-full items-center justify-between rounded-xl bg-brand px-4 text-sm font-bold text-on-brand transition hover:bg-brand-hover focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-focus">Buscar espaços<Icon name="arrow" /></button>
        <p className="mt-3 text-[11px] leading-5 text-muted">Encontre lugares pelo nome, endereço ou descrição.</p>
      </form>
    </section>
  );
}

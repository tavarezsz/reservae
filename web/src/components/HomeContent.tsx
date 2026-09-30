"use client";

import { useEffect, useState } from "react";
import { getApiSpaces, getApiSpacesSearch } from "@/lib/api/space/space";
import type { CategoryEnum, SpaceDTO } from "@/lib/api/models";
import { categoryLabels } from "@/src/constants/CategoryLabels";
import { HeroSearch } from "./HeroSearch";
import { SpaceCarousel } from "./SpaceCarousel";
import { Icon } from "./atoms/Icon";

const PAGE_SIZE = 12;

export function HomeContent() {
  const [query, setQuery] = useState({ term: "", page: 1, revision: 0 });
  const [inputTerm, setInputTerm] = useState("");
  const [spaces, setSpaces] = useState<SpaceDTO[]>([]);
  const [category, setCategory] = useState<CategoryEnum | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);
  const [hasMore, setHasMore] = useState(false);

  useEffect(() => {
    const controller = new AbortController();
    const options = { signal: controller.signal };
    const params = { page: query.page, pageSize: PAGE_SIZE };
    const request = query.term
      ? getApiSpacesSearch({ ...params, term: query.term }, options)
      : getApiSpaces(params, options);

    request.then((response) => {
      if (controller.signal.aborted) return;
      const items = response.items ?? [];
      setSpaces((previous) => {
        if (query.page === 1) return items;
        const ids = new Set(previous.map((space) => space.id));
        return [...previous, ...items.filter((space) => space.id == null || !ids.has(space.id))];
      });
      setHasMore(items.length === PAGE_SIZE || (response.totalCount ?? 0) > query.page * PAGE_SIZE);
      setError(false);
    }).catch(() => {
      if (!controller.signal.aborted) setError(true);
    }).finally(() => {
      if (!controller.signal.aborted) setLoading(false);
    });

    return () => controller.abort();
  }, [query]);

  function search(term: string) {
    setInputTerm(term);
    setSpaces([]);
    setCategory(null);
    setLoading(true);
    setError(false);
    setHasMore(false);
    setQuery((previous) => ({ term, page: 1, revision: previous.revision + 1 }));
  }

  function load(retry = false) {
    setLoading(true);
    setError(false);
    setQuery((previous) => ({ ...previous, page: retry ? previous.page : previous.page + 1, revision: previous.revision + 1 }));
  }

  const visibleSpaces = category == null ? spaces : spaces.filter((space) => space.category === category);

  return (
    <>
      <HeroSearch term={inputTerm} onTermChange={setInputTerm} onSearch={search} />
      <section id="espacos" className="scroll-mt-6 pt-7 sm:pt-8" aria-labelledby="spaces-title">
        <div className="-mx-5 flex gap-2 overflow-x-auto px-5 pb-3 sm:mx-0 sm:flex-wrap sm:px-0" role="group" aria-label="Filtrar espaços carregados por categoria">
          {[{ value: null, label: "Todos" }, ...Object.entries(categoryLabels).map(([value, label]) => ({ value: Number(value) as CategoryEnum, label }))].map(({ value, label }) => (
            <button key={value ?? "all"} type="button" aria-pressed={category === value} onClick={() => setCategory(value)} className={`min-h-10 shrink-0 rounded-full border px-4 text-xs font-bold transition focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-focus ${category === value ? "border-brand bg-brand text-white" : "border-line bg-surface text-muted hover:border-line-strong hover:text-ink"}`}>{label}</button>
          ))}
        </div>
        <div className="mt-5 mb-5 flex flex-wrap items-center justify-between gap-3">
          <h2 id="spaces-title" className="text-[26px] font-extrabold tracking-tight sm:text-[30px]">{query.term ? "Resultados da busca" : "Encontre seu lugar"}</h2>
          {(query.term || category != null) && <button type="button" onClick={() => search("")} className="min-h-10 text-xs font-extrabold text-brand underline decoration-brand/30 underline-offset-4">Ver todos os espaços</button>}
        </div>
        <div aria-live="polite" aria-atomic="true">
          {query.term && <p className="mb-4 text-sm text-muted">Espaços para “{query.term}”</p>}
          {category != null && <p className="mb-4 text-xs text-muted">Categoria aplicada aos espaços já carregados.</p>}
          {!loading && !error && visibleSpaces.length === 0 && <div className="rounded-2xl border border-dashed border-line-strong bg-surface px-6 py-10 text-center"><Icon name="search" className="mx-auto mb-3 size-7 text-muted" /><h3 className="font-extrabold">{category != null && hasMore ? "Nenhum espaço desta categoria carregado ainda" : "Nenhum espaço encontrado"}</h3><p className="mt-2 text-sm text-muted">{category != null && hasMore ? "Carregue mais espaços para continuar explorando." : "Tente outro termo ou escolha uma categoria diferente."}</p></div>}
        </div>
        <div aria-busy={loading}>
          {visibleSpaces.length > 0 && <SpaceCarousel key={`${query.term}:${category ?? "all"}`} spaces={visibleSpaces} />}
          <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 xl:grid-cols-3">
          {loading && spaces.length === 0 && Array.from({ length: 3 }, (_, index) => <div key={`loading-${index}`} aria-hidden="true" className="overflow-hidden rounded-[22px] border border-line bg-surface motion-safe:animate-pulse"><div className="aspect-[1.55] bg-surface-hint" /><div className="space-y-4 p-6"><div className="h-3 w-16 rounded bg-surface-hint" /><div className="h-6 w-3/4 rounded bg-surface-hint" /><div className="h-3 w-1/2 rounded bg-surface-hint" /><div className="h-4 w-2/3 rounded bg-surface-hint" /></div></div>)}
          </div>
        </div>
        {loading && <p role="status" className="mt-5 text-center text-sm text-muted">Carregando espaços…</p>}
        {error && <div role="alert" className="mt-5 rounded-2xl bg-error-surface p-5 text-center text-error"><p className="text-sm">Não foi possível carregar os espaços. Tente novamente.</p><button type="button" onClick={() => load(true)} className="mt-3 min-h-10 rounded-lg border border-error/30 px-4 text-xs font-bold focus-visible:outline-2 focus-visible:outline-offset-2">Tentar novamente</button></div>}
        {hasMore && !error && <div className="mt-6 text-center"><button type="button" disabled={loading} onClick={() => load()} className="min-h-11 rounded-xl border border-line-strong bg-surface px-6 text-sm font-bold text-brand transition hover:bg-surface-soft disabled:opacity-50 focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-focus">{loading ? "Carregando…" : "Carregar mais espaços"}</button></div>}
      </section>
    </>
  );
}

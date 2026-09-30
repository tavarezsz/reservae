"use client";

import { useEffect, useRef, useState } from "react";
import type { SpaceDTO } from "@/lib/api/models";
import { SpaceCard } from "./atoms/SpaceCard";

export function SpaceCarousel({ spaces }: { spaces: SpaceDTO[] }) {
  const viewport = useRef<HTMLDivElement>(null);
  const pageSize = useRef(1);
  const [perPage, setPerPage] = useState(1);
  const [activePage, setActivePage] = useState(0);

  useEffect(() => {
    const element = viewport.current;
    if (!element) return;

    const observer = new ResizeObserver(() => {
      const count = window.innerWidth >= 1700 ? 4 : window.innerWidth >= 960 ? 3 : window.innerWidth >= 640 ? 2 : 1;
      if (pageSize.current !== count) {
        pageSize.current = count;
        setPerPage(count);
        setActivePage(0);
        element.scrollTo({ left: 0, behavior: "instant" });
      }
    });
    observer.observe(element);
    return () => observer.disconnect();
  }, []);

  const pages = Array.from({ length: Math.ceil(spaces.length / perPage) }, (_, index) =>
    spaces.slice(index * perPage, (index + 1) * perPage),
  );
  const currentPage = Math.min(activePage, pages.length - 1);

  function goToPage(index: number) {
    const element = viewport.current;
    const page = element?.children[index] as HTMLElement | undefined;
    if (!element || !page) return;
    element.scrollTo({
      left: page.offsetLeft,
      behavior: window.matchMedia("(prefers-reduced-motion: reduce)").matches ? "instant" : "smooth",
    });
  }

  function updatePage() {
    const element = viewport.current;
    if (!element) return;
    const maxScroll = element.scrollWidth - element.clientWidth;
    let closest = 0;
    let distance = Infinity;
    Array.from(element.children).forEach((child, index) => {
      const target = Math.min((child as HTMLElement).offsetLeft, maxScroll);
      const difference = Math.abs(element.scrollLeft - target);
      if (difference < distance) {
        closest = index;
        distance = difference;
      }
    });
    setActivePage(closest);
  }

  return (
    <div role="region" aria-roledescription="carrossel" aria-label="Espaços disponíveis">
      <div
        ref={viewport}
        tabIndex={0}
        aria-label="Use as setas para navegar pelos espaços"
        onScroll={updatePage}
        onKeyDown={(event) => {
          if (event.target !== event.currentTarget) return;
          if (event.key === "ArrowRight" || event.key === "ArrowLeft") {
            event.preventDefault();
            goToPage(Math.max(0, Math.min(pages.length - 1, currentPage + (event.key === "ArrowRight" ? 1 : -1))));
          }
        }}
        className="relative flex snap-x snap-mandatory gap-5 overflow-x-auto overscroll-x-contain rounded-[22px] [scrollbar-width:none] [&::-webkit-scrollbar]:hidden focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-focus"
      >
        {pages.map((page, pageIndex) => (
          <div
            key={pageIndex}
            role="group"
            aria-roledescription="página"
            aria-label={`${pageIndex + 1} de ${pages.length}`}
            className="grid basis-[85%] shrink-0 snap-start gap-5 sm:basis-full"
            style={{ gridTemplateColumns: `repeat(${perPage}, minmax(0, 1fr))` }}
          >
            {page.map((space, index) => <SpaceCard key={space.id ?? index} space={space} />)}
          </div>
        ))}
      </div>
      <div className="mt-4 flex items-center justify-between gap-4">
        <p aria-live="polite" aria-atomic="true" className="shrink-0 text-xs tabular-nums text-muted">
          <span className="sr-only">Página </span>{String(currentPage + 1).padStart(2, "0")} / {String(pages.length).padStart(2, "0")}
        </p>
        <div className="flex gap-2 max-w-[75%] items-center overflow-x-auto [scrollbar-width:none] [&::-webkit-scrollbar]:hidden" role="group" aria-label="Páginas do carrossel">
          {pages.map((_, index) => (
            <button
              key={index}
              type="button"
              onClick={() => goToPage(index)}
              aria-label={`Ir para a página ${index + 1}`}
              aria-current={currentPage === index ? "page" : undefined}
              className="flex w-auto size-11 shrink-0 items-center justify-center rounded-lg focus-visible:outline-2 focus-visible:outline-offset-[-2px] focus-visible:outline-focus"
            >
              <span className={`h-1 w-7 rounded-full transition-colors ${currentPage === index ? "bg-brand" : "bg-line"}`} />
            </button>
          ))}
        </div>
      </div>
    </div>
  );
}

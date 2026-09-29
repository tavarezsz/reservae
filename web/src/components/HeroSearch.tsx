"use client";

export function HeroSearch() {
  return (
    <section className="flex flex-col p-6 bg-surface-soft gap-5 ">
      <span className="text-[10px] text-brand uppercase bg-brand-accent rounded-full line-hei py-3 px-2 font-extrabold w-fit leading-0">
        Mais espaço para seus planos
      </span>
      <h2 className="text-dark-surface text-3xl font-extrabold tracking-tight">
        Seu próximo plano tem lugar aqui.
      </h2>
      <span className="text-muted text-xs ">
        <p>Para trabalhar, jogar ou se reunir. </p>
        <p>Encontre um espaço e reserve seu horário.</p>
      </span>
      <form className="flex flex-col p-4 rounded-2xl bg-white ">
        <div className="flex items-center gap-3 p-3 my-3 rounded-2xl bg-canvas w-full text-xs text-ink leading-0">
          <SearchIcon />
          <input type="text" placeholder="O que você quer reservar?" />
        </div>
        <button
          type="submit"
          className="flex items-center justify-between w-full rounded-2xl bg-brand p-4 text-white font-extrabold text-xs "
        >
          Buscar espaços <ArrowIcon />
        </button>
      </form>
    </section>
  );
}

const SearchIcon = () => {
  return (
    <svg
      width="20"
      height="20"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="1.6"
      stroke-linecap="round"
      stroke-linejoin="round"
      aria-hidden="true"
    >
      <path d="M21 21l-6-6M17 10a7 7 0 1 1-14 0 7 7 0 0 1 14 0"></path>
    </svg>
  );
};

const ArrowIcon = () => {
  return (
    <svg
      width="20"
      height="20"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="1.6"
      stroke-linecap="round"
      stroke-linejoin="round"
      aria-hidden="true"
    >
      <path d="M4 12h16m-6-6 6 6-6 6"></path>
    </svg>
  );
};

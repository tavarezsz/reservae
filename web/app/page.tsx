import { HeroSearch } from "@/src/components/HeroSearch";
import Link from "next/link";

export default function Home() {
  return (
    <div className="flex flex-col">
      <header className="flex items-center justify-between py-3.5 px-6 min-h-20">
        <h1 className="text-dark-surface text-2xl font-extrabold">reservaê</h1>
        <div className="flex items-center gap-2 text-dark-surface"> <ReserveIcon/> <Link className="text-xs font-bold" href="">Minhas reservas</Link></div>
      </header>
      <main>
        <HeroSearch/>
      </main>

      <footer className="flex justify-between items-center p-4 bg-ink">
        <h3 className="text-brand-accent font-extrabold text-[20px]">reservaê</h3>
        <p className="text-[10px] text-white">Um lugar pra cada plano.</p>
      </footer>
    </div>
  );
}

const ReserveIcon = () => {
  return (
    <svg
      width="18"
      height="18"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="1.6"
      stroke-linecap="round"
      stroke-linejoin="round"
      aria-hidden="true"
    >
      <path d="M7 3v4m10-4v4M3 11h18M7 16h3m4 0h3M6 5h12a3 3 0 0 1 3 3v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a3 3 0 0 1 3-3"></path>
    </svg>
  );
};

import Link from "next/link";
import { SpaceForm } from "@/src/components/advertiser/Forms";

export default function NewSpacePage() {
  return <div className="pb-8"><Link href="/advertiser" className="text-sm font-bold text-brand hover:underline">← Meus espaços</Link>
    <p className="mt-9 text-[10px] font-extrabold tracking-[.15em] text-brand">SEU ESPAÇO, SEU JEITO</p>
    <h1 className="mt-3 text-[32px] font-extrabold tracking-tight sm:text-[40px]">Novo espaço</h1>
    <p className="mt-2 mb-8 text-sm text-muted">Conte o que torna esse lugar especial.</p><SpaceForm />
  </div>;
}

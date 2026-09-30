import Link from "next/link";

export default function SpaceNotFound() {
  return <main className="mx-auto max-w-lg px-6 py-20 text-center text-ink"><h1 className="text-3xl font-extrabold">Espaço não encontrado</h1><p className="mt-4 text-muted">Este espaço não está disponível.</p><Link href="/" className="mt-6 inline-block rounded-xl bg-brand px-5 py-3 font-bold text-white">Explorar espaços</Link></main>;
}

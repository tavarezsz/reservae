import Link from "next/link";
import { notFound } from "next/navigation";
import { CoverForm, SpaceForm } from "@/src/components/advertiser/Forms";
import { ApiError, ownedSpace } from "@/src/lib/advertiser";
import { CoverImage } from "@/src/components/advertiser/CoverImage";

export default async function EditSpacePage({ params, searchParams }: { params: Promise<{ id: string }>; searchParams: Promise<{ created?: string }> }) {
  const id = Number((await params).id);
  if (!Number.isInteger(id) || id < 1) notFound();
  let space;
  try { space = await ownedSpace(id); } catch (error) { if (error instanceof ApiError && error.status === 404) notFound(); throw error; }
  const created = (await searchParams).created === "1";
  return <div className="pb-8"><Link href="/advertiser" className="text-sm font-bold text-brand hover:underline">← Meus espaços</Link>
    <p className="mt-9 text-[10px] font-extrabold tracking-[.15em] text-brand">SEU ESPAÇO, SEU JEITO</p>
    <h1 className="mt-3 text-[32px] font-extrabold tracking-tight sm:text-[40px]">Editar espaço</h1>
    <p className="mt-2 text-sm text-muted">{space.title}</p>
    {created && <p role="status" className="mt-5 rounded-xl bg-surface-soft p-4 text-sm font-semibold text-brand">Espaço criado. Agora você pode adicionar uma foto e configurar horários.</p>}
    <div className="mt-8 grid gap-9 xl:grid-cols-[minmax(0,1fr)_340px]">
      <SpaceForm space={space} />
      <aside className="space-y-5"><div className="relative flex aspect-[1.6] items-center justify-center overflow-hidden rounded-2xl bg-surface-hint text-sm text-muted"><CoverImage path={space.coverImagePath} title={space.title} sizes="340px" /></div><CoverForm spaceId={id} /><Link href={`/advertiser/spaces/${id}/schedule`} className="flex min-h-12 items-center justify-between rounded-xl border border-brand px-4 text-sm font-bold text-brand">Gerenciar horários <span>→</span></Link></aside>
    </div>
  </div>;
}

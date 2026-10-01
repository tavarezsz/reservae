import Link from "next/link";
import { notFound } from "next/navigation";
import type { BookableSlotDTO } from "@/lib/api/models";
import { SlotForm } from "@/src/components/advertiser/Forms";
import { ApiError, api, ownedSpace } from "@/src/lib/advertiser";

export default async function EditSlotPage({ params }: { params: Promise<{ id: string; slotId: string }> }) {
  const { id: rawSpaceId, slotId: rawSlotId } = await params;
  const spaceId = Number(rawSpaceId), slotId = Number(rawSlotId);
  if (!Number.isInteger(spaceId) || spaceId < 1 || !Number.isInteger(slotId) || slotId < 1) notFound();

  let space, slot;
  try {
    [space, slot] = await Promise.all([ownedSpace(spaceId), api<BookableSlotDTO>(`/api/bookable-slots/${slotId}`)]);
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) notFound();
    throw error;
  }
  if (slot.spaceId !== spaceId) notFound();

  return <div className="pb-8">
    <Link href={`/advertiser/spaces/${spaceId}/schedule`} className="text-sm font-bold text-brand hover:underline">← Horários de {space.title}</Link>
    <p className="mt-9 text-[10px] font-extrabold tracking-[.15em] text-brand">{slot.availabilityRuleId ? "HORÁRIO DE REGRA" : "HORÁRIO AVULSO"}</p>
    <h1 className="mt-3 text-[32px] font-extrabold tracking-tight sm:text-[40px]">Editar horário</h1>
    <p className="mt-2 mb-8 text-sm text-muted">{slot.startsAt?.slice(0, 10)} · {slot.startsAt?.slice(11, 16)}–{slot.endsAt?.slice(11, 16)}</p>
    <div className="max-w-3xl rounded-2xl border border-line bg-white p-5 sm:p-7"><SlotForm spaceId={spaceId} slot={slot} /></div>
  </div>;
}

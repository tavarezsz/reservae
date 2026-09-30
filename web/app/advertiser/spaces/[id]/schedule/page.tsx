import Link from "next/link";
import { notFound } from "next/navigation";
import type { BookableSlotDTO } from "@/lib/api/models";
import { RuleForm, SlotForm } from "@/src/components/advertiser/Forms";
import { ApiError, api, ownedSpace, spaceRules } from "@/src/lib/advertiser";

const weekdays = ["Segunda", "Terça", "Quarta", "Quinta", "Sexta", "Sábado", "Domingo"];

export default async function SchedulePage({ params, searchParams }: {
  params: Promise<{ id: string }>;
  searchParams: Promise<{ rule?: string; slot?: string }>;
}) {
  const id = Number((await params).id);
  if (!Number.isInteger(id) || id < 1) notFound();
  let space, rules;
  try { [space, rules] = await Promise.all([ownedSpace(id), spaceRules(id)]); }
  catch (error) { if (error instanceof ApiError && error.status === 404) notFound(); throw error; }
  const query = await searchParams;
  const selectedRule = query.rule ? rules.find(rule => rule.id === Number(query.rule)) : undefined;
  let selectedSlot: BookableSlotDTO | undefined;
  if (query.slot) {
    try {
      selectedSlot = await api<BookableSlotDTO>(`/api/bookable-slots/${Number(query.slot)}`);
      if (selectedSlot.spaceId !== id) notFound();
    } catch (error) { if (error instanceof ApiError && error.status === 404) notFound(); throw error; }
  }
  return <div className="pb-8">
    <Link href="/advertiser" className="text-sm font-bold text-brand hover:underline">← Meus espaços</Link>
    <p className="mt-9 text-[10px] font-extrabold tracking-[.15em] text-brand">DISPONIBILIDADE</p>
    <h1 className="mt-3 text-[32px] font-extrabold tracking-tight sm:text-[40px]">Horários de {space.title}</h1>
    <p className="mt-2 text-sm text-muted">Crie uma regra semanal ou um horário para uma data específica.</p>
    <div className="mt-7 flex flex-wrap gap-3"><Link href={`/advertiser/spaces/${id}`} className="rounded-xl border border-line-strong px-4 py-3 text-sm font-bold text-brand">Editar espaço</Link><Link href={`/advertiser/occupancy?space=${id}`} className="rounded-xl border border-line-strong px-4 py-3 text-sm font-bold text-brand">Ver ocupação</Link></div>
    <section className="mt-10" aria-labelledby="rules-title"><div className="mb-5 flex items-center justify-between gap-3"><h2 id="rules-title" className="text-xl font-extrabold">Regras recorrentes</h2><span className="text-xs text-muted">{rules.length} {rules.length === 1 ? "regra" : "regras"}</span></div>
      {rules.length === 0 ? <p className="rounded-xl border border-dashed border-line-strong p-5 text-sm text-muted">Ainda não há regras para este espaço.</p> : <div className="grid gap-4 md:grid-cols-2">{rules.map(rule => <article key={rule.id} className="rounded-2xl border border-line bg-white p-5"><div className="flex items-center justify-between gap-2"><span className="text-[10px] font-extrabold uppercase tracking-wider text-brand">{weekdays[rule.dayOfTheWeek ?? 0]}</span><span className="rounded-full bg-surface-soft px-3 py-1 text-[10px] font-bold text-brand">{rule.isActive ? "Ativa" : "Inativa"}</span></div><h3 className="mt-3 text-lg font-extrabold">{rule.startTime?.slice(0, 5)}–{rule.endTime?.slice(0, 5)}</h3><p className="mt-2 text-xs leading-5 text-muted">{rule.slotDurationMinutes} min por horário · {rule.capacity} vagas</p><p className="mt-1 text-xs text-muted">{rule.validFrom?.slice(0, 10)} até {rule.validUntil?.slice(0, 10)}</p><Link href={`?rule=${rule.id}`} className="mt-4 inline-block text-xs font-bold text-brand hover:underline">Editar regra →</Link></article>)}</div>}
    </section>
    <div className="mt-10 grid gap-10 xl:grid-cols-2">
      <section id="rule-form" className="rounded-2xl border border-line bg-white p-5 sm:p-7"><h2 className="mb-2 text-xl font-extrabold">{selectedRule ? "Editar regra" : "Adicionar regra semanal"}</h2><p className="mb-6 text-xs leading-5 text-muted">A regra gera horários virtuais até sua data final.</p><RuleForm key={selectedRule?.id ?? "new-rule"} spaceId={id} rule={selectedRule} /></section>
      <section id="slot-form" className="rounded-2xl border border-line bg-white p-5 sm:p-7"><h2 className="mb-2 text-xl font-extrabold">{selectedSlot ? "Editar horário" : "Horário avulso"}</h2><p className="mb-6 text-xs leading-5 text-muted">Use para uma data específica, sem criar uma regra recorrente. Datas e horas seguem o horário (UTC).</p><SlotForm key={selectedSlot?.id ?? "new-slot"} spaceId={id} slot={selectedSlot} /></section>
    </div>
  </div>;
}

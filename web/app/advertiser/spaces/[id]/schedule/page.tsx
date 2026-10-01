import Link from "next/link";
import { notFound } from "next/navigation";
import { ScheduleCreatePanel } from "@/src/components/advertiser/ScheduleCreatePanel";
import { ApiError, ownedSpace, spaceRules, standaloneSlots } from "@/src/lib/advertiser";

const weekdays = ["Segunda", "Terça", "Quarta", "Quinta", "Sexta", "Sábado", "Domingo"];
const dateLabel = (value?: string) => value?.slice(0, 10).split("-").reverse().join("/") ?? "—";

export default async function SchedulePage({ params }: { params: Promise<{ id: string }> }) {
  const id = Number((await params).id);
  if (!Number.isInteger(id) || id < 1) notFound();

  let space, rules, slots;
  try {
    [space, rules, slots] = await Promise.all([ownedSpace(id), spaceRules(id), standaloneSlots(id)]);
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) notFound();
    throw error;
  }

  return <div className="pb-8">
    <Link href="/advertiser" className="text-sm font-bold text-brand hover:underline">← Meus espaços</Link>
    <p className="mt-9 text-[10px] font-extrabold tracking-[.15em] text-brand">DISPONIBILIDADE</p>
    <h1 className="mt-3 text-[32px] font-extrabold tracking-tight sm:text-[40px]">Horários de {space.title}</h1>
    <p className="mt-2 text-sm text-muted">Defina sua agenda recorrente ou adicione horários para datas específicas.</p>
    <div className="mt-7 flex flex-wrap gap-3">
      <Link href={`/advertiser/spaces/${id}`} className="rounded-xl border border-line-strong px-4 py-3 text-sm font-bold text-brand">Editar espaço</Link>
      <Link href={`/advertiser/occupancy?space=${id}`} className="rounded-xl border border-line-strong px-4 py-3 text-sm font-bold text-brand">Ver ocupação</Link>
    </div>

    <div className="mt-10 max-w-3xl"><ScheduleCreatePanel spaceId={id} /></div>

    <section className="mt-12" aria-labelledby="rules-title">
      <div className="mb-5 flex items-center justify-between gap-3"><h2 id="rules-title" className="text-xl font-extrabold">Regras recorrentes</h2><span className="text-xs text-muted">{rules.length} {rules.length === 1 ? "regra" : "regras"}</span></div>
      {rules.length === 0 ? <p className="rounded-xl border border-dashed border-line-strong p-5 text-sm text-muted">Ainda não há regras para este espaço.</p> :
        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">{rules.map(rule => <article key={rule.id} className="rounded-2xl border border-line bg-white p-5">
          <div className="flex items-center justify-between gap-2"><span className="text-[10px] font-extrabold uppercase tracking-wider text-brand">{weekdays[rule.dayOfTheWeek ?? 0]}</span><span className="rounded-full bg-surface-soft px-3 py-1 text-[10px] font-bold text-brand">{rule.isActive ? "Ativa" : "Inativa"}</span></div>
          <h3 className="mt-3 text-lg font-extrabold">{rule.startTime?.slice(0, 5)}–{rule.endTime?.slice(0, 5)}</h3>
          <p className="mt-2 text-xs leading-5 text-muted">{rule.slotDurationMinutes} min por horário · {rule.capacity} vagas</p>
          <p className="mt-1 text-xs text-muted">{dateLabel(rule.validFrom)} até {dateLabel(rule.validUntil)}</p>
          <Link href={`/advertiser/spaces/${id}/schedule/rules/${rule.id}`} className="mt-5 inline-flex min-h-10 items-center text-xs font-bold text-brand hover:underline">Editar regra →</Link>
        </article>)}</div>}
    </section>

    <section className="mt-12" aria-labelledby="standalone-title">
      <div className="mb-5 flex items-center justify-between gap-3"><h2 id="standalone-title" className="text-xl font-extrabold">Horários avulsos</h2><span className="text-xs text-muted">{slots.length} {slots.length === 1 ? "horário" : "horários"}</span></div>
      {slots.length === 0 ? <p className="rounded-xl border border-dashed border-line-strong p-5 text-sm text-muted">Ainda não há horários avulsos para este espaço.</p> :
        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">{slots.map(slot => <article key={slot.id} className="rounded-2xl border border-line bg-white p-5">
          <div className="flex items-center justify-between gap-2"><span className="text-[10px] font-extrabold uppercase tracking-wider text-brand">{dateLabel(slot.startsAt)}</span><span className="rounded-full bg-surface-soft px-3 py-1 text-[10px] font-bold text-brand">{slot.isActive ? "Ativo" : "Inativo"}</span></div>
          <h3 className="mt-3 text-lg font-extrabold">{slot.startsAt?.slice(11, 16)}–{slot.endsAt?.slice(11, 16)}</h3>
          <p className="mt-2 text-xs leading-5 text-muted">{slot.capacity} vagas · {slot.reservedQuantity ?? 0} reservadas</p>
          <Link href={`/advertiser/spaces/${id}/schedule/slots/${slot.id}`} className="mt-5 inline-flex min-h-10 items-center text-xs font-bold text-brand hover:underline">Editar horário →</Link>
        </article>)}</div>}
    </section>
  </div>;
}

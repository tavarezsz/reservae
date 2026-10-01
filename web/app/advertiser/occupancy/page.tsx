import Link from "next/link";
import type { AvailableSlotDto, BookingDto, SpaceDTO } from "@/lib/api/models";
import { advertiserUserId, ownerSpaces, spaceBookings, spaceSlots } from "@/src/lib/advertiser";
import { dateKey } from "@/src/lib/availability";

type Entry = { space: SpaceDTO; slot: AvailableSlotDto };

export default async function OccupancyPage({ searchParams }: { searchParams: Promise<{ space?: string; date?: string; booked?: string; slot?: string }> }) {
  const query = await searchParams;
  const today = dateKey(new Date());
  const date = query.date && /^\d{4}-\d{2}-\d{2}$/.test(query.date) ? query.date : today;
  const spaces = await ownerSpaces().catch(() => null);
  const selectedSpace = query.space && spaces?.some(space => space.id === Number(query.space)) ? Number(query.space) : null;
  const selected = spaces?.filter(space => selectedSpace === null || space.id === selectedSpace) ?? [];
  let entries: Entry[] | null = null;
  if (spaces) {
    try {
      const results = await Promise.all(selected.map(async space => ({ space, slots: await spaceSlots(space.id!, date) })));
      entries = results.flatMap(({ space, slots }) => slots.map(slot => ({ space, slot })));
      entries.sort((a, b) => (a.slot.startsAt ?? "").localeCompare(b.slot.startsAt ?? ""));
    } catch { entries = null; }
  }
  const visible = entries?.filter(({ slot }) => query.booked !== "1" || (slot.reservedQuantity ?? 0) > 0) ?? [];
  const reserved = entries?.reduce((sum, { slot }) => sum + (slot.reservedQuantity ?? 0), 0) ?? 0;
  const full = entries?.filter(({ slot }) => (slot.availableQuantity ?? 0) === 0).length ?? 0;
  const chosen = query.slot ? entries?.find(({ slot }) => slot.bookableSlotId === Number(query.slot)) : undefined;
  let bookings: BookingDto[] = [];
  if (chosen?.space.id) bookings = await spaceBookings(chosen.space.id).catch(() => []);
  const selectedBookings = bookings.filter(booking => booking.bookableSlotId === chosen?.slot.bookableSlotId && booking.status === 0);
  const baseQuery = new URLSearchParams({ date, ...(selectedSpace ? { space: String(selectedSpace) } : {}), ...(query.booked === "1" ? { booked: "1" } : {}) });
  return <div className="pb-8">
    <p className="text-[10px] font-extrabold tracking-[.15em] text-brand">RESERVAS RECEBIDAS</p>
    <h1 className="mt-3 text-[32px] font-extrabold tracking-tight sm:text-[40px]">Ocupação</h1>
    <p className="mt-2 text-sm text-muted">Veja quem reservou e quais horários estão preenchidos.</p>
    <form method="GET" className="mt-8 grid max-w-2xl gap-4 sm:grid-cols-2">
      <div><label htmlFor="space" className="text-sm font-bold">Espaço</label><select id="space" name="space" defaultValue={selectedSpace ?? ""} className="mt-2 min-h-12 w-full rounded-xl border border-line bg-white px-4 text-sm"><option value="">Todos os espaços</option>{spaces?.map(space => <option key={space.id} value={space.id}>{space.title}</option>)}</select></div>
      <div><label htmlFor="date" className="text-sm font-bold">Data</label><input id="date" name="date" type="date" defaultValue={date} className="mt-2 min-h-12 w-full rounded-xl border border-line bg-white px-4 text-sm" /></div>
      <label className="flex items-center gap-3 text-sm sm:col-span-2"><input type="checkbox" name="booked" value="1" defaultChecked={query.booked === "1"} className="size-5 accent-brand" />Somente horários com reservas</label>
      <button className="min-h-11 rounded-xl bg-brand px-5 text-sm font-bold text-white sm:w-fit">Aplicar filtros</button>
    </form>
    {!advertiserUserId && <p role="alert" className="mt-8 rounded-xl bg-error-surface p-5 text-sm text-error">Configure o usuário de desenvolvimento para ver a ocupação.</p>}
    {advertiserUserId && !entries && <p role="alert" className="mt-8 rounded-xl bg-error-surface p-5 text-sm text-error">Não foi possível carregar a agenda. Atualize a página para tentar novamente.</p>}
    {entries && <>
      <div className="mt-8 flex max-w-2xl gap-7 rounded-2xl bg-ink p-6 text-white"><div><strong className="text-2xl text-brand-accent">{reserved}</strong><p className="mt-1 text-xs">vagas reservadas</p></div><div className="border-l border-dark-line pl-7"><strong className="text-2xl text-brand-accent">{full}</strong><p className="mt-1 text-xs">horário(s) lotado(s)</p></div></div>
      <div className="mt-8 flex items-center justify-between"><h2 className="text-xl font-extrabold">{new Date(`${date}T12:00:00`).toLocaleDateString("pt-BR", { day: "numeric", month: "short", year: "numeric" })}</h2><span className="text-xs text-muted">{visible.length} horários</span></div>
      {visible.length === 0 ? <div className="mt-5 rounded-2xl border border-dashed border-line-strong bg-white p-7 text-sm text-muted">Nenhum horário encontrado para esses filtros.</div> : <div className="mt-5 grid gap-4 md:grid-cols-2 xl:grid-cols-3">{visible.map(({ space, slot }, index) => {
        const count = slot.reservedQuantity ?? 0, capacity = slot.capacity ?? 0;
        const status = count === 0 ? "Livre" : count >= capacity ? "Lotado" : "Parcial";
        return <article key={`${space.id}:${slot.startsAt}:${index}`} className="rounded-2xl border border-line bg-white p-5">
          <span className="text-[10px] font-extrabold uppercase tracking-wider text-brand">{space.title}</span>
          <div className="mt-2 flex items-center justify-between gap-2"><h3 className="text-lg font-extrabold">{slot.startsAt?.slice(11, 16)}–{slot.endsAt?.slice(11, 16)}</h3><span className={`rounded-full px-3 py-1 text-[10px] font-bold ${status === "Lotado" ? "bg-brand text-white" : "bg-surface-soft text-brand"}`}>{status}</span></div>
          <p className="mt-2 text-xs text-muted">{slot.isVirtual ? "Recorrente" : slot.availabilityRuleId ? "Recorrente" : "Avulso"} · {capacity} vagas</p>
          <div className="mt-5 h-1.5 overflow-hidden rounded-full bg-surface-soft"><div className="h-full bg-brand" style={{ width: `${capacity ? Math.min(100, count / capacity * 100) : 0}%` }} /></div>
          <div className="mt-2 flex justify-between text-[11px]"><strong className="text-brand">{count} reservadas</strong><span className="text-muted">{slot.availableQuantity ?? capacity - count} livres</span></div>
          <div className="mt-5 flex justify-between gap-2 border-t border-line pt-4 text-xs font-bold"><Link href={slot.bookableSlotId ? `/advertiser/spaces/${space.id}/schedule/slots/${slot.bookableSlotId}` : `/advertiser/spaces/${space.id}/schedule/rules/${slot.availabilityRuleId}`} className="text-muted hover:text-brand">{slot.bookableSlotId ? "Editar horário" : "Editar regra"}</Link>{slot.bookableSlotId && count > 0 && <Link href={`/advertiser/occupancy?${baseQuery}&slot=${slot.bookableSlotId}`} className="text-brand hover:underline">Ver reservas →</Link>}</div>
        </article>;
      })}</div>}
    </>}
    {chosen && <div className="fixed inset-0 z-50 flex items-end justify-center bg-overlay p-0 sm:items-center sm:p-6"><div className="max-h-[90vh] w-full max-w-lg overflow-auto rounded-t-3xl bg-canvas p-6 shadow-xl sm:rounded-3xl sm:p-8" role="dialog" aria-modal="true" aria-labelledby="bookings-title"><div className="flex items-center justify-between"><h2 id="bookings-title" className="text-xl font-extrabold">Reservas do horário</h2><Link href={`/advertiser/occupancy?${baseQuery}`} aria-label="Fechar" className="rounded-full bg-surface-soft px-3 py-2">✕</Link></div><p className="mt-5 text-[10px] font-extrabold uppercase tracking-wider text-brand">{chosen.space.title}</p><p className="mt-2 text-lg font-extrabold">{date} · {chosen.slot.startsAt?.slice(11, 16)}–{chosen.slot.endsAt?.slice(11, 16)}</p><p className="mt-4 text-sm text-muted">{chosen.slot.reservedQuantity} de {chosen.slot.capacity} vagas reservadas</p><ul className="mt-6 divide-y divide-line">{selectedBookings.length ? selectedBookings.map(booking => <li key={booking.id} className="flex justify-between gap-4 py-4 text-sm"><span className="font-bold">{booking.userBookedName}</span><span className="text-muted">{booking.quantity} {booking.quantity === 1 ? "vaga" : "vagas"} · {booking.status === 0 ? "Confirmada" : "Cancelada"}</span></li>) : <li className="py-4 text-sm text-muted">Nenhuma reserva encontrada.</li>}</ul><Link href={`/advertiser/occupancy?${baseQuery}`} className="mt-6 block rounded-xl bg-brand px-5 py-4 text-center text-sm font-bold text-white">Voltar à agenda</Link></div></div>}
  </div>;
}

"use client";

import { useEffect, useRef, useState, type FormEvent } from "react";
import type { AvailableSlotDto, SpaceDTO } from "@/lib/api/models";
import { getApiSpacesSpaceIdAvailability } from "@/lib/api/space/space";
import { getApiSpacesSpaceIdAvailabilityLimit } from "@/lib/api/space/space";
import { postApiBookingsCreateAuto } from "@/lib/api/booking/booking";
import { Icon } from "./atoms/Icon";
import { addDays, canBook, dateKey, dateLabel, money, parseDate, shortDate, slotDate, slotKey, timeLabel, weekStart } from "@/src/lib/availability";
import { AvailabilityCalendar } from "./AvailabilityCalendar";

type Period = { anchor: string; monday: string; slots: AvailableSlotDto[] };
const weekdays = ["SEG", "TER", "QUA", "QUI", "SEX", "SÁB", "DOM"];
const SLOTS_PER_PAGE = 10;

export function SpaceAvailability({ space, spaceId }: { space: SpaceDTO; spaceId: number }) {
  const [request, setRequest] = useState({ date: "", revision: 0 });
  const [period, setPeriod] = useState<Period | null>(null);
  const [selectedDay, setSelectedDay] = useState("");
  const [selectedSlot, setSelectedSlot] = useState<string | null>(null);
  const [slotPage, setSlotPage] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);
  const [bookingOpen, setBookingOpen] = useState(false);
  const [calendarOpen, setCalendarOpen] = useState(false);
  const [limit, setLimit] = useState<{ validUntil: string | null; loading: boolean; error: boolean }>({ validUntil: null, loading: true, error: false });
  const [limitRevision, setLimitRevision] = useState(0);

  useEffect(() => {
    const controller = new AbortController();
    getApiSpacesSpaceIdAvailabilityLimit(spaceId, { signal: controller.signal }).then((result) => {
      if (!controller.signal.aborted) setLimit({ validUntil: result.validUntil ?? null, loading: false, error: false });
    }).catch(() => {
      if (!controller.signal.aborted) setLimit({ validUntil: null, loading: false, error: true });
    });
    return () => controller.abort();
  }, [spaceId, limitRevision]);

  useEffect(() => {
    const controller = new AbortController();
    const anchor = request.date || dateKey(new Date());
    const monday = weekStart(anchor);
    getApiSpacesSpaceIdAvailability(spaceId, {
      fromDate: addDays(monday, -1),
      toDate: addDays(monday, 14),
    }, { signal: controller.signal }).then((slots) => {
      if (controller.signal.aborted) return;
      setPeriod({ anchor, monday, slots });
      setSelectedDay(anchor);
      setSelectedSlot(null);
      setSlotPage(0);
      setError(false);
    }).catch(() => {
      if (!controller.signal.aborted) setError(true);
    }).finally(() => {
      if (!controller.signal.aborted) setLoading(false);
    });
    return () => controller.abort();
  }, [spaceId, request]);

  function chooseDate(date: string) {
    if (!date || date < dateKey(new Date()) || (limit.validUntil && date > limit.validUntil)) return;
    setSelectedSlot(null);
    setSlotPage(0);
    if (!period || weekStart(date) !== period.monday) {
      setLoading(true);
      setError(false);
      setRequest((previous) => ({ date, revision: previous.revision + 1 }));
    } else setSelectedDay(date);
  }

  function retry() {
    setLoading(true);
    setError(false);
    setRequest((previous) => ({ ...previous, revision: previous.revision + 1 }));
  }

  const today = dateKey(new Date());
  const monday = period?.monday ?? weekStart(today);
  const days = Array.from({ length: 7 }, (_, index) => addDays(monday, index));
  const day = selectedDay || period?.anchor || today;
  const slotsForDay = (period?.slots ?? []).filter((slot) => slotDate(slot) === day).sort((a, b) => (a.startsAt ?? "").localeCompare(b.startsAt ?? ""));
  const bookableSlots = slotsForDay.filter(canBook);
  const pageCount = Math.ceil(bookableSlots.length / SLOTS_PER_PAGE);
  const currentPage = Math.min(slotPage, Math.max(0, pageCount - 1));
  const visibleSlots = bookableSlots.slice(currentPage * SLOTS_PER_PAGE, (currentPage + 1) * SLOTS_PER_PAGE);
  const selected = bookableSlots.find((slot) => slotKey(slot) === selectedSlot);
  const futureDays = period ? Array.from(new Set(period.slots.filter(canBook).map(slotDate))).filter((value) => value > addDays(monday, 6) && (!limit.validUntil || value <= limit.validUntil)).slice(0, 2) : [];

  return (
    <>
      <section className="min-w-0 overflow-hidden rounded-3xl border border-line bg-surface-soft xl:col-start-2 xl:row-span-2 xl:row-start-1" aria-labelledby="availability-title">
        <div className="p-5 sm:p-6">
          <h2 id="availability-title" className="text-[24px] font-extrabold tracking-tight">Escolha seu horário</h2>
          <div className="mt-5 flex items-center justify-between gap-2 text-xs"><strong>{monday === weekStart(today) ? "Nesta semana" : "Semana escolhida"}</strong><span className="text-muted">{shortDate(monday)} – {shortDate(addDays(monday, 6))}</span></div>
          <div className="mt-4 grid grid-cols-7 gap-1 sm:gap-2" role="group" aria-label="Escolher dia">
            {days.map((date, index) => {
              const count = period?.slots.filter((slot) => slotDate(slot) === date && canBook(slot)).length ?? 0;
              return <button key={date} type="button" disabled={date < today || loading || (limit.validUntil != null && date > limit.validUntil)} aria-label={dateLabel(date)} aria-pressed={day === date} onClick={() => chooseDate(date)} className={`flex min-w-0 flex-col items-center rounded-xl border py-2 text-center transition focus-visible:outline-2 focus-visible:outline-focus ${day === date ? "border-brand bg-brand text-white" : "border-line bg-white text-ink hover:border-line-strong"} disabled:opacity-45`}><span className="text-[9px] font-bold">{weekdays[index]}</span><strong className="mt-1 text-base">{parseDate(date).getDate()}</strong><span className="h-3 text-xs">{count ? "•" : "–"}</span></button>;
            })}
          </div>
          <div className="mt-6"><h3 className="text-sm font-extrabold capitalize">{dateLabel(day)}</h3><p className="mt-2 text-xs text-muted">Horários locais · Escolha uma faixa disponível</p></div>
          {loading && <p role="status" className="py-10 text-center text-sm text-muted">Carregando horários…</p>}
          {error && <div role="alert" className="mt-5 rounded-xl bg-error-surface p-4 text-sm text-error">Não foi possível consultar os horários.<button type="button" onClick={retry} className="ml-2 font-bold underline">Tentar novamente</button></div>}
          {!loading && !error && <div id="slot-list" className="mt-5 space-y-2" role="group" aria-label="Horários disponíveis">{bookableSlots.length ? visibleSlots.map((slot) => <button key={slotKey(slot)} type="button" aria-pressed={selectedSlot === slotKey(slot)} onClick={() => setSelectedSlot(slotKey(slot))} className={`flex min-h-20 w-full items-center justify-between gap-2 rounded-xl border bg-white p-4 text-left transition focus-visible:outline-2 focus-visible:outline-focus ${selectedSlot === slotKey(slot) ? "border-brand ring-1 ring-brand" : "border-line hover:border-line-strong"}`}><span className="min-w-0"><strong className="block text-sm">{timeLabel(slot.startsAt!)} – {timeLabel(slot.endsAt!)}</strong><small className="mt-2 block text-[11px] text-muted">{slot.availableQuantity} {slot.availableQuantity === 1 ? "vaga disponível" : "vagas disponíveis"}</small></span><span className="ml-auto shrink-0 text-right"><strong className="block text-sm">{money(slot.pricePerSpot ?? space.pricePerSpot ?? 0)}</strong><small className="text-[11px] text-muted">por vaga</small></span><span aria-hidden="true" className={`size-4 shrink-0 rounded-full border ${selectedSlot === slotKey(slot) ? "border-brand bg-brand" : "border-line"}`} /></button>) : <div className="rounded-xl bg-white px-4 py-8 text-center"><Icon name="calendar" className="mx-auto text-muted" /><h4 className="mt-3 text-sm font-bold">Sem horários neste dia</h4><p className="mt-2 text-xs text-muted">Escolha outra data para continuar.</p></div>}</div>}
          {!loading && !error && pageCount > 1 && <nav className="mt-4 flex items-center justify-between gap-3" aria-label="Páginas de horários">
            <span className="text-xs text-muted" aria-live="polite">{currentPage * SLOTS_PER_PAGE + 1}–{Math.min((currentPage + 1) * SLOTS_PER_PAGE, bookableSlots.length)} de {bookableSlots.length} horários</span>
            <div className="flex items-center gap-2">
              <button type="button" aria-label="Página anterior de horários" aria-controls="slot-list" disabled={currentPage === 0} onClick={() => { setSlotPage(currentPage - 1); setSelectedSlot(null); }} className="flex size-11 items-center justify-center rounded-xl border border-line bg-white text-brand hover:border-line-strong disabled:cursor-not-allowed disabled:opacity-40 focus-visible:outline-2 focus-visible:outline-focus"><Icon name="arrow" className="size-4 rotate-180" /></button>
              <button type="button" aria-label="Próxima página de horários" aria-controls="slot-list" disabled={currentPage >= pageCount - 1} onClick={() => { setSlotPage(currentPage + 1); setSelectedSlot(null); }} className="flex size-11 items-center justify-center rounded-xl border border-line bg-white text-brand hover:border-line-strong disabled:cursor-not-allowed disabled:opacity-40 focus-visible:outline-2 focus-visible:outline-focus"><Icon name="arrow" className="size-4" /></button>
            </div>
          </nav>}
          <p className="mt-4 flex items-center gap-2 rounded-xl bg-surface-hint p-3 text-[11px] text-brand"><Icon name="calendar" className="size-4" />Cada reserva vale para a faixa escolhida.</p>
          <p className="mt-4 text-[11px] text-muted">Preço informativo. Sem pagamento no app.</p>
        </div>
        <div className="hidden border-t border-line bg-white p-5 sm:p-6 xl:block"><BookingAction slot={selected} space={space} onOpen={() => setBookingOpen(true)} /></div>
      </section>
      <section className="min-w-0 xl:col-start-1 xl:row-start-2" aria-labelledby="future-title">
        <h2 id="future-title" className="text-[24px] font-extrabold tracking-tight">Planejando mais à frente?</h2>
        <p className="mt-3 text-sm leading-6 text-muted">Consulte os próximos dias ou escolha uma data no calendário.</p>
        <button type="button" disabled={limit.loading || limit.error || !limit.validUntil} onClick={() => setCalendarOpen(true)} className="mt-5 flex min-h-14 w-full items-center justify-between gap-3 rounded-xl border border-line bg-white p-4 text-left text-sm font-bold hover:border-line-strong disabled:text-disabled-text focus-visible:outline-2 focus-visible:outline-focus"><span className="flex items-center gap-3"><Icon name="calendar" className="text-brand" />Escolher outra data</span><Icon name="arrow" className="size-4" /></button>
        {limit.loading && <p role="status" className="mt-2 text-xs text-muted">Consultando o período disponível…</p>}
        {limit.error && <p role="alert" className="mt-2 text-xs text-error">Não foi possível consultar o calendário. <button type="button" onClick={() => { setLimit((previous) => ({ ...previous, loading: true, error: false })); setLimitRevision((value) => value + 1); }} className="font-bold underline">Tentar novamente</button></p>}
        {!limit.loading && !limit.error && !limit.validUntil && <p className="mt-2 text-xs text-muted">Não há datas futuras cadastradas para este espaço.</p>}
        <div className="mt-3 space-y-3">{futureDays.map((date) => <button key={date} type="button" onClick={() => chooseDate(date)} className="flex w-full items-center justify-between rounded-xl border border-line bg-white p-4 text-left text-sm hover:border-line-strong focus-visible:outline-2 focus-visible:outline-focus"><span className="font-bold capitalize">{shortDate(date)}</span><span className="text-xs text-brand">{period?.slots.filter((slot) => slotDate(slot) === date && canBook(slot)).length} horários →</span></button>)}</div>
      </section>
      <div className="fixed inset-x-0 bottom-0 z-20 border-t border-line bg-white p-4 shadow-[0_-8px_28px_#173e351a] xl:hidden"><BookingAction slot={selected} space={space} onOpen={() => setBookingOpen(true)} /></div>
      {calendarOpen && limit.validUntil && <AvailabilityCalendar spaceId={spaceId} initialDate={day <= limit.validUntil ? day : today} validUntil={limit.validUntil} onSelect={(date) => { setCalendarOpen(false); chooseDate(date); }} onClose={() => setCalendarOpen(false)} />}
      {bookingOpen && selected && <BookingDialog slot={selected} space={space} spaceId={spaceId} onClose={() => setBookingOpen(false)} onBooked={() => { setBookingOpen(false); retry(); }} />}
    </>
  );
}

function BookingAction({ slot, space, onOpen }: { slot: AvailableSlotDto | undefined; space: SpaceDTO; onOpen: () => void }) {
  return <div className="flex items-center justify-between gap-4"><div><p className="text-[10px] font-extrabold text-brand uppercase">{slot ? `${dateLabel(slotDate(slot))} · ${timeLabel(slot.startsAt!)}` : "Escolha uma faixa para continuar"}</p><strong className="mt-2 block text-xl font-extrabold">{money(slot ? slot.pricePerSpot ?? space.pricePerSpot ?? 0 : space.pricePerSpot ?? 0)}</strong><small className="text-[11px] text-muted">por vaga</small></div><button type="button" disabled={!slot} onClick={onOpen} className="flex min-h-12 items-center justify-between gap-3 rounded-xl bg-brand px-4 text-sm font-bold text-white hover:bg-brand-hover disabled:bg-disabled disabled:text-disabled-text focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-focus">Reservar horário<Icon name="arrow" /></button></div>;
}

function BookingDialog({ slot, space, spaceId, onClose, onBooked }: { slot: AvailableSlotDto; space: SpaceDTO; spaceId: number; onClose: () => void; onBooked: () => void }) {
  const dialog = useRef<HTMLDialogElement>(null);
  const sending = useRef(false);
  const [quantity, setQuantity] = useState(1);
  const [pending, setPending] = useState(false);
  const [error, setError] = useState("");
  const [bookingId, setBookingId] = useState<number | null>(null);
  const available = slot.availableQuantity ?? 0;
  const valid = quantity >= 1 && quantity <= available && Number.isInteger(quantity);

  useEffect(() => {
    const element = dialog.current;
    element?.showModal();
    return () => element?.close();
  }, []);

  async function confirm(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!valid || sending.current || !canBook(slot)) return;
    const userId = process.env.NEXT_PUBLIC_BOOKING_USER_ID;
    if (!userId) { setError("Configure o usuário de desenvolvimento antes de reservar."); return; }
    sending.current = true;
    setPending(true);
    setError("");
    try {
      const booking = await postApiBookingsCreateAuto({ userBookedId: userId, bookableSlotId: slot.bookableSlotId, availabilityRuleId: slot.availabilityRuleId, spaceId, startsAt: slot.startsAt, endsAt: slot.endsAt, quantity });
      setBookingId(booking.id ?? null);
    } catch (reason) {
      const detail = (reason as { info?: { detail?: string } }).info?.detail;
      setError(detail || "Não foi possível confirmar a reserva. Confira a disponibilidade e tente novamente.");
    } finally {
      sending.current = false;
      setPending(false);
    }
  }

  function close() {
    if (bookingId != null) onBooked();
    else onClose();
  }

  return <dialog ref={dialog} onCancel={(event) => { event.preventDefault(); if (!pending) close(); }} className="m-auto w-[calc(100%-2rem)] max-w-md rounded-3xl border border-line bg-white p-0 text-ink shadow-2xl backdrop:bg-overlay"><div className="p-6 sm:p-8"><div className="flex items-start justify-between gap-3"><h2 className="text-2xl font-extrabold">{bookingId != null ? "Reserva confirmada" : "Confirme sua reserva"}</h2><button type="button" disabled={pending} onClick={close} aria-label="Fechar" className="text-2xl leading-none text-muted">×</button></div>{bookingId != null ? <div role="status"><p className="mt-6 text-sm">Sua reserva foi criada para <strong>{space.title}</strong>.</p><p className="mt-2 text-sm text-muted">{dateLabel(slotDate(slot))} · {timeLabel(slot.startsAt!)} – {timeLabel(slot.endsAt!)}</p><p className="mt-3 text-sm text-muted">{quantity} {quantity === 1 ? "vaga" : "vagas"} · Reserva #{bookingId}</p><button type="button" onClick={close} className="mt-7 w-full rounded-xl bg-brand p-3 font-bold text-white">Voltar ao espaço</button></div> : <form onSubmit={confirm}><p className="mt-6 font-bold">{space.title}</p><p className="mt-2 text-sm capitalize text-muted">{dateLabel(slotDate(slot))} · {timeLabel(slot.startsAt!)} – {timeLabel(slot.endsAt!)}</p><div className="mt-7 flex items-center justify-between gap-3 border-y border-line py-5"><label htmlFor="booking-quantity" className="font-bold">Vagas<span className="block text-xs font-normal text-muted">{available} disponíveis</span></label><div className="flex items-center gap-2"><button type="button" disabled={pending || quantity <= 1} onClick={() => setQuantity((value) => value - 1)} aria-label="Diminuir vagas" className="size-9 rounded-lg border border-line disabled:opacity-40">−</button><input id="booking-quantity" type="number" min="1" max={available} required value={quantity || ""} onChange={(event) => setQuantity(Number(event.target.value))} className="w-12 text-center font-bold outline-none" /><button type="button" disabled={pending || quantity >= available} onClick={() => setQuantity((value) => value + 1)} aria-label="Aumentar vagas" className="size-9 rounded-lg border border-line disabled:opacity-40">+</button></div></div><p className="mt-6 flex items-center justify-between text-sm">Total estimado<strong className="text-xl">{money((slot.pricePerSpot ?? space.pricePerSpot ?? 0) * (valid ? quantity : 0))}</strong></p><p className="mt-2 text-xs text-muted">Sem pagamento no app.</p>{error && <p role="alert" className="mt-4 rounded-lg bg-error-surface p-3 text-sm text-error">{error}</p>}<button type="submit" disabled={pending || !valid} className="mt-7 w-full rounded-xl bg-brand p-3 font-bold text-white disabled:bg-disabled disabled:text-disabled-text">{pending ? "Confirmando…" : "Confirmar reserva"}</button></form>}</div></dialog>;
}

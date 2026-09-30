"use client";

import { useEffect, useRef, useState } from "react";
import { getApiSpacesSpaceIdAvailability } from "@/lib/api/space/space";
import { addDays, canBook, dateKey, parseDate, slotDate } from "@/src/lib/availability";
import { Icon } from "./atoms/Icon";

const monthNames = ["Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho", "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"];
const weekdays = ["S", "T", "Q", "Q", "S", "S", "D"];

export function AvailabilityCalendar({ spaceId, initialDate, validUntil, onSelect, onClose }: {
  spaceId: number;
  initialDate: string;
  validUntil: string;
  onSelect: (date: string) => void;
  onClose: () => void;
}) {
  const dialog = useRef<HTMLDialogElement>(null);
  const today = dateKey(new Date());
  const [selectedDate, setSelectedDate] = useState<string | null>(initialDate);
  const [month, setMonth] = useState(initialDate.slice(0, 7));
  const [availableDays, setAvailableDays] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);
  const [revision, setRevision] = useState(0);

  useEffect(() => {
    const element = dialog.current;
    element?.showModal();
    return () => element?.close();
  }, []);

  useEffect(() => {
    const controller = new AbortController();
    const first = `${month}-01`;
    const last = dateKey(new Date(Number(month.slice(0, 4)), Number(month.slice(5, 7)), 0));
    getApiSpacesSpaceIdAvailability(spaceId, {
      fromDate: addDays(first, -1),
      toDate: addDays(last, 1),
    }, { signal: controller.signal }).then((slots) => {
      if (controller.signal.aborted) return;
      setAvailableDays(Array.from(new Set(slots.filter(canBook).map(slotDate))));
      setError(false);
    }).catch(() => {
      if (!controller.signal.aborted) setError(true);
    }).finally(() => {
      if (!controller.signal.aborted) setLoading(false);
    });
    return () => controller.abort();
  }, [spaceId, month, revision]);

  const [year, monthNumber] = month.split("-").map(Number);
  const monthIndex = monthNumber - 1;
  const firstWeekday = (parseDate(`${month}-01`).getDay() + 6) % 7;
  const daysInMonth = new Date(year, monthNumber, 0).getDate();
  const minimumMonth = today.slice(0, 7);
  const maximumMonth = validUntil.slice(0, 7);

  function changeMonth(value: string) {
    if (value < minimumMonth || value > maximumMonth) return;
    setMonth(value);
    setSelectedDate(null);
    setLoading(true);
    setError(false);
  }

  function shiftMonth(amount: number) {
    const next = new Date(year, monthIndex + amount, 1);
    changeMonth(`${next.getFullYear()}-${String(next.getMonth() + 1).padStart(2, "0")}`);
  }

  function changeYear(value: number) {
    const target = `${value}-${String(monthNumber).padStart(2, "0")}`;
    changeMonth(target < minimumMonth ? minimumMonth : target > maximumMonth ? maximumMonth : target);
  }

  function confirm() {
    if (selectedDate && selectedDate >= today && selectedDate <= validUntil) onSelect(selectedDate);
  }

  return (
    <dialog
      ref={dialog}
      onCancel={(event) => { event.preventDefault(); onClose(); }}
      aria-labelledby="calendar-title"
      className="m-auto w-[calc(100%-2rem)] max-w-[390px] rounded-[26px] border border-line bg-canvas p-0 text-ink shadow-2xl backdrop:bg-overlay"
    >
      <div className="mx-auto mt-3 h-1 w-9 rounded-full bg-line" aria-hidden="true" />
      <div className="px-6 pt-4 pb-7">
        <div className="flex items-start justify-between gap-3">
          <h2 id="calendar-title" className="text-[23px] font-extrabold tracking-tight">Escolha uma data</h2>
          <button type="button" onClick={onClose} aria-label="Fechar calendário" className="flex size-10 shrink-0 items-center justify-center rounded-full bg-surface-soft text-ink focus-visible:outline-2 focus-visible:outline-focus">×</button>
        </div>
        <div className="mt-5 flex items-center justify-between gap-2">
          <div className="flex min-w-0 items-center gap-1 text-sm font-extrabold">
            <select aria-label="Mês" value={monthIndex} onChange={(event) => changeMonth(`${year}-${String(Number(event.target.value) + 1).padStart(2, "0")}`)} className="max-w-[140px] appearance-none rounded-lg bg-transparent py-2 pr-1 focus-visible:outline-2 focus-visible:outline-focus">
              {monthNames.map((name, index) => <option key={name} value={index} disabled={`${year}-${String(index + 1).padStart(2, "0")}` < minimumMonth || `${year}-${String(index + 1).padStart(2, "0")}` > maximumMonth}>{name}</option>)}
            </select>
            <select aria-label="Ano" value={year} onChange={(event) => changeYear(Number(event.target.value))} className="appearance-none rounded-lg bg-transparent py-2 focus-visible:outline-2 focus-visible:outline-focus">
              {Array.from({ length: Number(maximumMonth.slice(0, 4)) - Number(minimumMonth.slice(0, 4)) + 1 }, (_, index) => Number(minimumMonth.slice(0, 4)) + index).map((value) => <option key={value} value={value}>{value}</option>)}
            </select>
          </div>
          <div className="flex shrink-0 gap-1">
            <button type="button" disabled={month <= minimumMonth} onClick={() => shiftMonth(-1)} aria-label="Mês anterior" className="flex size-9 items-center justify-center rounded-lg text-brand hover:bg-surface-soft disabled:text-disabled-text focus-visible:outline-2 focus-visible:outline-focus"><Icon name="arrow" className="size-4 rotate-180" /></button>
            <button type="button" disabled={month >= maximumMonth} onClick={() => shiftMonth(1)} aria-label="Próximo mês" className="flex size-9 items-center justify-center rounded-lg text-brand hover:bg-surface-soft disabled:text-disabled-text focus-visible:outline-2 focus-visible:outline-focus"><Icon name="arrow" className="size-4" /></button>
          </div>
        </div>
        <div className="mt-4 grid grid-cols-7 gap-y-1 text-center" aria-hidden="true">{weekdays.map((label, index) => <span key={index} className="py-2 text-[11px] text-muted">{label}</span>)}</div>
        <div className="grid grid-cols-7 gap-y-1" role="group" aria-label={`${monthNames[monthIndex]} de ${year}`}>
          {Array.from({ length: firstWeekday }, (_, index) => <span key={`empty-${index}`} />)}
          {Array.from({ length: daysInMonth }, (_, index) => {
            const date = `${month}-${String(index + 1).padStart(2, "0")}`;
            const enabled = date >= today && date <= validUntil;
            const active = selectedDate === date;
            return <button key={date} type="button" disabled={!enabled} aria-label={parseDate(date).toLocaleDateString("pt-BR", { day: "numeric", month: "long", year: "numeric" })} aria-pressed={active} onClick={() => setSelectedDate(date)} className={`mx-auto flex size-10 flex-col items-center justify-center rounded-xl text-xs font-bold focus-visible:outline-2 focus-visible:outline-focus ${active ? "bg-brand text-white" : "text-ink hover:bg-surface-soft"} disabled:text-disabled-muted disabled:hover:bg-transparent`}><span>{index + 1}</span><span aria-hidden="true" className="h-1 text-[10px] leading-none">{enabled && availableDays.includes(date) ? "•" : ""}</span></button>;
          })}
        </div>
        <p role="status" className="mt-4 min-h-5 text-[11px] text-muted">{loading ? "Consultando horários…" : error ? "Não foi possível consultar os dias disponíveis." : "• Dias com horários disponíveis"}</p>
        {error && <button type="button" onClick={() => { setLoading(true); setError(false); setRevision((value) => value + 1); }} className="mt-1 text-xs font-bold text-brand underline">Tentar novamente</button>}
        <button type="button" onClick={confirm} disabled={!selectedDate || selectedDate < today || selectedDate > validUntil} className="mt-5 flex min-h-12 w-full items-center justify-between rounded-xl bg-brand px-5 text-sm font-bold text-white hover:bg-brand-hover disabled:bg-disabled disabled:text-disabled-text focus-visible:outline-2 focus-visible:outline-focus">{selectedDate ? `Ver horários de ${parseDate(selectedDate).toLocaleDateString("pt-BR", { day: "numeric", month: "long" })}` : "Escolha um dia para continuar"}<Icon name="arrow" /></button>
      </div>
    </dialog>
  );
}

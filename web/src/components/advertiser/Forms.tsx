"use client";

import { useActionState } from "react";
import type { AvailabilityRuleDto, BookableSlotDTO, SpaceDTO } from "@/lib/api/models";
import { categoryLabels } from "@/src/constants/CategoryLabels";
import { createRules, createSpace, saveSlot, updateRule, updateSpace, uploadCover, type FormState } from "@/app/advertiser/actions";

const emptyFormState: FormState = { values: {}, errors: {}, message: "", success: false, revision: 0 };

const inputClass = "mt-2 min-h-12 w-full rounded-xl border border-line bg-white px-4 text-sm text-ink outline-none focus:border-brand focus:ring-2 focus:ring-focus/25";
const labelClass = "block text-sm font-bold text-ink";

function Feedback({ state }: { state: FormState }) {
  if (!state.message) return null;
  return <p role={state.success ? "status" : "alert"} aria-live="polite" className={`rounded-xl p-4 text-sm ${state.success ? "bg-surface-soft text-brand" : "bg-error-surface text-error"}`}>{state.message}</p>;
}
function ErrorText({ state, name }: { state: FormState; name: string }) {
  return state.errors[name] ? <span className="mt-1 block text-xs text-error">{state.errors[name]}</span> : null;
}
function Submit({ pending, label }: { pending: boolean; label: string }) {
  return <button type="submit" disabled={pending} className="flex min-h-13 w-full items-center justify-between rounded-xl bg-brand px-5 text-sm font-extrabold text-white transition hover:bg-brand-hover disabled:opacity-60 sm:w-auto sm:min-w-60"><span>{pending ? "Salvando…" : label}</span><span aria-hidden="true">→</span></button>;
}

export function SpaceForm({ space }: { space?: SpaceDTO }) {
  const initial: FormState = { ...emptyFormState, values: space ? {
    title: space.title ?? "", address: space.address ?? "", description: space.description ?? "",
    category: String(space.category ?? 5), pricePerSpot: String(space.pricePerSpot ?? 0), isActive: space.isActive ? "on" : "",
  } : { category: "5", isActive: "on" } };
  const [rawState, action, pending] = useActionState(space?.id ? updateSpace.bind(null, space.id) : createSpace, initial);
  const state = { ...initial, ...rawState, values: rawState?.values ?? initial.values, errors: rawState?.errors ?? {} };
  const values = state.values;
  return <form action={action} key={state.revision} className="max-w-2xl space-y-6" noValidate>
    <Feedback state={state} />
    <div><label htmlFor="title" className={labelClass}>Nome do espaço</label><input id="title" name="title" required maxLength={200} defaultValue={values.title ?? ""} placeholder="Ex.: Casa Aurora" className={inputClass} aria-invalid={!!state.errors.title} /><ErrorText state={state} name="title" /></div>
    <div><label htmlFor="category" className={labelClass}>Categoria</label><select id="category" name="category" defaultValue={values.category ?? "5"} className={inputClass}>{Object.entries(categoryLabels).map(([key, label]) => <option key={key} value={key}>{label}</option>)}</select></div>
    <div><label htmlFor="address" className={labelClass}>Endereço</label><input id="address" name="address" required maxLength={500} defaultValue={values.address ?? ""} placeholder="Rua, número, bairro e cidade" className={inputClass} aria-invalid={!!state.errors.address} /><ErrorText state={state} name="address" /></div>
    <div><label htmlFor="description" className={labelClass}>Descrição</label><textarea id="description" name="description" required maxLength={4000} rows={5} defaultValue={values.description ?? ""} placeholder="Como é o espaço e para quais atividades ele serve?" className={`${inputClass} py-3`} aria-invalid={!!state.errors.description} /><ErrorText state={state} name="description" /></div>
    <div><label htmlFor="pricePerSpot" className={labelClass}>Preço padrão por vaga (R$){!space && <span className="font-normal text-muted"> · opcional</span>}</label><input id="pricePerSpot" name="pricePerSpot" type="number" min="0" step="0.01" defaultValue={values.pricePerSpot ?? ""} placeholder="Ex.: 35,00" className={inputClass} aria-invalid={!!state.errors.pricePerSpot} /><ErrorText state={state} name="pricePerSpot" /></div>
    {space && <label className="flex items-center gap-3 text-sm font-semibold"><input type="checkbox" name="isActive" defaultChecked={values.isActive === "on"} className="size-5 accent-brand" />Espaço ativo</label>}
    {!space && <p className="text-xs leading-5 text-muted">A foto de capa poderá ser enviada assim que o espaço for criado.</p>}
    <Submit pending={pending} label={space ? "Salvar alterações" : "Criar espaço"} />
  </form>;
}

export function CoverForm({ spaceId }: { spaceId: number }) {
  const [rawState, action, pending] = useActionState(uploadCover.bind(null, spaceId), emptyFormState);
  const state = { ...emptyFormState, ...rawState, values: rawState?.values ?? {}, errors: rawState?.errors ?? {} };
  return <form action={action} className="space-y-4">
    <Feedback state={state} />
    <label htmlFor="cover-file" className={labelClass}>Escolha uma foto de capa</label>
    <input id="cover-file" type="file" name="file" accept="image/jpeg,image/png,image/webp" required className="block w-full rounded-xl border border-dashed border-line-strong bg-surface-soft p-4 text-sm text-ink file:mr-4 file:rounded-lg file:border-0 file:bg-brand file:px-4 file:py-2 file:font-bold file:text-white" />
    <p className="text-xs text-muted">JPG, PNG ou WebP. A nova foto substitui a anterior.</p>
    <Submit pending={pending} label="Atualizar foto" />
  </form>;
}

const weekdays = ["Segunda", "Terça", "Quarta", "Quinta", "Sexta", "Sábado", "Domingo"];
const shortWeekdays = ["SEG", "TER", "QUA", "QUI", "SEX", "SÁB", "DOM"];

function RuleFields({ state }: { state: FormState }) {
  const values = state.values;
  return <>
    <div><label htmlFor="slotDurationMinutes" className={labelClass}>Duração de cada horário (min)</label><input id="slotDurationMinutes" name="slotDurationMinutes" type="number" min="30" step="1" required defaultValue={values.slotDurationMinutes ?? "60"} className={inputClass} /><ErrorText state={state} name="slotDurationMinutes" /></div>
    <div><label htmlFor="validFrom" className={labelClass}>A partir de</label><input id="validFrom" name="validFrom" type="date" required defaultValue={values.validFrom ?? ""} className={inputClass} /><ErrorText state={state} name="validFrom" /></div>
    <div><label htmlFor="validUntil" className={labelClass}>Até</label><input id="validUntil" name="validUntil" type="date" required defaultValue={values.validUntil ?? ""} className={inputClass} /><ErrorText state={state} name="validUntil" /></div>
    <div><label htmlFor="startTime" className={labelClass}>Início</label><input id="startTime" name="startTime" type="time" required defaultValue={values.startTime ?? ""} className={inputClass} /></div>
    <div><label htmlFor="endTime" className={labelClass}>Fim</label><input id="endTime" name="endTime" type="time" required defaultValue={values.endTime ?? ""} className={inputClass} /><ErrorText state={state} name="endTime" /></div>
    <div><label htmlFor="rule-capacity" className={labelClass}>Capacidade por horário</label><input id="rule-capacity" name="capacity" type="number" min="1" step="1" required defaultValue={values.capacity ?? ""} className={inputClass} /><ErrorText state={state} name="capacity" /></div>
    <div><label htmlFor="rule-price" className={labelClass}>Preço personalizado (R$) · opcional</label><input id="rule-price" name="customPricePerSpot" type="number" min="0" step="0.01" defaultValue={values.customPricePerSpot ?? ""} placeholder="Usar preço do espaço" className={inputClass} /><ErrorText state={state} name="customPricePerSpot" /></div>
    <label className="flex items-center gap-3 text-sm font-semibold sm:col-span-2"><input type="checkbox" name="isActive" defaultChecked={values.isActive === "on"} className="size-5 accent-brand" />Regra ativa</label>
  </>;
}

export function CreateRuleForm({ spaceId }: { spaceId: number }) {
  const initial: FormState = { ...emptyFormState, values: {
    day0: "on", day1: "on", day2: "on", day3: "on", day4: "on",
    slotDurationMinutes: "60", isActive: "on",
  } };
  const [rawState, action, pending] = useActionState(createRules.bind(null, spaceId), initial);
  const state = { ...initial, ...rawState, values: rawState?.values ?? initial.values, errors: rawState?.errors ?? {} };
  return <form action={action} key={state.revision} className="grid max-w-2xl gap-5 sm:grid-cols-2" noValidate>
    <div className="sm:col-span-2"><Feedback state={state} /></div>
    <fieldset className="sm:col-span-2">
      <legend className={labelClass}>Repetir toda semana</legend>
      <div className="mt-3 grid grid-cols-7 gap-1 sm:gap-2">{shortWeekdays.map((day, index) => <label key={day} className="cursor-pointer"><input type="checkbox" name={`day${index}`} defaultChecked={state.values[`day${index}`] === "on"} className="peer sr-only" /><span className="flex min-h-11 items-center justify-center rounded-lg border border-line text-[10px] font-extrabold text-ink transition peer-checked:border-brand peer-checked:bg-brand peer-checked:text-white peer-focus-visible:outline-2 peer-focus-visible:outline-offset-2 peer-focus-visible:outline-focus sm:text-xs">{day}</span></label>)}</div>
      <ErrorText state={state} name="days" />
    </fieldset>
    <RuleFields state={state} />
    <div className="sm:col-span-2"><Submit pending={pending} label="Adicionar regras" /></div>
  </form>;
}

export function EditRuleForm({ spaceId, rule }: { spaceId: number; rule: AvailabilityRuleDto }) {
  const initial: FormState = { ...emptyFormState, values: {
    dayOfTheWeek: String(rule.dayOfTheWeek ?? 0), startTime: rule.startTime?.slice(0, 5) ?? "", endTime: rule.endTime?.slice(0, 5) ?? "",
    validFrom: rule.validFrom?.slice(0, 10) ?? "", validUntil: rule.validUntil?.slice(0, 10) ?? "",
    capacity: String(rule.capacity ?? 1), slotDurationMinutes: String(rule.slotDurationMinutes ?? 60),
    customPricePerSpot: rule.customPricePerSpot == null ? "" : String(rule.customPricePerSpot), isActive: rule.isActive ? "on" : "",
  } };
  const [rawState, action, pending] = useActionState(updateRule.bind(null, spaceId, rule.id!), initial);
  const state = { ...initial, ...rawState, values: rawState?.values ?? initial.values, errors: rawState?.errors ?? {} };
  const values = state.values;
  return <form action={action} key={state.revision} className="grid max-w-2xl gap-5 sm:grid-cols-2" noValidate>
    <div className="sm:col-span-2"><Feedback state={state} /></div>
    <div className="sm:col-span-2"><label htmlFor="dayOfTheWeek" className={labelClass}>Dia da semana</label><select id="dayOfTheWeek" name="dayOfTheWeek" defaultValue={values.dayOfTheWeek ?? "0"} className={inputClass}>{weekdays.map((day, index) => <option key={day} value={index}>{day}</option>)}</select><ErrorText state={state} name="dayOfTheWeek" /></div>
    <RuleFields state={state} />
    <div className="sm:col-span-2"><Submit pending={pending} label="Salvar regra" /></div>
  </form>;
}

export function SlotForm({ spaceId, slot }: { spaceId: number; slot?: BookableSlotDTO }) {
  const initial: FormState = { ...emptyFormState, values: slot ? {
    startsAt: slot.startsAt?.slice(0, 16) ?? "", endsAt: slot.endsAt?.slice(0, 16) ?? "",
    capacity: String(slot.capacity ?? 1), customPricePerSpot: slot.customPricePerSpot == null ? "" : String(slot.customPricePerSpot),
    isActive: slot.isActive ? "on" : "",
  } : { isActive: "on" } };
  const [rawState, action, pending] = useActionState(saveSlot.bind(null, spaceId, slot?.id ?? null), initial);
  const state = { ...initial, ...rawState, values: rawState?.values ?? initial.values, errors: rawState?.errors ?? {} };
  const values = state.values;
  return <form action={action} key={state.revision} className="grid max-w-2xl gap-5 sm:grid-cols-2" noValidate>
    <div className="sm:col-span-2"><Feedback state={state} /></div>
    <div><label htmlFor="startsAt" className={labelClass}>Início</label><input id="startsAt" name="startsAt" type="datetime-local" required defaultValue={values.startsAt ?? ""} className={inputClass} /></div>
    <div><label htmlFor="endsAt" className={labelClass}>Fim</label><input id="endsAt" name="endsAt" type="datetime-local" required defaultValue={values.endsAt ?? ""} className={inputClass} /><ErrorText state={state} name="endsAt" /></div>
    <div><label htmlFor="slot-capacity" className={labelClass}>Capacidade</label><input id="slot-capacity" name="capacity" type="number" min="1" step="1" required defaultValue={values.capacity ?? ""} className={inputClass} /><ErrorText state={state} name="capacity" /></div>
    <div><label htmlFor="slot-price" className={labelClass}>Preço personalizado (R$) · opcional</label><input id="slot-price" name="customPricePerSpot" type="number" min="0" step="0.01" defaultValue={values.customPricePerSpot ?? ""} placeholder="Usar preço do espaço" className={inputClass} /><ErrorText state={state} name="customPricePerSpot" /></div>
    <label className="flex items-center gap-3 text-sm font-semibold sm:col-span-2"><input type="checkbox" name="isActive" defaultChecked={values.isActive === "on"} className="size-5 accent-brand" />Horário ativo</label>
    <div className="sm:col-span-2"><Submit pending={pending} label={slot ? "Salvar horário" : "Adicionar horário avulso"} /></div>
  </form>;
}

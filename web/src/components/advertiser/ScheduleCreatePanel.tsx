"use client";

import { useState } from "react";
import { CreateRuleForm, SlotForm } from "./Forms";

export function ScheduleCreatePanel({ spaceId }: { spaceId: number }) {
  const [kind, setKind] = useState<"recurring" | "standalone">("recurring");
  return <section aria-labelledby="add-schedule-title" className="rounded-2xl border border-line bg-white p-5 sm:p-7">
    <h2 id="add-schedule-title" className="text-xl font-extrabold">Adicionar horários</h2>
    <div className="mt-5 grid max-w-2xl grid-cols-2 rounded-xl bg-surface-soft p-1" role="group" aria-label="Tipo de horário">
      <button type="button" aria-pressed={kind === "recurring"} onClick={() => setKind("recurring")} className={`min-h-11 rounded-lg px-3 text-xs font-bold sm:text-sm ${kind === "recurring" ? "bg-brand text-white" : "text-ink hover:bg-surface-hint"}`}>Recorrentes</button>
      <button type="button" aria-pressed={kind === "standalone"} onClick={() => setKind("standalone")} className={`min-h-11 rounded-lg px-3 text-xs font-bold sm:text-sm ${kind === "standalone" ? "bg-brand text-white" : "text-ink hover:bg-surface-hint"}`}>Data específica</button>
    </div>
    <p className="my-6 max-w-2xl text-xs leading-5 text-muted">{kind === "recurring" ? "Selecione os dias que compartilham horário, capacidade e vigência. Cada dia será cadastrado como uma regra independente." : "Use para um horário avulso, sem regra semanal. As datas e horas seguem a referência UTC da API."}</p>
    <div hidden={kind !== "recurring"}><CreateRuleForm spaceId={spaceId} /></div>
    <div hidden={kind !== "standalone"}><SlotForm spaceId={spaceId} /></div>
  </section>;
}

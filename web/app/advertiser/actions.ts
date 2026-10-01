"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { ApiError, advertiserUserId, api, ownedSpace, spaceRules } from "@/src/lib/advertiser";
import type {
  AvailabilityRuleDto, BookableSlotDTO, CategoryEnum, CreateAvailabilityRuleDTO,
  CreateBookableSlotDTO, CreateSpaceDto, DayOfTheWeekEnum, SpaceDTO,
  UpdateAvailabilityRuleDTO, UpdateBookableSlotDTO, UpdateSpaceDto,
} from "@/lib/api/models";

export type FormState = {
  values: Record<string, string>;
  errors: Record<string, string>;
  message: string;
  success: boolean;
  revision: number;
};
function valuesOf(formData: FormData) {
  return Object.fromEntries([...formData.entries()]
    .filter(([key, value]) => !key.startsWith("$ACTION_") && typeof value === "string")
    .map(([key, value]) => [key, value as string]));
}
function failure(previous: FormState, values: Record<string, string>, message: string, errors: Record<string, string> = {}): FormState {
  return { values, errors, message, success: false, revision: previous.revision + 1 };
}
function success(previous: FormState, values: Record<string, string>, message: string): FormState {
  return { values, errors: {}, message, success: true, revision: previous.revision + 1 };
}
function apiFailure(previous: FormState, values: Record<string, string>, error: unknown) {
  if (error instanceof ApiError) {
    const details = error.details as { errors?: Record<string, string[]>; detail?: string } | null;
    const errors = Object.fromEntries(Object.entries(details?.errors ?? {}).map(([key, messages]) => [key.replace(/^dto\./i, "").replace(/^./, letter => letter.toLowerCase()), messages.join(" ")]));
    const message = error.status === 400 ? details?.detail ?? "Revise os campos destacados e tente novamente."
      : error.status === 404 ? "Registro não encontrado ou indisponível para este anunciante."
      : "Não foi possível salvar agora. Tente novamente.";
    return failure(previous, values, message, errors);
  }
  return failure(previous, values, "Não foi possível conectar à API. Seus dados foram mantidos.");
}
function field(values: Record<string, string>, name: string) { return values[name]?.trim() ?? ""; }
function price(values: Record<string, string>, name: string) {
  const raw = field(values, name);
  return raw ? Number(raw.replace(",", ".")) : null;
}
function json(body: unknown): RequestInit { return { headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) }; }
const dateUtc = (value: string) => `${value}T00:00:00Z`;
const dateTimeUtc = (value: string) => `${value}:00Z`;

export async function createSpace(previous: FormState, formData: FormData): Promise<FormState> {
  const values = valuesOf(formData);
  const errors: Record<string, string> = {};
  for (const name of ["title", "address", "description"]) if (!field(values, name)) errors[name] = "Campo obrigatório.";
  const amount = price(values, "pricePerSpot");
  if (amount !== null && (!Number.isFinite(amount) || amount < 0)) errors.pricePerSpot = "Informe um preço válido.";
  if (Object.keys(errors).length) return failure(previous, values, "Revise os campos destacados.", errors);
  if (!advertiserUserId) return failure(previous, values, "Configure o usuário de desenvolvimento antes de cadastrar espaços.");
  let id: number | undefined;
  try {
    const body = {
      ownerId: advertiserUserId, title: field(values, "title"), address: field(values, "address"),
      description: field(values, "description"), category: Number(values.category ?? 5) as CategoryEnum,
      pricePerSpot: amount,
    } satisfies CreateSpaceDto;
    const result = await api<SpaceDTO>("/api/spaces", { method: "POST", ...json(body) });
    id = result.id;
  } catch (error) { return apiFailure(previous, values, error); }
  revalidatePath("/advertiser");
  redirect(id ? `/advertiser/spaces/${id}?created=1` : "/advertiser");
}

export async function updateSpace(id: number, previous: FormState, formData: FormData): Promise<FormState> {
  const values = valuesOf(formData);
  const errors: Record<string, string> = {};
  for (const name of ["title", "address", "description"]) if (!field(values, name)) errors[name] = "Campo obrigatório.";
  const amount = price(values, "pricePerSpot");
  if (amount === null || !Number.isFinite(amount) || amount < 0) errors.pricePerSpot = "Informe um preço válido.";
  if (Object.keys(errors).length) return failure(previous, values, "Revise os campos destacados.", errors);
  try {
    await ownedSpace(id);
    const body = {
      title: field(values, "title"), address: field(values, "address"), description: field(values, "description"),
      category: Number(values.category ?? 5) as CategoryEnum, pricePerSpot: amount,
      isActive: values.isActive === "on",
    } satisfies UpdateSpaceDto;
    await api<SpaceDTO>(`/api/spaces/${id}`, { method: "PUT", ...json(body) });
    revalidatePath("/advertiser"); revalidatePath(`/advertiser/spaces/${id}`);
    return success(previous, values, "Espaço atualizado.");
  } catch (error) { return apiFailure(previous, values, error); }
}

export async function uploadCover(id: number, previous: FormState, formData: FormData): Promise<FormState> {
  const values = valuesOf(formData);
  const file = formData.get("file");
  if (!(file instanceof File) || !file.size) return failure(previous, values, "Escolha uma imagem para enviar.");
  try {
    await ownedSpace(id);
    const payload = new FormData(); payload.set("spaceId", String(id)); payload.set("file", file);
    await api<SpaceDTO>("/api/uploads/images", { method: "POST", body: payload });
    revalidatePath("/advertiser"); revalidatePath(`/advertiser/spaces/${id}`);
    return success(previous, values, "Foto de capa atualizada.");
  } catch (error) { return apiFailure(previous, values, error); }
}

function ruleFields(values: Record<string, string>) {
  const errors: Record<string, string> = {};
  const start = field(values, "startTime"), end = field(values, "endTime");
  if (!start || !end || end <= start) errors.endTime = "O fim deve ser posterior ao início.";
  if (!values.validFrom || !values.validUntil || values.validUntil < values.validFrom) errors.validUntil = "Confira o período de vigência.";
  if (!Number.isInteger(Number(values.capacity)) || Number(values.capacity) < 1) errors.capacity = "Informe uma capacidade positiva.";
  if (!Number.isInteger(Number(values.slotDurationMinutes)) || Number(values.slotDurationMinutes) < 30) errors.slotDurationMinutes = "Mínimo de 30 minutos.";
  const amount = price(values, "customPricePerSpot");
  if (amount !== null && (!Number.isFinite(amount) || amount < 0)) errors.customPricePerSpot = "Informe um preço válido.";
  return { errors, amount, fields: {
    startTime: `${start}:00`, endTime: `${end}:00`,
    validFrom: dateUtc(values.validFrom), validUntil: dateUtc(values.validUntil),
    capacity: Number(values.capacity), slotDurationMinutes: Number(values.slotDurationMinutes),
    customPricePerSpot: amount, isActive: values.isActive === "on",
  } };
}

const weekdays = ["segunda", "terça", "quarta", "quinta", "sexta", "sábado", "domingo"];

export async function createRules(spaceId: number, previous: FormState, formData: FormData): Promise<FormState> {
  const values = valuesOf(formData);
  const { errors, fields } = ruleFields(values);
  const days = weekdays.map((_, index) => index).filter(index => values[`day${index}`] === "on");
  if (!days.length) errors.days = "Selecione pelo menos um dia da semana.";
  if (Object.keys(errors).length) return failure(previous, values, "Revise os campos destacados.", errors);
  try {
    await ownedSpace(spaceId);
    const results = await Promise.all(days.map(async day => {
      const body = { ...fields, spaceId, dayOfTheWeek: day as DayOfTheWeekEnum } satisfies CreateAvailabilityRuleDTO;
      try {
        await api<AvailabilityRuleDto>("/api/availability-rule", { method: "POST", ...json(body) });
        return { day, created: true };
      } catch {
        return { day, created: false };
      }
    }));
    revalidatePath(`/advertiser/spaces/${spaceId}/schedule`); revalidatePath("/advertiser/occupancy");
    const failed = results.filter(result => !result.created).map(result => result.day);
    if (failed.length) {
      const retryValues = { ...values };
      for (const { day, created } of results) if (created) delete retryValues[`day${day}`];
      const completed = results.length - failed.length;
      return failure(previous, retryValues,
        `${completed} de ${results.length} ${results.length === 1 ? "regra criada" : "regras criadas"}. Falha em ${failed.map(day => weekdays[day]).join(", ")}. Tente novamente para os dias que continuam selecionados.`,
        { days: "Os dias criados foram desmarcados para evitar duplicação." });
    }
    const nextValues = { ...values };
    for (const day of days) delete nextValues[`day${day}`];
    return success(previous, nextValues, `${results.length} ${results.length === 1 ? "regra criada" : "regras criadas"}.`);
  } catch (error) { return apiFailure(previous, values, error); }
}

export async function updateRule(spaceId: number, ruleId: number, previous: FormState, formData: FormData): Promise<FormState> {
  const values = valuesOf(formData);
  const { errors, amount, fields } = ruleFields(values);
  const day = Number(values.dayOfTheWeek);
  if (!Number.isInteger(day) || day < 0 || day > 6) errors.dayOfTheWeek = "Selecione um dia válido.";
  if (Object.keys(errors).length) return failure(previous, values, "Revise os campos destacados.", errors);
  try {
    await ownedSpace(spaceId);
    if (!(await spaceRules(spaceId)).some(rule => rule.id === ruleId)) throw new ApiError(404, null);
    const body = {
      ...fields, dayOfTheWeek: day as DayOfTheWeekEnum,
      clearCustomPricePerSpot: amount === null,
    } satisfies UpdateAvailabilityRuleDTO;
    await api<AvailabilityRuleDto>(`/api/availability-rule/${ruleId}`, { method: "PUT", ...json(body) });
    revalidatePath(`/advertiser/spaces/${spaceId}/schedule`); revalidatePath("/advertiser/occupancy");
    return success(previous, values, "Regra atualizada.");
  } catch (error) { return apiFailure(previous, values, error); }
}

export async function saveSlot(spaceId: number, slotId: number | null, previous: FormState, formData: FormData): Promise<FormState> {
  const values = valuesOf(formData);
  const errors: Record<string, string> = {};
  if (!values.startsAt || !values.endsAt || values.endsAt <= values.startsAt) errors.endsAt = "O fim deve ser posterior ao início.";
  if (!Number.isInteger(Number(values.capacity)) || Number(values.capacity) < 1) errors.capacity = "Informe uma capacidade positiva.";
  const amount = price(values, "customPricePerSpot");
  if (amount !== null && (!Number.isFinite(amount) || amount < 0)) errors.customPricePerSpot = "Informe um preço válido.";
  if (Object.keys(errors).length) return failure(previous, values, "Revise os campos destacados.", errors);
  try {
    await ownedSpace(spaceId);
    let existing: BookableSlotDTO | null = null;
    if (slotId !== null) {
      existing = await api<BookableSlotDTO>(`/api/bookable-slots/${slotId}`);
      if (existing.spaceId !== spaceId) throw new ApiError(404, null);
    }
    const common = { startsAt: dateTimeUtc(values.startsAt), endsAt: dateTimeUtc(values.endsAt),
      customPricePerSpot: amount, isActive: values.isActive === "on" };
    if (slotId === null) {
      const body = { ...common, spaceId, capacity: Number(values.capacity) } satisfies CreateBookableSlotDTO;
      await api<BookableSlotDTO>("/api/bookable-slots", { method: "POST", ...json(body) });
    } else {
      const body = { ...common,
        capacity: existing?.capacity === Number(values.capacity) ? undefined : Number(values.capacity),
        clearCustomPricePerSpot: amount === null } satisfies UpdateBookableSlotDTO;
      await api<BookableSlotDTO>(`/api/bookable-slots/${slotId}`, { method: "PUT", ...json(body) });
    }
    revalidatePath(`/advertiser/spaces/${spaceId}/schedule`); revalidatePath("/advertiser/occupancy");
    return success(previous, values, slotId === null ? "Horário avulso criado." : "Horário atualizado.");
  } catch (error) { return apiFailure(previous, values, error); }
}

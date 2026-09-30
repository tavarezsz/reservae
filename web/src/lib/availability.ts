import type { AvailableSlotDto } from "@/lib/api/models";

export const money = (value: number) => new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" }).format(value);
export const dateKey = (date: Date) => `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}-${String(date.getDate()).padStart(2, "0")}`;
export const parseDate = (value: string) => new Date(`${value}T12:00:00`);
export function addDays(value: string, days: number) {
  const date = parseDate(value);
  date.setDate(date.getDate() + days);
  return dateKey(date);
}
export function weekStart(value: string) {
  const date = parseDate(value);
  return addDays(value, -((date.getDay() + 6) % 7));
}
export const dateLabel = (value: string) => parseDate(value).toLocaleDateString("pt-BR", { weekday: "long", day: "numeric", month: "long" });
export const shortDate = (value: string) => parseDate(value).toLocaleDateString("pt-BR", { day: "numeric", month: "short" });
export const timeLabel = (value: string) => new Date(value).toLocaleTimeString("pt-BR", { hour: "2-digit", minute: "2-digit", timeZone: "UTC" });
export const slotKey = (slot: AvailableSlotDto) => `${slot.bookableSlotId ?? slot.availabilityRuleId}:${slot.startsAt}:${slot.endsAt}`;
export const slotDate = (slot: AvailableSlotDto) => slot.startsAt?.slice(0, 10) ?? "";
export const canBook = (slot: AvailableSlotDto) => !!slot.startsAt && !!slot.endsAt && new Date(slot.startsAt).getTime() > Date.now() && (slot.availableQuantity ?? 0) > 0 && (slot.bookableSlotId != null || slot.availabilityRuleId != null);

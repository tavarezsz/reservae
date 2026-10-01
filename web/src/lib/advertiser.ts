import type { AvailabilityRuleDto, AvailableSlotDto, BookableSlotDTO, BookingDto, SpaceDTO } from "@/lib/api/models";

const baseUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5144";

export const advertiserUserId = process.env.NEXT_PUBLIC_BOOKING_USER_ID;

export class ApiError extends Error {
  constructor(public status: number, public details: unknown) {
    super(`API retornou ${status}`);
  }
}

export async function api<T>(path: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${baseUrl}${path}`, { cache: "no-store", ...options });
  const text = await response.text();
  let body: unknown;
  try { body = text ? JSON.parse(text) : null; } catch { body = text; }
  if (!response.ok) throw new ApiError(response.status, body);
  return body as T;
}

type Page<T> = { items?: T[]; totalCount?: number };

async function allPages<T>(path: string): Promise<T[]> {
  const items: T[] = [];
  for (let page = 1; ; page++) {
    const separator = path.includes("?") ? "&" : "?";
    const response = await api<Page<T>>(`${path}${separator}page=${page}&pageSize=100`);
    items.push(...response.items ?? []);
    if (items.length >= (response.totalCount ?? 0) || !response.items?.length) return items;
  }
}

export async function ownerSpaces(): Promise<SpaceDTO[]> {
  if (!advertiserUserId) return [];
  return allPages<SpaceDTO>(`/api/spaces/owner/${encodeURIComponent(advertiserUserId)}`);
}

export async function ownedSpace(id: number): Promise<SpaceDTO> {
  const space = await api<SpaceDTO>(`/api/spaces/${id}`);
  if (!advertiserUserId || space.ownerId !== advertiserUserId) throw new ApiError(404, null);
  return space;
}

export async function spaceRules(id: number): Promise<AvailabilityRuleDto[]> {
  await ownedSpace(id);
  return allPages<AvailabilityRuleDto>(`/api/availability-rule/space/${id}`);
}

export async function standaloneSlots(id: number): Promise<BookableSlotDTO[]> {
  await ownedSpace(id);
  return allPages<BookableSlotDTO>(`/api/bookable-slots/space/${id}/standalone`);
}

export async function spaceBookings(id: number): Promise<BookingDto[]> {
  await ownedSpace(id);
  return allPages<BookingDto>(`/api/bookings/space/${id}`);
}

export async function spaceSlots(id: number, date: string): Promise<AvailableSlotDto[]> {
  await ownedSpace(id);
  return api<AvailableSlotDto[]>(`/api/spaces/${id}/availability?fromDate=${date}&toDate=${date}`);
}

"use client";

import { useEffect, useState } from "react";
import Image from "next/image";
import Link from "next/link";
import type { BookingDto } from "@/lib/api/models";
import { getApiBookingsUserUserId } from "@/lib/api/booking/booking";
import { categoryLabels } from "@/src/constants/CategoryLabels";
import { timeLabel } from "@/src/lib/availability";
import { Icon } from "./atoms/Icon";

const PAGE_SIZE = 9;
const statusLabels: Record<number, string> = { 0: "Confirmada", 1: "Cancelada", 2: "Rejeitada", 3: "Expirada" };
const dateFormatter = new Intl.DateTimeFormat("pt-BR", { weekday: "short", day: "numeric", month: "long", year: "numeric", timeZone: "UTC" });

export function MyBookings() {
  const userId = process.env.NEXT_PUBLIC_BOOKING_USER_ID;
  const [request, setRequest] = useState({ page: 1, revision: 0 });
  const [bookings, setBookings] = useState<BookingDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);
  const [referenceTime, setReferenceTime] = useState(0);

  useEffect(() => {
    if (!userId) return;
    const controller = new AbortController();
    getApiBookingsUserUserId(userId, { page: request.page, pagesize: PAGE_SIZE }, { signal: controller.signal }).then((response) => {
      if (controller.signal.aborted) return;
      const next = response.items ?? [];
      setBookings((previous) => request.page === 1 ? next : [...previous, ...next.filter((item) => !previous.some((existing) => existing.id === item.id))]);
      setTotalCount(response.totalCount ?? 0);
      setReferenceTime(Date.now());
      setError(false);
    }).catch(() => {
      if (!controller.signal.aborted) setError(true);
    }).finally(() => {
      if (!controller.signal.aborted) setLoading(false);
    });
    return () => controller.abort();
  }, [userId, request]);

  function reload() {
    setLoading(true);
    setError(false);
    setRequest((previous) => ({ ...previous, revision: previous.revision + 1 }));
  }

  function loadMore() {
    setLoading(true);
    setRequest((previous) => ({ page: previous.page + 1, revision: previous.revision + 1 }));
  }

  const upcoming = bookings.filter((booking) => booking.status === 0 && booking.endsAt && new Date(booking.endsAt).getTime() >= referenceTime).sort((a, b) => (a.startsAt ?? "").localeCompare(b.startsAt ?? ""));
  const past = bookings.filter((booking) => !upcoming.includes(booking)).sort((a, b) => (b.startsAt ?? "").localeCompare(a.startsAt ?? ""));

  return (
    <div className="pb-8">
      <header className="pt-4 sm:pt-6">
        <p className="text-[10px] font-extrabold tracking-[.15em] text-brand">SUA AGENDA</p>
        <h1 className="mt-3 text-[32px] font-extrabold tracking-tight text-ink sm:text-[40px]">Minhas reservas</h1>
        <p className="mt-3 text-sm text-muted sm:text-base">Seus próximos planos já têm lugar.</p>
      </header>

      {!userId && <div role="alert" className="mt-9 rounded-2xl bg-error-surface p-5 text-sm text-error">Configure o usuário de desenvolvimento para consultar as reservas.</div>}
      {userId && loading && bookings.length === 0 && <div role="status" className="mt-10 grid gap-5 sm:grid-cols-2 xl:grid-cols-3">{Array.from({ length: 3 }, (_, index) => <div key={index} className="overflow-hidden rounded-[22px] border border-line bg-white motion-safe:animate-pulse"><div className="aspect-[1.65] bg-surface-hint" /><div className="space-y-4 p-6"><div className="h-3 w-20 rounded bg-surface-hint" /><div className="h-6 w-2/3 rounded bg-surface-hint" /><div className="h-4 w-1/2 rounded bg-surface-hint" /></div></div>)}</div>}
      {userId && !loading && !error && bookings.length === 0 && <div className="mt-10 rounded-3xl bg-surface-soft px-6 py-12 text-center"><Icon name="calendar" className="mx-auto size-9 text-brand" /><h2 className="mt-4 text-xl font-extrabold">Você ainda não tem reservas</h2><p className="mt-2 text-sm text-muted">Encontre um espaço e escolha seu primeiro horário.</p><Link href="/#espacos" className="mt-6 inline-flex min-h-11 items-center gap-2 rounded-xl bg-brand px-5 text-sm font-bold text-white">Explorar espaços<Icon name="arrow" /></Link></div>}

      {upcoming.length > 0 && <section className="mt-11" aria-labelledby="upcoming-title"><div className="mb-5 flex items-center justify-between gap-3"><h2 id="upcoming-title" className="text-[22px] font-extrabold">Próximas reservas</h2><span className="rounded-full bg-surface-soft px-3 py-2 text-xs font-bold text-brand">{upcoming.length} {upcoming.length === 1 ? "reserva" : "reservas"}</span></div><div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-3">{upcoming.map((booking) => <BookingCard key={booking.id} booking={booking} />)}</div></section>}
      {past.length > 0 && <section className="mt-11" aria-labelledby="past-title"><div className="mb-5 flex items-center justify-between gap-3"><h2 id="past-title" className="text-[22px] font-extrabold">Reservas anteriores</h2><span className="rounded-full bg-surface-soft px-3 py-2 text-xs font-bold text-brand">{past.length} {past.length === 1 ? "reserva" : "reservas"}</span></div><div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-3">{past.map((booking) => <BookingCard key={booking.id} booking={booking} />)}</div></section>}
      {error && <div role="alert" className="mt-9 rounded-2xl bg-error-surface p-5 text-center text-sm text-error">Não foi possível carregar as reservas.<button type="button" onClick={reload} className="ml-2 font-bold underline">Tentar novamente</button></div>}
      {bookings.length < totalCount && !error && <div className="mt-8 text-center"><button type="button" onClick={loadMore} disabled={loading} className="min-h-11 rounded-xl border border-line-strong bg-white px-6 text-sm font-bold text-brand disabled:opacity-50">{loading ? "Carregando…" : "Carregar mais reservas"}</button></div>}
      {bookings.length > 0 && <p className="mt-9 text-center text-xs text-muted">{bookings.length} de {totalCount} reservas carregadas · Um lugar para cada plano.</p>}
    </div>
  );
}

function BookingCard({ booking }: { booking: BookingDto }) {
  const [failedImage, setFailedImage] = useState(false);
  const imageUrl = booking.spaceCoverImagePath ? new URL(booking.spaceCoverImagePath, process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5144").toString() : null;
  const date = booking.startsAt ? new Date(booking.startsAt) : null;
  const status = booking.status ?? -1;

  return (
    <article className="flex h-full flex-col overflow-hidden rounded-[22px] border border-line bg-white">
      <div className="relative flex aspect-[1.65] items-center justify-center bg-surface-hint text-brand">
        {imageUrl && !failedImage ? <Image src={imageUrl} alt={`Foto de ${booking.spaceTitle ?? "espaço"}`} fill unoptimized sizes="(max-width: 639px) 90vw, (max-width: 1279px) 45vw, 30vw" className="object-cover" onError={() => setFailedImage(true)} /> : <div className="flex flex-col items-center gap-2 text-muted"><Icon name="space" className="size-9" /><span className="text-xs">Espaço sem foto</span></div>}
      </div>
      <div className="flex flex-1 flex-col p-5 sm:p-6">
        <span className="text-[10px] font-extrabold tracking-[.13em] text-brand uppercase">{booking.spaceCategory != null ? categoryLabels[booking.spaceCategory] : "Espaço"}</span>
        <h3 className="mt-2 text-[22px] font-extrabold leading-tight text-ink">{booking.spaceTitle || "Espaço indisponível"}</h3>
        <p className="mt-1 text-xs leading-5 text-muted">{booking.spaceAddress || "Endereço não informado"}</p>
        <div className="mt-5 border-y border-line py-4">
          <p className="flex items-center gap-2 text-xs text-muted"><Icon name="calendar" className="size-4 shrink-0 text-brand" />{date ? dateFormatter.format(date) : "Data não disponível"}</p>
          <p className="mt-2 pl-6 text-lg font-extrabold text-ink">{booking.startsAt && booking.endsAt ? `${timeLabel(booking.startsAt)} – ${timeLabel(booking.endsAt)}` : "Horário não disponível"}</p>
        </div>
        <div className="mt-auto flex flex-wrap items-center justify-between gap-3 pt-4">
          <div><span className={`inline-flex rounded-full px-3 py-1.5 text-xs font-bold ${status === 0 ? "bg-surface-soft text-brand" : "bg-disabled-subtle text-muted"}`}>{statusLabels[status] ?? "Status indisponível"}</span><p className="mt-2 text-[11px] text-muted">{booking.quantity ?? 0} {booking.quantity === 1 ? "vaga" : "vagas"} · Reserva #{booking.id}</p></div>
          {booking.spaceId != null && <Link href={`/spaces/${booking.spaceId}`} className="inline-flex min-h-10 items-center gap-2 text-xs font-bold text-brand hover:underline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-focus">Ver espaço<Icon name="arrow" className="size-4" /></Link>}
        </div>
      </div>
    </article>
  );
}

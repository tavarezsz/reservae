"use client";

import { SpaceDTO } from "@/lib/api/models";
import Link from "next/link";

type SpaceCardProps = {
  space: SpaceDTO;
};

export function SpaceCard({ space }: SpaceCardProps) {
  if (!space) return null;

  const isFree = !space.pricePerSpot || space.pricePerSpot === 0

  const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5144";

  return (
    <Link
      href={`/spaces/${space.id}`}
      className="flex flex-col rounded-2xl border border-line m-6"
    >
      {space.coverImagePath && (
        <img src={new URL(space.coverImagePath, apiUrl).toString()} className="rounded-t-2xl"/>
      )}
      <div className="flex flex-col p-4">
        <span>{space.category}</span>
        <h3 className="text-2xl text-dark-surface font-extrabold mt-3 mb-0.5">
          {space.title}
        </h3>
        <p className="text-xs text-muted">{space.address}</p>
        <p className="text-dark-surface text-sm my-4 font-semibold">{isFree ? "Gratuito" : `R$ ${space.pricePerSpot} / vaga por horário`}</p>
      </div>
    </Link>
  );
}

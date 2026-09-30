"use client";

import type { SpaceDTO } from "@/lib/api/models";
import { categoryLabels } from "@/src/constants/CategoryLabels";
import Image from "next/image";
import Link from "next/link";
import { useState } from "react";
import { Icon } from "./Icon";

const currency = new Intl.NumberFormat("pt-BR", {
  style: "currency",
  currency: "BRL",
});

export function SpaceCard({ space }: { space: SpaceDTO }) {
  const [failedImage, setFailedImage] = useState(false);
  const imageUrl = space.coverImagePath
    ? new URL(
        space.coverImagePath,
        process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5144",
      ).toString()
    : null;

  return (
    <Link
      href={`/spaces/${space.id}`}
      className="flex h-full flex-col overflow-hidden rounded-[22px] border border-line bg-surface focus-visible:outline-2 focus-visible:outline-offset-[-2px] focus-visible:outline-focus"
    >
      <div className="relative flex aspect-[1.55] items-center justify-center bg-surface-hint text-brand sm:aspect-[1.65]">
        {imageUrl && !failedImage ? (
          <Image
            src={imageUrl}
            alt={`Foto de ${space.title ?? "espaço"}`}
            fill
            unoptimized
            sizes="(max-width: 639px) 85vw, (max-width: 1279px) 45vw, 30vw"
            className="object-cover"
            onError={() => setFailedImage(true)}
          />
        ) : (
          <div className="flex flex-col items-center gap-3">
            <Icon name="space" className="size-10 opacity-60" />
            <span className="text-xs text-muted">Espaço sem foto</span>
          </div>
        )}
      </div>
      <div className="flex flex-1 flex-col p-5 sm:p-6">
        <span className="text-[10px] font-extrabold tracking-[.13em] text-brand uppercase">
          {space.category != null ? categoryLabels[space.category] : "Outros"}
        </span>
        <h3 className="mt-3 text-[22px] leading-tight font-extrabold tracking-tight text-ink">
          {space.title}
        </h3>
        <p className="mt-2 flex items-start gap-1.5 text-xs leading-5 text-muted">
          <Icon name="pin" className="mt-0.5 size-4 shrink-0" />
          {space.address || "Endereço não informado"}
        </p>
        <p className="mt-auto pt-5 text-sm font-extrabold text-ink">
          {space.pricePerSpot === 0 ? (
            "Gratuito"
          ) : space.pricePerSpot != null ? (
            <>
              {currency.format(space.pricePerSpot)}{" "}
              <span className="text-xs font-medium text-muted">
                / vaga por horário
              </span>
            </>
          ) : (
            "Consulte o preço do horário"
          )}
        </p>
      </div>
    </Link>
  );
}

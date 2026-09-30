"use client";

import { useEffect, useState } from "react";
import Image from "next/image";
import Link from "next/link";
import type { SpaceDTO } from "@/lib/api/models";
import { getApiSpacesId } from "@/lib/api/space/space";
import { categoryLabels } from "@/src/constants/CategoryLabels";
import { Icon } from "./atoms/Icon";
import { SpaceAvailability } from "./SpaceAvailability";

export function SpaceDetail({ spaceId }: { spaceId: number }) {
  const [space, setSpace] = useState<SpaceDTO | null>(null);
  const [error, setError] = useState<"missing" | "failed" | null>(null);
  const [revision, setRevision] = useState(0);
  const [failedImage, setFailedImage] = useState(false);

  useEffect(() => {
    const controller = new AbortController();
    getApiSpacesId(spaceId, { signal: controller.signal }).then((response) => {
      if (!controller.signal.aborted) { setSpace(response); setError(null); }
    }).catch((reason: unknown) => {
      if (!controller.signal.aborted) setError((reason as { status?: number }).status === 404 ? "missing" : "failed");
    });
    return () => controller.abort();
  }, [spaceId, revision]);

  if (error) return <section role="alert" className="rounded-3xl bg-surface-soft p-8 text-center"><h1 className="text-2xl font-extrabold">{error === "missing" ? "Espaço não encontrado" : "Não foi possível carregar o espaço"}</h1><p className="mt-3 text-sm text-muted">{error === "missing" ? "Este espaço não está disponível." : "Tente novamente em alguns instantes."}</p>{error === "failed" && <button onClick={() => { setError(null); setRevision((value) => value + 1); }} className="mt-5 rounded-xl bg-brand px-5 py-3 text-sm font-bold text-white">Tentar novamente</button>}<Link href="/" className="mt-5 block text-sm font-bold text-brand underline">Explorar espaços</Link></section>;
  if (!space) return <div role="status" className="py-16 text-center text-muted">Carregando espaço…</div>;

  const imageUrl = space.coverImagePath ? new URL(space.coverImagePath, process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5144").toString() : null;

  return (
    <div className="grid items-start gap-8 xl:grid-cols-[1.1fr_1fr] xl:gap-x-8 xl:gap-y-10">
      <section className="min-w-0 xl:col-start-1 xl:row-start-1" aria-labelledby="space-title">
        <div className="relative flex aspect-[1.65] items-center justify-center overflow-hidden rounded-3xl bg-surface-hint">
          {imageUrl && !failedImage ? <Image src={imageUrl} alt={`Foto de ${space.title}`} fill unoptimized preload sizes="(min-width: 1280px) 45vw, 100vw" className="object-cover" onError={() => setFailedImage(true)} /> : <div className="flex flex-col items-center gap-3 text-muted"><Icon name="space" className="size-12" /><span className="text-sm">Espaço sem foto</span></div>}
          <span className="absolute bottom-4 left-4 rounded-full bg-surface px-4 py-2 text-[10px] font-extrabold text-brand uppercase">{space.category != null ? categoryLabels[space.category] : "Outros"}</span>
        </div>
        <h1 id="space-title" className="mt-7 text-[32px] leading-tight font-extrabold tracking-tight sm:text-[38px]">{space.title}</h1>
        <p className="mt-4 flex items-start gap-2 text-xs leading-5 text-muted"><Icon name="pin" className="size-4 shrink-0" />{space.address}</p>
        <p className="mt-6 text-sm leading-7 whitespace-pre-line text-muted">{space.description}</p>
      </section>
      <SpaceAvailability space={space} spaceId={spaceId} />
    </div>
  );
}

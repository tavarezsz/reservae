"use client";

import { useState } from "react";
import Image from "next/image";
import { Icon } from "@/src/components/atoms/Icon";

export function CoverImage({ path, title, sizes }: { path?: string | null; title?: string | null; sizes: string }) {
  const [failed, setFailed] = useState(false);
  if (!path || failed) return <div className="flex h-full w-full flex-col items-center justify-center gap-2 text-muted"><Icon name="space" className="size-10" /><span className="text-xs">Espaço sem foto</span></div>;
  return <Image src={new URL(path, process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5144").toString()} alt={`Foto de ${title ?? "espaço"}`} fill unoptimized sizes={sizes} className="object-cover" onError={() => setFailed(true)} />;
}

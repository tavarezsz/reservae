"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

export function AdvertiserTabs() {
  const path = usePathname();
  const onSpaces = path !== "/advertiser/occupancy";
  return <nav aria-label="Área do anunciante" className="flex gap-7 text-sm font-bold">
    <Link href="/advertiser" aria-current={onSpaces ? "page" : undefined} className={`border-b-[3px] pb-3 hover:text-brand ${onSpaces ? "border-brand text-brand" : "border-transparent text-muted"}`}>Meus espaços</Link>
    <Link href="/advertiser/occupancy" aria-current={path === "/advertiser/occupancy" ? "page" : undefined} className={`border-b-[3px] pb-3 hover:text-brand ${path === "/advertiser/occupancy" ? "border-brand text-brand" : "border-transparent text-muted"}`}>Ocupação</Link>
  </nav>;
}

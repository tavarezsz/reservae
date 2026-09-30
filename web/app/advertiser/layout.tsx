import type { Metadata } from "next";
import { SiteShell } from "@/src/components/SiteShell";
import { AdvertiserTabs } from "@/src/components/advertiser/Tabs";

export const metadata: Metadata = { title: "Área do anunciante | reservaê" };

export default function AdvertiserLayout({ children }: { children: React.ReactNode }) {
  return <SiteShell active="advertiser">
    <div className="mb-7 border-b border-line">
      <p className="mb-5 text-xs text-muted">● Área do anunciante · Conta de demonstração</p>
      <AdvertiserTabs />
    </div>
    {children}
  </SiteShell>;
}

import type { Metadata } from "next";
import { SiteShell } from "@/src/components/SiteShell";
import { AdvertiserTabs } from "@/src/components/advertiser/Tabs";
import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import { accessTokenCookie } from "@/src/lib/api-auth";

export const metadata: Metadata = { title: "Área do anunciante | reservaê" };

export default async function AdvertiserLayout({ children }: { children: React.ReactNode }) {
  if (!(await cookies()).has(accessTokenCookie)) redirect("/login?next=/advertiser");
  return <SiteShell active="advertiser">
    <div className="mb-7 ">
      <AdvertiserTabs />
    </div>
    {children}
  </SiteShell>;
}

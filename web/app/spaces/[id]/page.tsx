import { notFound } from "next/navigation";
import { SiteShell } from "@/src/components/SiteShell";
import { SpaceDetail } from "@/src/components/SpaceDetail";

export default async function SpacePage({ params }: PageProps<"/spaces/[id]">) {
  const { id } = await params;
  const spaceId = Number(id);
  if (!/^\d+$/.test(id) || !Number.isSafeInteger(spaceId) || spaceId < 1) notFound();

  return <SiteShell><SpaceDetail key={spaceId} spaceId={spaceId} /></SiteShell>;
}

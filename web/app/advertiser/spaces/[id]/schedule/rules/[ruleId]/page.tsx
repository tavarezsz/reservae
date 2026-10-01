import Link from "next/link";
import { notFound } from "next/navigation";
import { EditRuleForm } from "@/src/components/advertiser/Forms";
import { ApiError, ownedSpace, spaceRules } from "@/src/lib/advertiser";

export default async function EditRulePage({ params }: { params: Promise<{ id: string; ruleId: string }> }) {
  const { id: rawSpaceId, ruleId: rawRuleId } = await params;
  const spaceId = Number(rawSpaceId), ruleId = Number(rawRuleId);
  if (!Number.isInteger(spaceId) || spaceId < 1 || !Number.isInteger(ruleId) || ruleId < 1) notFound();

  let space, rules;
  try { [space, rules] = await Promise.all([ownedSpace(spaceId), spaceRules(spaceId)]); }
  catch (error) { if (error instanceof ApiError && error.status === 404) notFound(); throw error; }
  const rule = rules.find(item => item.id === ruleId);
  if (!rule) notFound();

  return <div className="pb-8">
    <Link href={`/advertiser/spaces/${spaceId}/schedule`} className="text-sm font-bold text-brand hover:underline">← Horários de {space.title}</Link>
    <p className="mt-9 text-[10px] font-extrabold tracking-[.15em] text-brand">REGRA RECORRENTE</p>
    <h1 className="mt-3 text-[32px] font-extrabold tracking-tight sm:text-[40px]">Editar regra</h1>
    <p className="mt-2 mb-8 text-sm text-muted">Altere apenas esta regra. As demais regras semanais do espaço continuam independentes.</p>
    <div className="max-w-3xl rounded-2xl border border-line bg-white p-5 sm:p-7"><EditRuleForm spaceId={spaceId} rule={rule} /></div>
  </div>;
}

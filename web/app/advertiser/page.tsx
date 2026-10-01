import Link from "next/link";
import { ApiError, ownerSpaces } from "@/src/lib/advertiser";
import { categoryLabels } from "@/src/constants/CategoryLabels";
import { Icon } from "@/src/components/atoms/Icon";
import { CoverImage } from "@/src/components/advertiser/CoverImage";

export default async function AdvertiserPage() {
  const result = await ownerSpaces().then(spaces => ({ spaces, authRequired: false }))
    .catch((error: unknown) => ({ spaces: null, authRequired: error instanceof ApiError && error.status === 401 }));
  const { spaces, authRequired } = result;
  return <div className="pb-8">
    <p className="text-[10px] font-extrabold tracking-[.15em] text-brand">ÁREA DO ANUNCIANTE</p>
    <h1 className="mt-3 text-[32px] font-extrabold tracking-tight sm:text-[40px]">Meus espaços</h1>
    <p className="mt-2 text-sm text-muted sm:text-base">Seu lugar. Seus horários. Novos planos.</p>
    <Link href="/advertiser/spaces/new" className="mt-7 flex min-h-13 w-full items-center justify-between rounded-xl bg-brand px-5 text-sm font-extrabold text-white hover:bg-brand-hover sm:w-60">Criar espaço <Icon name="arrow" /></Link>
    {!spaces && <p role="alert" className="mt-8 rounded-xl bg-error-surface p-5 text-sm text-error">{authRequired ? <>Entre na sua conta para acessar seus espaços. <Link href="/login?next=/advertiser" className="font-bold underline">Entrar</Link></> : "Não foi possível carregar seus espaços. Atualize a página para tentar novamente."}</p>}
    {spaces && <section className="mt-9" aria-labelledby="list-title">
      <div className="mb-5 flex items-center justify-between"><h2 id="list-title" className="text-xl font-extrabold">Seus anúncios</h2><span className="text-xs text-muted">{spaces.length} {spaces.length === 1 ? "espaço" : "espaços"}</span></div>
      {spaces.length === 0 ? <div className="rounded-2xl border border-dashed border-line-strong bg-white p-8 text-center"><Icon name="space" className="mx-auto size-9 text-brand" /><h3 className="mt-3 font-extrabold">Seu primeiro espaço começa aqui</h3><p className="mt-2 text-sm text-muted">Cadastre o lugar para depois definir a foto e os horários.</p></div> :
        <div className="grid gap-5 sm:grid-cols-2">{spaces.map(space => <article key={space.id} className="overflow-hidden rounded-[22px] border border-line bg-white">
          <div className="relative flex aspect-[1.7] items-center justify-center bg-surface-hint text-brand">
            <CoverImage path={space.coverImagePath} title={space.title} sizes="(min-width: 640px) 45vw, 100vw" />
            <span className={`absolute top-3 left-3 rounded-full bg-white px-3 py-2 text-[10px] font-extrabold ${space.isActive ? "text-brand" : "text-muted"}`}>{space.isActive ? "Ativo" : "Inativo"}</span>
          </div>
          <div className="p-5 sm:p-6">
            <span className="text-[10px] font-extrabold tracking-wider text-brand uppercase">{categoryLabels[space.category ?? 5]}</span>
            <h3 className="mt-2 text-[24px] font-extrabold leading-tight">{space.title}</h3>
            <p className="mt-2 text-xs leading-5 text-muted">{space.address}</p>
            <div className="mt-6 flex items-center justify-between gap-3 border-t border-line pt-5 text-xs font-bold"><Link href={`/advertiser/spaces/${space.id}`} className="text-muted hover:text-brand">Editar espaço</Link><Link href={`/advertiser/spaces/${space.id}/schedule`} className="flex items-center gap-2 text-brand hover:underline">Gerenciar horários <Icon name="arrow" className="size-4" /></Link></div>
          </div>
        </article>)}</div>}
    </section>}
  </div>;
}

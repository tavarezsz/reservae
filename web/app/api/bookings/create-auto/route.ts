import { authenticatedFetch } from "@/src/lib/api-auth";

export async function POST(request: Request) {
  const upstream = await authenticatedFetch("/api/bookings/create-auto", {
    method: "POST", headers: { "Content-Type": "application/json" }, body: await request.text(),
  });
  return new Response(upstream.body, { status: upstream.status, headers: { "Content-Type": upstream.headers.get("Content-Type") ?? "application/json" } });
}

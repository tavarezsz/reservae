import { authenticatedFetch } from "@/src/lib/api-auth";

export async function GET(request: Request) {
  const query = new URL(request.url).search;
  const upstream = await authenticatedFetch(`/api/bookings/mine${query}`);
  return new Response(upstream.body, { status: upstream.status, headers: { "Content-Type": upstream.headers.get("Content-Type") ?? "application/json" } });
}

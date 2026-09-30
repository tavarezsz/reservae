import type { Metadata } from "next";
import { SiteShell } from "@/src/components/SiteShell";
import { MyBookings } from "@/src/components/MyBookings";

export const metadata: Metadata = {
  title: "Minhas reservas | reservaê",
  description: "Consulte seus próximos horários e reservas anteriores.",
};

export default function BookingsPage() {
  return <SiteShell active="bookings"><MyBookings /></SiteShell>;
}

import type { ReactNode, SVGProps } from "react";

type IconName = "search" | "arrow" | "pin" | "calendar" | "grid" | "space";
const paths: Record<IconName, ReactNode> = {
  search: <><circle cx="10.5" cy="10.5" r="6.5" /><path d="m16 16 4 4" /></>,
  arrow: <path d="M4 12h16m-6-6 6 6-6 6" />,
  pin: <><path d="M19 10c0 5-7 11-7 11S5 15 5 10a7 7 0 1 1 14 0Z" /><circle cx="12" cy="10" r="2" /></>,
  calendar: <><rect x="4" y="5" width="16" height="16" rx="3" /><path d="M8 3v4m8-4v4M4 11h16m-12 4h3" /></>,
  grid: <><rect x="3" y="3" width="7" height="7" rx="2" /><rect x="14" y="3" width="7" height="7" rx="2" /><rect x="3" y="14" width="7" height="7" rx="2" /><rect x="14" y="14" width="7" height="7" rx="2" /></>,
  space: <><path d="M4 21V8l8-5 8 5v13M2 21h20M9 21v-6h6v6M8 9h1m6 0h1" /></>,
};

export function Icon({ name, className = "", ...props }: SVGProps<SVGSVGElement> & { name: IconName }) {
  return <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" className={`size-5 ${className}`} {...props}>{paths[name]}</svg>;
}

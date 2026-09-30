import type { Metadata } from "next";
import { Manrope } from "next/font/google";
import "./globals.css";

const manrope = Manrope({
  subsets: ['latin'],
  display: 'swap',
  // Optional: Define a CSS variable name if you are using Tailwind CSS
  variable: '--font-manrope', 
});
export const metadata: Metadata = {
  title: "reservaê | Um lugar para cada plano",
  description: "Encontre espaços para trabalhar, jogar ou se reunir. Seu próximo plano tem lugar aqui.",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html
      lang="pt"
      className={`${manrope.variable}  h-full antialiased`}
    >
      <body className="min-h-full flex flex-col font-sans">{children}</body>
    </html>
  );
}

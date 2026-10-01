"use server";

import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import { accessTokenCookie } from "./api-auth";

export async function logout() {
  (await cookies()).delete(accessTokenCookie);
  redirect("/");
}

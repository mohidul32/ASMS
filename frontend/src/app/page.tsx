"use client";
import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { getRole, getDashboardPath } from "@/lib/auth";

export default function Home() {
  const router = useRouter();
  useEffect(() => {
    const role = getRole();
    router.replace(role ? getDashboardPath(role) : "/login");
  }, [router]);
  return null;
}

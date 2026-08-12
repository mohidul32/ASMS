import { AuthResponse } from "@/types";

export const saveAuth = (data: AuthResponse) => {
  localStorage.setItem("token", data.token);
  localStorage.setItem("role", data.role);
  localStorage.setItem("name", data.name);
  localStorage.setItem("userId", data.id.toString());
};

export const getRole = () => (typeof window !== "undefined" ? localStorage.getItem("role") : null);
export const getName = () => (typeof window !== "undefined" ? localStorage.getItem("name") : null);
export const getToken = () => (typeof window !== "undefined" ? localStorage.getItem("token") : null);

export const logout = () => {
  localStorage.clear();
  window.location.href = "/login";
};

export const getDashboardPath = (role: string) => {
  if (role === "Admin") return "/dashboard/admin";
  if (role === "Teacher") return "/dashboard/teacher";
  return "/dashboard/student";
};

"use client";
import { getName, getRole, logout } from "@/lib/auth";

export default function Navbar() {
  return (
    <nav className="bg-blue-700 text-white px-6 py-3 flex justify-between items-center">
      <span className="font-bold text-lg">ASMS</span>
      <div className="flex items-center gap-4">
        <span className="text-sm">{getName()} <span className="bg-blue-500 px-2 py-0.5 rounded text-xs">{getRole()}</span></span>
        <button onClick={logout} className="bg-white text-blue-700 px-3 py-1 rounded text-sm font-medium hover:bg-blue-50">
          Logout
        </button>
      </div>
    </nav>
  );
}

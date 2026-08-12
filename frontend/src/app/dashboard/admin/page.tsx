"use client";
import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import api from "@/lib/api";
import { getRole } from "@/lib/auth";
import Navbar from "@/components/Navbar";
import Modal from "@/components/Modal";
import { User, Class, Subject } from "@/types";

type Tab = "users" | "classes" | "subjects";
type UserForm = { name: string; email: string; password: string; role: string };
type ClassForm = { name: string; description: string };
type SubjectForm = { name: string; classId: number; teacherId: number };

export default function AdminDashboard() {
  const router = useRouter();
  const [tab, setTab] = useState<Tab>("users");
  const [users, setUsers] = useState<User[]>([]);
  const [classes, setClasses] = useState<Class[]>([]);
  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [modal, setModal] = useState<string | null>(null);
  const [error, setError] = useState("");

  const userForm = useForm<UserForm>();
  const classForm = useForm<ClassForm>();
  const subjectForm = useForm<SubjectForm>();

  useEffect(() => {
    if (getRole() !== "Admin") { router.replace("/login"); return; }
    fetchAll();
  }, [router]);

  const fetchAll = async () => {
    const [u, c, s] = await Promise.all([api.get("/api/users"), api.get("/api/classes"), api.get("/api/subjects")]);
    setUsers(u.data); setClasses(c.data); setSubjects(s.data);
  };

  const createUser = async (data: UserForm) => {
    try { await api.post("/api/users", data); setModal(null); userForm.reset(); fetchAll(); }
    catch (e: any) { setError(e.response?.data?.message || "Error"); }
  };

  const deleteUser = async (id: number) => {
    if (!confirm("Delete this user?")) return;
    await api.delete(`/api/users/${id}`); fetchAll();
  };

  const createClass = async (data: ClassForm) => {
    try { await api.post("/api/classes", data); setModal(null); classForm.reset(); fetchAll(); }
    catch (e: any) { setError(e.response?.data?.message || "Error"); }
  };

  const deleteClass = async (id: number) => {
    if (!confirm("Delete this class?")) return;
    await api.delete(`/api/classes/${id}`); fetchAll();
  };

  const createSubject = async (data: SubjectForm) => {
    try { await api.post("/api/subjects", data); setModal(null); subjectForm.reset(); fetchAll(); }
    catch (e: any) { setError(e.response?.data?.message || "Error"); }
  };

  const deleteSubject = async (id: number) => {
    if (!confirm("Delete this subject?")) return;
    await api.delete(`/api/subjects/${id}`); fetchAll();
  };

  const tabs: Tab[] = ["users", "classes", "subjects"];

  return (
    <div>
      <Navbar />
      <div className="max-w-5xl mx-auto p-6">
        <div className="flex gap-2 mb-6">
          {tabs.map(t => (
            <button key={t} onClick={() => setTab(t)}
              className={`px-4 py-2 rounded-lg capitalize font-medium ${tab === t ? "bg-blue-700 text-white" : "bg-white border"}`}>
              {t}
            </button>
          ))}
        </div>

        {/* Users Tab */}
        {tab === "users" && (
          <div>
            <div className="flex justify-between items-center mb-4">
              <h2 className="text-xl font-semibold">Users</h2>
              <button onClick={() => { setError(""); setModal("user"); }} className="bg-blue-700 text-white px-4 py-2 rounded-lg text-sm">+ Add User</button>
            </div>
            <div className="bg-white rounded-xl shadow overflow-hidden">
              <table className="w-full text-sm">
                <thead className="bg-gray-50 text-gray-600"><tr>
                  <th className="px-4 py-3 text-left">Name</th><th className="px-4 py-3 text-left">Email</th>
                  <th className="px-4 py-3 text-left">Role</th><th className="px-4 py-3 text-left">Status</th>
                  <th className="px-4 py-3 text-left">Actions</th>
                </tr></thead>
                <tbody>{users.map(u => (
                  <tr key={u.id} className="border-t">
                    <td className="px-4 py-3">{u.name}</td><td className="px-4 py-3">{u.email}</td>
                    <td className="px-4 py-3"><span className="bg-blue-100 text-blue-700 px-2 py-0.5 rounded text-xs">{u.role}</span></td>
                    <td className="px-4 py-3"><span className={`px-2 py-0.5 rounded text-xs ${u.isActive ? "bg-green-100 text-green-700" : "bg-red-100 text-red-700"}`}>{u.isActive ? "Active" : "Inactive"}</span></td>
                    <td className="px-4 py-3"><button onClick={() => deleteUser(u.id)} className="text-red-500 hover:underline text-xs">Delete</button></td>
                  </tr>
                ))}</tbody>
              </table>
            </div>
          </div>
        )}

        {/* Classes Tab */}
        {tab === "classes" && (
          <div>
            <div className="flex justify-between items-center mb-4">
              <h2 className="text-xl font-semibold">Classes</h2>
              <button onClick={() => { setError(""); setModal("class"); }} className="bg-blue-700 text-white px-4 py-2 rounded-lg text-sm">+ Add Class</button>
            </div>
            <div className="bg-white rounded-xl shadow overflow-hidden">
              <table className="w-full text-sm">
                <thead className="bg-gray-50 text-gray-600"><tr>
                  <th className="px-4 py-3 text-left">Name</th><th className="px-4 py-3 text-left">Description</th>
                  <th className="px-4 py-3 text-left">Actions</th>
                </tr></thead>
                <tbody>{classes.map(c => (
                  <tr key={c.id} className="border-t">
                    <td className="px-4 py-3">{c.name}</td><td className="px-4 py-3">{c.description || "-"}</td>
                    <td className="px-4 py-3"><button onClick={() => deleteClass(c.id)} className="text-red-500 hover:underline text-xs">Delete</button></td>
                  </tr>
                ))}</tbody>
              </table>
            </div>
          </div>
        )}

        {/* Subjects Tab */}
        {tab === "subjects" && (
          <div>
            <div className="flex justify-between items-center mb-4">
              <h2 className="text-xl font-semibold">Subjects</h2>
              <button onClick={() => { setError(""); setModal("subject"); }} className="bg-blue-700 text-white px-4 py-2 rounded-lg text-sm">+ Add Subject</button>
            </div>
            <div className="bg-white rounded-xl shadow overflow-hidden">
              <table className="w-full text-sm">
                <thead className="bg-gray-50 text-gray-600"><tr>
                  <th className="px-4 py-3 text-left">Name</th><th className="px-4 py-3 text-left">Class</th>
                  <th className="px-4 py-3 text-left">Teacher</th><th className="px-4 py-3 text-left">Actions</th>
                </tr></thead>
                <tbody>{subjects.map(s => (
                  <tr key={s.id} className="border-t">
                    <td className="px-4 py-3">{s.name}</td><td className="px-4 py-3">{s.className}</td>
                    <td className="px-4 py-3">{s.teacherName || "-"}</td>
                    <td className="px-4 py-3"><button onClick={() => deleteSubject(s.id)} className="text-red-500 hover:underline text-xs">Delete</button></td>
                  </tr>
                ))}</tbody>
              </table>
            </div>
          </div>
        )}
      </div>

      {/* Add User Modal */}
      {modal === "user" && (
        <Modal title="Add User" onClose={() => setModal(null)}>
          <form onSubmit={userForm.handleSubmit(createUser)} className="space-y-3">
            {error && <p className="text-red-500 text-sm">{error}</p>}
            <input {...userForm.register("name", { required: true })} placeholder="Name" className="w-full border rounded-lg px-3 py-2 text-sm" />
            <input {...userForm.register("email", { required: true })} placeholder="Email" className="w-full border rounded-lg px-3 py-2 text-sm" />
            <input type="password" {...userForm.register("password", { required: true })} placeholder="Password" className="w-full border rounded-lg px-3 py-2 text-sm" />
            <select {...userForm.register("role", { required: true })} className="w-full border rounded-lg px-3 py-2 text-sm">
              <option value="">Select Role</option>
              <option value="Admin">Admin</option>
              <option value="Teacher">Teacher</option>
              <option value="Student">Student</option>
            </select>
            <button type="submit" className="w-full bg-blue-700 text-white py-2 rounded-lg text-sm">Create</button>
          </form>
        </Modal>
      )}

      {/* Add Class Modal */}
      {modal === "class" && (
        <Modal title="Add Class" onClose={() => setModal(null)}>
          <form onSubmit={classForm.handleSubmit(createClass)} className="space-y-3">
            {error && <p className="text-red-500 text-sm">{error}</p>}
            <input {...classForm.register("name", { required: true })} placeholder="Class Name" className="w-full border rounded-lg px-3 py-2 text-sm" />
            <input {...classForm.register("description")} placeholder="Description (optional)" className="w-full border rounded-lg px-3 py-2 text-sm" />
            <button type="submit" className="w-full bg-blue-700 text-white py-2 rounded-lg text-sm">Create</button>
          </form>
        </Modal>
      )}

      {/* Add Subject Modal */}
      {modal === "subject" && (
        <Modal title="Add Subject" onClose={() => setModal(null)}>
          <form onSubmit={subjectForm.handleSubmit(createSubject)} className="space-y-3">
            {error && <p className="text-red-500 text-sm">{error}</p>}
            <input {...subjectForm.register("name", { required: true })} placeholder="Subject Name" className="w-full border rounded-lg px-3 py-2 text-sm" />
            <select {...subjectForm.register("classId", { required: true, valueAsNumber: true })} className="w-full border rounded-lg px-3 py-2 text-sm">
              <option value="">Select Class</option>
              {classes.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
            <select {...subjectForm.register("teacherId", { valueAsNumber: true })} className="w-full border rounded-lg px-3 py-2 text-sm">
              <option value="">Select Teacher (optional)</option>
              {users.filter(u => u.role === "Teacher").map(u => <option key={u.id} value={u.id}>{u.name}</option>)}
            </select>
            <button type="submit" className="w-full bg-blue-700 text-white py-2 rounded-lg text-sm">Create</button>
          </form>
        </Modal>
      )}
    </div>
  );
}

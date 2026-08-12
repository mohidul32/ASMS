"use client";
import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import api from "@/lib/api";
import { getRole } from "@/lib/auth";
import Navbar from "@/components/Navbar";
import Modal from "@/components/Modal";
import { Assignment, Submission, Class, Subject } from "@/types";

type Tab = "assignments" | "submissions";
type AssignmentForm = { title: string; description: string; subjectId: number; classId: number; deadline: string; maxMarks: number; isPublished: boolean; allowLateUpdate: boolean };
type GradeForm = { marks: number; feedback: string; status: string };

export default function TeacherDashboard() {
  const router = useRouter();
  const [tab, setTab] = useState<Tab>("assignments");
  const [assignments, setAssignments] = useState<Assignment[]>([]);
  const [submissions, setSubmissions] = useState<Submission[]>([]);
  const [classes, setClasses] = useState<Class[]>([]);
  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [modal, setModal] = useState<string | null>(null);
  const [selected, setSelected] = useState<Assignment | null>(null);
  const [selectedSub, setSelectedSub] = useState<Submission | null>(null);
  const [error, setError] = useState("");

  const aForm = useForm<AssignmentForm>();
  const gForm = useForm<GradeForm>();

  useEffect(() => {
    if (getRole() !== "Teacher") { router.replace("/login"); return; }
    fetchAll();
  }, [router]);

  const fetchAll = async () => {
    const [a, s, c, sub] = await Promise.all([
      api.get("/api/assignments"), api.get("/api/submissions"),
      api.get("/api/classes"), api.get("/api/subjects")
    ]);
    setAssignments(a.data); setSubmissions(s.data); setClasses(c.data); setSubjects(sub.data);
  };

  const createAssignment = async (data: AssignmentForm) => {
    try {
      await api.post("/api/assignments", { ...data, subjectId: Number(data.subjectId), classId: Number(data.classId), maxMarks: Number(data.maxMarks) });
      setModal(null); aForm.reset(); fetchAll();
    } catch (e: any) { setError(e.response?.data?.message || "Error"); }
  };

  const updateAssignment = async (data: AssignmentForm) => {
    try {
      await api.put(`/api/assignments/${selected!.id}`, { ...data, maxMarks: Number(data.maxMarks) });
      setModal(null); aForm.reset(); setSelected(null); fetchAll();
    } catch (e: any) { setError(e.response?.data?.message || "Error"); }
  };

  const deleteAssignment = async (id: number) => {
    if (!confirm("Delete this assignment?")) return;
    await api.delete(`/api/assignments/${id}`); fetchAll();
  };

  const openEdit = (a: Assignment) => {
    setSelected(a); setError("");
    aForm.reset({ ...a, deadline: a.deadline.slice(0, 16) });
    setModal("edit");
  };

  const gradeSubmission = async (data: GradeForm) => {
    try {
      await api.put(`/api/submissions/${selectedSub!.id}/grade`, { ...data, marks: Number(data.marks) });
      setModal(null); gForm.reset(); setSelectedSub(null); fetchAll();
    } catch (e: any) { setError(e.response?.data?.message || "Error"); }
  };

  return (
    <div>
      <Navbar />
      <div className="max-w-5xl mx-auto p-6">
        <div className="flex gap-2 mb-6">
          {(["assignments", "submissions"] as Tab[]).map(t => (
            <button key={t} onClick={() => setTab(t)}
              className={`px-4 py-2 rounded-lg capitalize font-medium ${tab === t ? "bg-blue-700 text-white" : "bg-white border"}`}>
              {t}
            </button>
          ))}
        </div>

        {/* Assignments Tab */}
        {tab === "assignments" && (
          <div>
            <div className="flex justify-between items-center mb-4">
              <h2 className="text-xl font-semibold">Assignments</h2>
              <button onClick={() => { setError(""); aForm.reset(); setModal("create"); }} className="bg-blue-700 text-white px-4 py-2 rounded-lg text-sm">+ New Assignment</button>
            </div>
            <div className="bg-white rounded-xl shadow overflow-hidden">
              <table className="w-full text-sm">
                <thead className="bg-gray-50 text-gray-600"><tr>
                  <th className="px-4 py-3 text-left">Title</th><th className="px-4 py-3 text-left">Class</th>
                  <th className="px-4 py-3 text-left">Deadline</th><th className="px-4 py-3 text-left">Status</th>
                  <th className="px-4 py-3 text-left">Actions</th>
                </tr></thead>
                <tbody>{assignments.map(a => (
                  <tr key={a.id} className="border-t">
                    <td className="px-4 py-3">{a.title}</td><td className="px-4 py-3">{a.className}</td>
                    <td className="px-4 py-3">{new Date(a.deadline).toLocaleDateString()}</td>
                    <td className="px-4 py-3"><span className={`px-2 py-0.5 rounded text-xs ${a.isPublished ? "bg-green-100 text-green-700" : "bg-yellow-100 text-yellow-700"}`}>{a.isPublished ? "Published" : "Draft"}</span></td>
                    <td className="px-4 py-3 flex gap-2">
                      <button onClick={() => openEdit(a)} className="text-blue-600 hover:underline text-xs">Edit</button>
                      <button onClick={() => deleteAssignment(a.id)} className="text-red-500 hover:underline text-xs">Delete</button>
                    </td>
                  </tr>
                ))}</tbody>
              </table>
            </div>
          </div>
        )}

        {/* Submissions Tab */}
        {tab === "submissions" && (
          <div>
            <h2 className="text-xl font-semibold mb-4">Submissions</h2>
            <div className="bg-white rounded-xl shadow overflow-hidden">
              <table className="w-full text-sm">
                <thead className="bg-gray-50 text-gray-600"><tr>
                  <th className="px-4 py-3 text-left">Assignment</th><th className="px-4 py-3 text-left">Student</th>
                  <th className="px-4 py-3 text-left">Status</th><th className="px-4 py-3 text-left">Marks</th>
                  <th className="px-4 py-3 text-left">Actions</th>
                </tr></thead>
                <tbody>{submissions.map(s => (
                  <tr key={s.id} className="border-t">
                    <td className="px-4 py-3">{s.assignmentTitle}</td><td className="px-4 py-3">{s.studentName}</td>
                    <td className="px-4 py-3"><span className="bg-blue-100 text-blue-700 px-2 py-0.5 rounded text-xs">{s.status}</span></td>
                    <td className="px-4 py-3">{s.marks ?? "-"}</td>
                    <td className="px-4 py-3">
                      <button onClick={() => { setSelectedSub(s); setError(""); gForm.reset({ marks: s.marks ?? 0, feedback: s.feedback ?? "", status: s.status }); setModal("grade"); }}
                        className="text-blue-600 hover:underline text-xs">Grade</button>
                    </td>
                  </tr>
                ))}</tbody>
              </table>
            </div>
          </div>
        )}
      </div>

      {/* Create Assignment Modal */}
      {(modal === "create" || modal === "edit") && (
        <Modal title={modal === "create" ? "New Assignment" : "Edit Assignment"} onClose={() => setModal(null)}>
          <form onSubmit={aForm.handleSubmit(modal === "create" ? createAssignment : updateAssignment)} className="space-y-3">
            {error && <p className="text-red-500 text-sm">{error}</p>}
            <input {...aForm.register("title", { required: true })} placeholder="Title" className="w-full border rounded-lg px-3 py-2 text-sm" />
            <textarea {...aForm.register("description", { required: true })} placeholder="Description" className="w-full border rounded-lg px-3 py-2 text-sm" rows={3} />
            <select {...aForm.register("classId", { required: true })} className="w-full border rounded-lg px-3 py-2 text-sm">
              <option value="">Select Class</option>
              {classes.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
            <select {...aForm.register("subjectId", { required: true })} className="w-full border rounded-lg px-3 py-2 text-sm">
              <option value="">Select Subject</option>
              {subjects.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
            </select>
            <input type="datetime-local" {...aForm.register("deadline", { required: true })} className="w-full border rounded-lg px-3 py-2 text-sm" />
            <input type="number" {...aForm.register("maxMarks", { required: true })} placeholder="Max Marks" className="w-full border rounded-lg px-3 py-2 text-sm" />
            <div className="flex gap-4 text-sm">
              <label className="flex items-center gap-2"><input type="checkbox" {...aForm.register("isPublished")} /> Publish</label>
              <label className="flex items-center gap-2"><input type="checkbox" {...aForm.register("allowLateUpdate")} /> Allow Late Update</label>
            </div>
            <button type="submit" className="w-full bg-blue-700 text-white py-2 rounded-lg text-sm">{modal === "create" ? "Create" : "Update"}</button>
          </form>
        </Modal>
      )}

      {/* Grade Modal */}
      {modal === "grade" && selectedSub && (
        <Modal title="Grade Submission" onClose={() => setModal(null)}>
          <div className="mb-3 p-3 bg-gray-50 rounded text-sm">
            <p><strong>Student:</strong> {selectedSub.studentName}</p>
            <p className="mt-1"><strong>Answer:</strong> {selectedSub.answer}</p>
          </div>
          <form onSubmit={gForm.handleSubmit(gradeSubmission)} className="space-y-3">
            {error && <p className="text-red-500 text-sm">{error}</p>}
            <input type="number" {...gForm.register("marks", { required: true })} placeholder="Marks" className="w-full border rounded-lg px-3 py-2 text-sm" />
            <textarea {...gForm.register("feedback")} placeholder="Feedback (optional)" className="w-full border rounded-lg px-3 py-2 text-sm" rows={3} />
            <select {...gForm.register("status", { required: true })} className="w-full border rounded-lg px-3 py-2 text-sm">
              <option value="Submitted">Submitted</option>
              <option value="Reviewed">Reviewed</option>
              <option value="Returned">Returned</option>
            </select>
            <button type="submit" className="w-full bg-blue-700 text-white py-2 rounded-lg text-sm">Save Grade</button>
          </form>
        </Modal>
      )}
    </div>
  );
}

"use client";
import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import api from "@/lib/api";
import { getRole } from "@/lib/auth";
import Navbar from "@/components/Navbar";
import Modal from "@/components/Modal";
import { Assignment, Submission } from "@/types";

type Tab = "assignments" | "submissions";
type SubmitForm = { answer: string };

export default function StudentDashboard() {
  const router = useRouter();
  const [tab, setTab] = useState<Tab>("assignments");
  const [assignments, setAssignments] = useState<Assignment[]>([]);
  const [submissions, setSubmissions] = useState<Submission[]>([]);
  const [modal, setModal] = useState<string | null>(null);
  const [selected, setSelected] = useState<Assignment | null>(null);
  const [selectedSub, setSelectedSub] = useState<Submission | null>(null);
  const [error, setError] = useState("");

  const form = useForm<SubmitForm>();

  useEffect(() => {
    if (getRole() !== "Student") { router.replace("/login"); return; }
    fetchAll();
  }, [router]);

  const fetchAll = async () => {
    const [a, s] = await Promise.all([api.get("/api/assignments"), api.get("/api/submissions")]);
    setAssignments(a.data); setSubmissions(s.data);
  };

  const hasSubmitted = (assignmentId: number) => submissions.some(s => s.assignmentId === assignmentId);
  const getSubmission = (assignmentId: number) => submissions.find(s => s.assignmentId === assignmentId);
  const isPastDeadline = (deadline: string) => new Date(deadline) < new Date();

  const submitAnswer = async (data: SubmitForm) => {
    try {
      await api.post("/api/submissions", { assignmentId: selected!.id, answer: data.answer });
      setModal(null); form.reset(); fetchAll();
    } catch (e: any) { setError(e.response?.data?.message || "Error"); }
  };

  const updateAnswer = async (data: SubmitForm) => {
    try {
      await api.put(`/api/submissions/${selectedSub!.id}`, { answer: data.answer });
      setModal(null); form.reset(); fetchAll();
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
            <h2 className="text-xl font-semibold mb-4">My Assignments</h2>
            <div className="grid gap-4">
              {assignments.map(a => {
                const submitted = hasSubmitted(a.id);
                const sub = getSubmission(a.id);
                const past = isPastDeadline(a.deadline);
                return (
                  <div key={a.id} className="bg-white rounded-xl shadow p-5">
                    <div className="flex justify-between items-start">
                      <div>
                        <h3 className="font-semibold text-lg">{a.title}</h3>
                        <p className="text-gray-500 text-sm mt-1">{a.description}</p>
                        <div className="flex gap-3 mt-2 text-xs text-gray-500">
                          <span>📚 {a.subjectName}</span>
                          <span>🏫 {a.className}</span>
                          <span>🎯 Max: {a.maxMarks}</span>
                          <span className={past ? "text-red-500" : "text-green-600"}>
                            ⏰ {new Date(a.deadline).toLocaleString()}
                          </span>
                        </div>
                      </div>
                      <div className="flex flex-col items-end gap-2">
                        {submitted
                          ? <span className="bg-green-100 text-green-700 px-2 py-0.5 rounded text-xs">Submitted</span>
                          : <span className="bg-yellow-100 text-yellow-700 px-2 py-0.5 rounded text-xs">Pending</span>}
                        {!submitted && !past && (
                          <button onClick={() => { setSelected(a); setError(""); form.reset(); setModal("submit"); }}
                            className="bg-blue-700 text-white px-3 py-1 rounded text-xs">Submit</button>
                        )}
                        {submitted && (a.allowLateUpdate || !past) && (
                          <button onClick={() => { setSelectedSub(sub!); setError(""); form.reset({ answer: sub!.answer }); setModal("update"); }}
                            className="bg-gray-200 text-gray-700 px-3 py-1 rounded text-xs">Update</button>
                        )}
                      </div>
                    </div>
                  </div>
                );
              })}
              {assignments.length === 0 && <p className="text-gray-500 text-center py-8">No assignments yet.</p>}
            </div>
          </div>
        )}

        {/* Submissions Tab */}
        {tab === "submissions" && (
          <div>
            <h2 className="text-xl font-semibold mb-4">My Submissions</h2>
            <div className="grid gap-4">
              {submissions.map(s => (
                <div key={s.id} className="bg-white rounded-xl shadow p-5">
                  <div className="flex justify-between items-start">
                    <div>
                      <h3 className="font-semibold">{s.assignmentTitle}</h3>
                      <p className="text-gray-600 text-sm mt-1">{s.answer}</p>
                    </div>
                    <span className={`px-2 py-0.5 rounded text-xs ${s.status === "Reviewed" ? "bg-green-100 text-green-700" : s.status === "Returned" ? "bg-red-100 text-red-700" : "bg-blue-100 text-blue-700"}`}>{s.status}</span>
                  </div>
                  {s.marks !== undefined && s.marks !== null && (
                    <div className="mt-3 p-3 bg-gray-50 rounded text-sm">
                      <p><strong>Marks:</strong> {s.marks}</p>
                      {s.feedback && <p className="mt-1"><strong>Feedback:</strong> {s.feedback}</p>}
                    </div>
                  )}
                  <p className="text-xs text-gray-400 mt-2">Submitted: {new Date(s.submittedAt).toLocaleString()}</p>
                </div>
              ))}
              {submissions.length === 0 && <p className="text-gray-500 text-center py-8">No submissions yet.</p>}
            </div>
          </div>
        )}
      </div>

      {/* Submit Modal */}
      {modal === "submit" && selected && (
        <Modal title={`Submit: ${selected.title}`} onClose={() => setModal(null)}>
          <form onSubmit={form.handleSubmit(submitAnswer)} className="space-y-3">
            {error && <p className="text-red-500 text-sm">{error}</p>}
            <textarea {...form.register("answer", { required: "Answer is required" })} placeholder="Write your answer here..." className="w-full border rounded-lg px-3 py-2 text-sm" rows={5} />
            {form.formState.errors.answer && <p className="text-red-500 text-xs">{form.formState.errors.answer.message}</p>}
            <button type="submit" className="w-full bg-blue-700 text-white py-2 rounded-lg text-sm">Submit</button>
          </form>
        </Modal>
      )}

      {/* Update Modal */}
      {modal === "update" && selectedSub && (
        <Modal title="Update Submission" onClose={() => setModal(null)}>
          <form onSubmit={form.handleSubmit(updateAnswer)} className="space-y-3">
            {error && <p className="text-red-500 text-sm">{error}</p>}
            <textarea {...form.register("answer", { required: "Answer is required" })} className="w-full border rounded-lg px-3 py-2 text-sm" rows={5} />
            {form.formState.errors.answer && <p className="text-red-500 text-xs">{form.formState.errors.answer.message}</p>}
            <button type="submit" className="w-full bg-blue-700 text-white py-2 rounded-lg text-sm">Update</button>
          </form>
        </Modal>
      )}
    </div>
  );
}

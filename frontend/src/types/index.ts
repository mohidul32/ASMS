export interface AuthResponse {
  token: string;
  role: string;
  name: string;
  id: number;
}

export interface User {
  id: number;
  name: string;
  email: string;
  role: string;
  isActive: boolean;
  createdAt: string;
}

export interface Class {
  id: number;
  name: string;
  description?: string;
  isActive: boolean;
}

export interface Subject {
  id: number;
  name: string;
  classId: number;
  className: string;
  teacherId?: number;
  teacherName?: string;
}

export interface Assignment {
  id: number;
  title: string;
  description: string;
  subjectId: number;
  subjectName: string;
  classId: number;
  className: string;
  teacherId: number;
  teacherName: string;
  deadline: string;
  maxMarks: number;
  isPublished: boolean;
  allowLateUpdate: boolean;
  createdAt: string;
}

export interface Submission {
  id: number;
  assignmentId: number;
  assignmentTitle: string;
  studentId: number;
  studentName: string;
  answer: string;
  status: string;
  marks?: number;
  feedback?: string;
  submittedAt: string;
  updatedAt?: string;
}

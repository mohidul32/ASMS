# Assignment & Submission Management System (ASMS)

A role-based full-stack web application for managing assignments and submissions in a school/college environment.

## Features

- **Admin**: Manage users, classes, subjects, and teacher assignments
- **Teacher**: Create/edit/delete assignments (draft or published), review and grade student submissions
- **Student**: View published assignments, submit answers, update submissions (if allowed), view marks and feedback
- **JWT-based authentication** with role-based authorization
- **Swagger UI** for API exploration

## Technology Stack

| Layer | Technology |
|-------|-----------|
| Frontend | Next.js 15, React, TypeScript, Tailwind CSS |
| Backend | ASP.NET Core 8 Web API, C# |
| Database | PostgreSQL 16 with EF Core 8 |
| Auth | JWT Bearer tokens |
| Testing | xUnit, EF Core InMemory |

## Project Structure

```
ASMS/
├── backend/
│   ├── ASMS.API/               # ASP.NET Core Web API
│   │   ├── Controllers/        # API endpoints
│   │   ├── Data/               # DbContext
│   │   ├── DTOs/               # Request/Response models
│   │   ├── Migrations/         # EF Core migrations
│   │   ├── Models/             # Entity models
│   │   ├── Services/           # JwtService
│   │   └── appsettings.json
│   └── ASMS.Tests/             # xUnit unit tests
└── frontend/
    └── src/
        ├── app/                # Next.js app router pages
        │   ├── login/
        │   └── dashboard/
        │       ├── admin/
        │       ├── teacher/
        │       └── student/
        ├── components/         # Shared UI components
        ├── lib/                # API client, auth helpers
        └── types/              # TypeScript interfaces
```

## Prerequisites

- .NET SDK 8.0
- Node.js 20+
- PostgreSQL 16

## Database Setup

1. Start PostgreSQL and open psql:
```bash
sudo -u postgres psql
```

2. Run the following:
```sql
CREATE DATABASE asms_db;
CREATE USER <db_user> WITH PASSWORD '<db_password>';
GRANT ALL PRIVILEGES ON DATABASE asms_db TO <db_user>;
\q
```

3. Grant schema permissions:
```bash
sudo -u postgres psql -d asms_db -c "GRANT ALL ON SCHEMA public TO <db_user>;"
```

## Backend Setup

1. Navigate to the API project:
```bash
cd backend/ASMS.API
```

2. Copy and configure environment:
```bash
cp .env.example appsettings.json  # or edit appsettings.json directly
```

3. Update `appsettings.json` with your DB credentials:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=asms_db;Username=<db_user>;Password=<db_password>"
}
```

4. Install dotnet-ef tool (if not installed):
```bash
dotnet tool install --global dotnet-ef
export PATH="$PATH:$HOME/.dotnet/tools"
```

5. Apply migrations and seed data:
```bash
dotnet ef database update
```

6. Run the API:
```bash
dotnet run
```

API runs at: `http://localhost:5000`
Swagger UI: `http://localhost:5000/swagger`

## Frontend Setup

1. Navigate to the frontend:
```bash
cd frontend
```

2. Install dependencies:
```bash
npm install
```

3. Create environment file:
```bash
cp .env.example .env.local
```

4. Run the development server:
```bash
npm run dev
```

Frontend runs at: `http://localhost:3000`

## Running Tests

```bash
cd backend/ASMS.Tests
dotnet test
```

Expected output: **16 tests passing**

Tests cover:
- Auth: valid login, wrong password, unknown email, inactive user
- Submissions: submit, past deadline, duplicate, late update, grading rules, authorization
- Assignments: create, ownership enforcement, student visibility rules

## Demo Credentials

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@asms.com | Admin@123 |
| Teacher | teacher@asms.com | Teacher@123 |
| Student | student@asms.com | Student@123 |

## API Endpoints

| Method | Endpoint | Role |
|--------|----------|------|
| POST | /api/auth/login | Public |
| GET/POST/PUT/DELETE | /api/users | Admin |
| GET/POST/PUT/DELETE | /api/classes | Admin (write), All (read) |
| GET/POST/PUT/DELETE | /api/subjects | Admin (write), All (read) |
| GET/POST/PUT/DELETE | /api/assignments | Teacher (write), Student (read) |
| GET/POST/PUT | /api/submissions | Student (submit), Teacher (grade) |
| PUT | /api/submissions/{id}/grade | Teacher |

## Assumptions

- A student can only submit once per assignment
- Students can update their submission before the deadline; after the deadline only if `AllowLateUpdate` is enabled on the assignment
- Teachers can only edit/delete their own assignments
- Teachers can only grade submissions for their own assignments
- Marks cannot exceed the assignment's `MaxMarks`
- Only published assignments are visible to students
- Admin can view all assignments and submissions
- Inactive users cannot log in

## Known Limitations

- No file upload support for submissions (text-only answers)
- No email notifications
- No pagination on list endpoints
- Password reset not implemented

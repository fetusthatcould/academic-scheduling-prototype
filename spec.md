# Project Specification: Academic Scheduling Prototype

## 1. Project Overview
Build a functional scheduling system prototype for academic staff to assign students to courses for an upcoming semester.
- **Framework:** .NET 8 Blazor Server (`InteractiveServer`)
- **Time Constraints:** Designed for a 4-hour take-home assignment; prioritize simplicity, robustness, and clean execution.
- **Styling:** Use standard Bootstrap 5 (included in default Blazor template). No custom CSS or inline styles.
- **Execution:** Must run cleanly with `dotnet run` on a fresh machine.

## 2. Architecture & Data Layer Constraints
- **NO DATABASE:** Do not use Entity Framework, SQL Server, SQLite, or ORMs.
- **In-Memory Storage:** Implement a single `SchedulingService.cs` concrete class registered as a Singleton in `Program.cs`.
- **Concurrency & Thread Safety:** Include a `private readonly object _syncRoot = new();` inside `SchedulingService` to synchronize mutations across concurrent Blazor circuit threads.
- **UI Reactivity:** Expose `public event Action? OnStateChanged;` on `SchedulingService` to notify active components of state updates.
- **Data Seeding:** Use `CsvHelper` to parse `students-starter.csv` and `courses-starter.csv` inside the `SchedulingService` constructor.
- **Dependencies:** `CsvHelper` is the only authorized external NuGet package.

## 3. Domain Models
Place all classes in the `Models/` folder:

### Course
- `Id` (string)
- `Name` (string)
- `Department` (string)
- `Instructor` (string)
- `Credits` (int)
- `MaxCapacity` (int)
- `PrerequisitesRaw` (string) - Raw comma-separated string from CSV
- `EnrolledCount` (int) - Default 0
- `AvailableSeats` (int) - Calculated: `MaxCapacity - EnrolledCount`

### Student
- `Id` (string)
- `Name` (string)
- `YearLevel` (string)
- `CompletedCoursesRaw` (string) - Raw comma-separated string from CSV
- `AssignedCourses` (List<Course>) - Initializes empty list
- `CurrentCreditLoad` (int) - Calculated: Sum of credits in `AssignedCourses`

### AssignmentResult
- `IsSuccess` (bool)
- `ErrorMessage` (string) - Null or empty on success; populated on validation failure

## 4. Core Business Logic (SchedulingService)
- **Properties:** `List<Student> Students`, `List<Course> Courses`
- **Method:** `AssignmentResult AssignStudent(string studentId, string courseId)`
- **Thread Safety:** Wrap state mutations inside `lock (_syncRoot) { ... }`.
- **Validation Rules (Evaluated in Order):**
  1. **Duplicate Assignment:** Fail if `Student.AssignedCourses` already contains `Course.Id`.
  2. **Seat Capacity:** Fail if `Course.AvailableSeats <= 0`.
  3. **Prerequisites:** Split `CompletedCoursesRaw` and `PrerequisitesRaw` using `.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)`. Fail if any prerequisite code is missing from completed courses. (Do NOT use basic `.Contains()` string matching).
  4. **Credit Limit:** Fail if `Student.CurrentCreditLoad + Course.Credits > 18`.
- **On Success:** Append course to `Student.AssignedCourses`, increment `Course.EnrolledCount`, invoke `OnStateChanged?.Invoke()`, and return `IsSuccess = true`.

## 5. UI Requirements (Blazor Components)
- **Lifecycle Management:** Components subscribing to `SchedulingService.OnStateChanged` MUST implement `IDisposable` to unsubscribe on component disposal (`OnStateChanged -= StateHasChanged`) to prevent memory leaks.
- **Student List View (`StudentList.razor`):** Data table displaying Student Name, Year Level, Completed Courses, and Current Credit Load.
- **Course Catalog (`CourseCatalog.razor`):** Data table displaying ID, Name, Department, Instructor, Credits, and Available Seats. Includes two-way bound text inputs for filtering by Department and Instructor.
- **Assignment Interface (`AssignmentDashboard.razor`):** Master-detail view. Select student -> view profile -> click "Assign" on eligible course -> display Bootstrap alert banner (`alert-success` / `alert-danger`).

## 6. Strict Output Constraints & Anti-Patterns (LLM Guardrails)
- **No Over-Engineering:** DO NOT use CQRS, MediatR, Repository patterns, or generic interfaces (`ISchedulingService`).
- **No Inline Styles:** Use default Bootstrap 5 utility classes exclusively (`table`, `btn`, `alert`).
- **Concise Naming:** Avoid overly verbose identifiers (e.g., use `HasMetPrereqs` instead of `CheckIfStudentHasCompletedAllPrerequisiteCourses`).
- **No Boilerplate Comments:** Omit conversational code comments, explanations, or basic C# tutorial syntax.
- **Clean Error Messages:** Return short, direct feedback strings (e.g., "Prerequisite missing: CS101", "Student already enrolled").
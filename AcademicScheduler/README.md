README: Academic Scheduling Prototype

Project Overview
A .NET 8 Blazor Server application engineered as an interactive, master-detail academic scheduling dashboard. This prototype ingests legacy CSV data and enforces strict university business rules through a centralized validation engine, providing instant, reactive UI feedback

Architecture & State Management

Centralized State: Utilizes a globally registered Singleton SchedulingService to maintain in-memory state across the application lifecycle.

Memory Management: Implements strict IDisposable patterns on UI components listening to the OnStateChanged event, proactively preventing Blazor Server memory leaks and dangling references.

Data Ingestion: Leverages CsvHelper for robust, invariant-culture parsing of starter data into strictly typed C# models (Student, Course).

Domain Validation Engine
The assignment engine evaluates all mutations within a thread-safe lock (_syncRoot) block to prevent multi-user race conditions. It enforces the following constraints:

Duplicate Enrollment: Intercepts redundant assignment attempts.

Capacity Limits: Enforces a hard maximum of 20 enrolled seats per course.

Prerequisite Matching: Employs case-insensitive string tokenization, correctly parsing pipe-delimited student histories against comma-delimited course requirements.

Credit Load: Caps student schedules at a strict 18-credit maximum.

Local Execution

Navigate to the project root directory containing the .csproj file.

Execute dotnet run or dotnet watch in the terminal.

Access the application via the localhost port provided in the terminal output.
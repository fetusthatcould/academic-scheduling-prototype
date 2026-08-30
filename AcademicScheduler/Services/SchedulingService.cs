using System.Globalization;
using AcademicScheduler.Models;
using CsvHelper;

namespace AcademicScheduler.Services;

public class SchedulingService
{
    private readonly object _syncRoot = new();

    public List<Student> Students { get; } = new();
    public List<Course> Courses { get; } = new();

    public event Action? OnStateChanged;

    public SchedulingService()
    {
        var coursesPath = Path.Combine(AppContext.BaseDirectory, "data", "courses-starter.csv");
        var studentsPath = Path.Combine(AppContext.BaseDirectory, "data", "students-starter.csv");

        using (var reader = new StreamReader(coursesPath))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            foreach (var record in csv.GetRecords<dynamic>())
            {
                var row = (IDictionary<string, object>)record;
                Courses.Add(new Course
                {
                    Id = row["CourseNumber"].ToString()!,
                    Name = row["CourseName"].ToString()!,
                    Department = row["Department"].ToString()!,
                    Instructor = row["Instructor"].ToString()!,
                    Credits = int.Parse(row["Credits"].ToString()!, CultureInfo.InvariantCulture),
                    MaxCapacity = 20,
                    PrerequisitesRaw = row["Prerequisites"].ToString() ?? string.Empty,
                    EnrolledCount = 0
                });
            }
        }

        using (var reader = new StreamReader(studentsPath))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            foreach (var record in csv.GetRecords<dynamic>())
            {
                var row = (IDictionary<string, object>)record;
                Students.Add(new Student
                {
                    Id = row["Email"].ToString()!,
                    Name = $"{row["FirstName"]} {row["LastName"]}",
                    YearLevel = row["YearLevel"].ToString()!,
                    CompletedCoursesRaw = row["CompletedCourses"].ToString() ?? string.Empty
                });
            }
        }
    }

    public AssignmentResult AssignStudent(string studentId, string courseId)
    {
        lock (_syncRoot)
        {
            var student = Students.FirstOrDefault(s => s.Id == studentId);
            if (student is null)
                return new AssignmentResult { IsSuccess = false, ErrorMessage = "Student not found" };

            var course = Courses.FirstOrDefault(c => c.Id == courseId);
            if (course is null)
                return new AssignmentResult { IsSuccess = false, ErrorMessage = "Course not found" };

            // Duplicate Assignment Prevention
            if (student.AssignedCourses.Any(c => c.Id == course.Id))
                return new AssignmentResult { IsSuccess = false, ErrorMessage = "Student already enrolled" };

            // Capacity Validation
            if (course.AvailableSeats <= 0)
                return new AssignmentResult { IsSuccess = false, ErrorMessage = "Course is full" };

            // Prerequisite Tokenization and Comparison
            var completed = student.CompletedCoursesRaw.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var prereqs = course.PrerequisitesRaw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var prereq in prereqs)
            {
                if (!completed.Contains(prereq, StringComparer.OrdinalIgnoreCase))
                    return new AssignmentResult { IsSuccess = false, ErrorMessage = $"Prerequisite missing: {prereq}" };
            }

            // Maximum Credit Load Check
            if (student.CurrentCreditLoad + course.Credits > 18)
                return new AssignmentResult { IsSuccess = false, ErrorMessage = "Credit limit exceeded" };

            // State Mutation and Event Trigger
            student.AssignedCourses.Add(course);
            course.EnrolledCount++;

            OnStateChanged?.Invoke();
            return new AssignmentResult { IsSuccess = true };
        }
    }
    public AssignmentResult UnenrollStudent(string studentId, string courseId)
    {
        lock (_syncRoot)
        {
            var student = Students.FirstOrDefault(s => s.Id == studentId);
            if (student is null)
                return new AssignmentResult { IsSuccess = false, ErrorMessage = "Student not found" };

            var course = Courses.FirstOrDefault(c => c.Id == courseId);
            if (course is null)
                return new AssignmentResult { IsSuccess = false, ErrorMessage = "Course not found" };

            // Verify the student is actually enrolled in this course
            var enrolledCourse = student.AssignedCourses.FirstOrDefault(c => c.Id == course.Id);
            if (enrolledCourse is null)
                return new AssignmentResult { IsSuccess = false, ErrorMessage = "Student is not enrolled in this course" };

            // State Mutation
            student.AssignedCourses.Remove(enrolledCourse);

            // Defensive check to prevent negative capacity
            if (course.EnrolledCount > 0)
                course.EnrolledCount--;

            // Trigger UI refresh
            OnStateChanged?.Invoke();

            return new AssignmentResult { IsSuccess = true };
        }
    }
}

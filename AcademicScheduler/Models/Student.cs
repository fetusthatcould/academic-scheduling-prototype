namespace AcademicScheduler.Models;

public class Student
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string YearLevel { get; set; } = string.Empty;
    public string CompletedCoursesRaw { get; set; } = string.Empty;
    public List<Course> AssignedCourses { get; set; } = new();
    public int CurrentCreditLoad => AssignedCourses.Sum(c => c.Credits);
}

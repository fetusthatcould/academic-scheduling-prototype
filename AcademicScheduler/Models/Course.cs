namespace AcademicScheduler.Models;

public class Course
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Instructor { get; set; } = string.Empty;
    public int Credits { get; set; }
    public int MaxCapacity { get; set; }
    public string PrerequisitesRaw { get; set; } = string.Empty;
    public int EnrolledCount { get; set; }
    public int AvailableSeats => MaxCapacity - EnrolledCount;
}



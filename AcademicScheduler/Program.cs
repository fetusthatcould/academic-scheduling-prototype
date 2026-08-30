using AcademicScheduler.Components;
using AcademicScheduler.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<SchedulingService>();

var app = builder.Build();

// --- TEMPORARY BACKEND TESTING ---
using (var scope = app.Services.CreateScope())
{
    var scheduler = scope.ServiceProvider.GetRequiredService<SchedulingService>();

    // Grab a test student and course from your seeded data
    var testStudent = scheduler.Students.First();
    var testCourse = scheduler.Courses.First();

    Console.WriteLine($"Testing Assignment for {testStudent.Name} into {testCourse.Name}");

    // Test 1: Initial Success
    var result1 = scheduler.AssignStudent(testStudent.Id, testCourse.Id);
    Console.WriteLine($"Test 1 (Valid): {result1.IsSuccess} - {result1.ErrorMessage}");

    // Test 2: Duplicate Prevention
    var result2 = scheduler.AssignStudent(testStudent.Id, testCourse.Id);
    Console.WriteLine($"Test 2 (Duplicate): {!result2.IsSuccess} - {result2.ErrorMessage}");
}

// --- TEMPORARY BACKEND TESTING ---
using (var scope = app.Services.CreateScope())
{
    var scheduler = scope.ServiceProvider.GetRequiredService<SchedulingService>();
    
    var testStudent = scheduler.Students.FirstOrDefault();
    
    // Select a course with NO prerequisites to guarantee initial success
    var testCourse = scheduler.Courses.FirstOrDefault(c => string.IsNullOrWhiteSpace(c.PrerequisitesRaw));

    if (testStudent != null && testCourse != null)
    {
        Console.WriteLine($"Testing Assignment for {testStudent.Name} into {testCourse.Name}");
        
        // Test 1: Should pass successfully
        var result1 = scheduler.AssignStudent(testStudent.Id, testCourse.Id);
        Console.WriteLine($"Test 1 (Valid): {result1.IsSuccess} - {result1.ErrorMessage}");

        // Test 2: Should now fail with "Student already enrolled"
        var result2 = scheduler.AssignStudent(testStudent.Id, testCourse.Id);
        Console.WriteLine($"Test 2 (Duplicate): {result2.IsSuccess} - {result2.ErrorMessage}");
    }
}
// ---------------------------------
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

var provider = PostgreSQL;
var connectionString = ConnectionStrings.Get(provider);

var options = new DbContextOptionsBuilder<UniversityContext>()
    .Use(provider, connectionString)
    .Options;

using var context = new UniversityContext(options);

//await Setup(context);
//await Query(context);
//await NoTracking(context);
//await SoftDelete(context);
//await View(context);
//await RawSql(context);
//await RepositoryPattern(context);
//await DbFunctionsQueries(context);
//await SplitQueries(context);
//await BulkOperations(context);

// Setup del database.
async Task Setup(UniversityContext context)
{
    await context.Database.EnsureDeletedAsync();
    await context.Database.EnsureCreatedAsync();

    var computerProgramming = new Course { Name = "Computer Programming" };
    var artificialIntelligence = new Course { Name = "Artificial Intelligence" };

    context.Students.AddRange(new[]
    {
        new Student {
            Name = "John",
            Address = new StudentAddress { Address = "John address" },
            Notes = new[]
            {
                new Note { Text = "Computer programming notes..." },
                new Note { Text = "Artificial Intelligence notes..." },
            },
            Courses = new[]
            {
                computerProgramming,
                artificialIntelligence,
            }
        },
        new Student
        {
            Name = "Stuart",
            Address = new StudentAddress { Address = "Stuart address" },
            Courses = new[]
            {
                computerProgramming,
            }
        },
    });

    await context.SaveChangesAsync();
}

// Esempi di query.
async Task Query(UniversityContext context)
{
    var student = await context.FindAsync<Student>(1);
    if (student == null) return;

    // Caricamento entità correlata (lazy).
    await context.Entry(student)
        .Reference(p => p.Address)
        .LoadAsync();

    PrintStudent(student);
    WriteLine();

    // Eager loading.
    var students = await context.Students
        .Include(p => p.Courses)
        .Include(p => p.Address)
        .ToListAsync();

    foreach (var s in students)
    {
        PrintStudent(s);

        WriteLine("Courses:");
        foreach (var course in s.Courses) WriteLine($"\tCourse: {course.Name}");

        WriteLine();
    }
    WriteLine();

    // Query: numero di studenti iscritti ad ogni corso.
    var courses = context.Courses
        .Select(p => new
        {
            p.Name,
            Students = p.Students.Count,
        });

    foreach (var c in courses) WriteLine($"Course: {c.Name}, #Students: {c.Students}");

    static void PrintStudent(Student student) => WriteLine($"Name: {student?.Name}, Address: {student?.Address?.Address}");
}

// Come disabilitare il tracking delle entità.
async Task NoTracking(UniversityContext context)
{
    // No tracking per l'istanza context.
    //context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTrackingWithIdentityResolution;

    var course = await context.Courses.FirstAsync(p => p.Name == "Computer Programming");
    course.Name = "Computer Programming I";
    await context.SaveChangesAsync();

    var students = await context.Students
        .Include(p => p.Courses)
        .ToListAsync();

    foreach (var s in students)
    {
        foreach (var c in s.Courses)
        {
            WriteLine($"Id: {c.Id}, Name: {c.Name}, HashCode: {c.GetHashCode()}");
        }
    }
}

// Implementazione della "soft delete".
async Task SoftDelete(UniversityContext context)
{
    var student = await context.Students.FirstAsync();
    context.Remove(student);

    await context.SaveChangesAsync();
}

// Come mappare un'entità sui risultati di una query.
async Task View(UniversityContext context)
{
    /*
        SQL Server:

        CREATE VIEW ViewStudentsCourses AS
        SELECT s.Name AS Student, c.Name AS Course
        FROM 
	        Students s 
	        LEFT JOIN CoursesStudents cs ON s.Id = cs.StudentsId
	        LEFT JOIN Courses c ON cs.CoursesId = c.Id
     */

    var results = await context.Set<StudentCourse>().ToListAsync();
    foreach (var sc in results) WriteLine(sc);
}

// Esecuzione di SQL ad-hoc.
async Task RawSql(UniversityContext context)
{
    var id = 1;
    var query = context
        .Students
        .FromSqlInterpolated($"SELECT * FROM Students WHERE Id = {id}");

    var sql = query.ToQueryString();
    WriteLine(sql);

    var results = await query.ToListAsync();
    foreach (var s in results) WriteLine($"Id: {s.Id}, Name: {s.Name}");
}

async Task RepositoryPattern(UniversityContext context)
{
    await using var uow = new UniversityUnitOfWork(context);

    var results = await uow.Students.GetAll(s => s.Id == 1);
    foreach (var r in results) WriteLine($"Id: {r.Id}, Name: {r.Name}");
}

// Utilizzo di funzioni mappate sulle equivalenti SQL.
async Task DbFunctionsQueries(UniversityContext context)
{
    var query =
        from s in context.Students
        where EF.Functions.Like(s.Name, "J%")
        select new
        {
            s.Id,
            s.Name,
            CreatedMinutesAgo = EF.Functions.DateDiffMinute(s.CreatedDate, DateTime.UtcNow),
        };

    WriteLine(query.ToQueryString());
    foreach (var s in await query.ToListAsync()) WriteLine(s);
}

// Split delle query.
async Task SplitQueries(UniversityContext context)
{
    // Cartesian explosion.
    await Dump(context.Students.Include(s => s.Courses));

    WriteLine();

    // Split queries.
    await Dump(context.Students.AsSplitQuery().Include(s => s.Courses));

    static async Task Dump(IQueryable<Student> query)
    {
        var results = await query.ToListAsync();

        foreach (var s in results)
        {
            WriteLine($"Id: {s.Id}, Name: {s.Name}");
            foreach (var c in s.Courses) WriteLine($"\t{c.Name}");
        }
    }
}

// Update / Delete bulk.
async Task BulkOperations(UniversityContext context)
{
    await context.Courses
        .Where(p => p.Name.Contains("Intelligence"))
        .ExecuteUpdateAsync(
            c => c.SetProperty(
                p => p.Name,
                p => p.Name.Replace("Artificial Intelligence", "AI")
            )
        );
}
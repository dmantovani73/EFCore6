class Student : ISoftDelete, ITimeTrack
{
    public int Id { get; set; }

    //[Required]
    //[StringLength(100)]
    public string Name { get; set; } = "";

    public int Age { get; set; }

    public StudentAddress? Address { get; set; }

    public ICollection<Note> Notes { get; set; } = new List<Note>();

    public ICollection<Course> Courses { get; set; } = new List<Course>();

    public bool IsDeleted { get; set; }

    public DateTime CreatedDate { get; set; }
    
    public DateTime UpdatedDate { get; set; }
}

class StudentAddress
{
    public int Id { get; set; }

    public string Address { get; set; } = "";

    public int StudentId { get; set; }

    public Student Student { get; set; } = new Student();
}

class Note
{
    public int Id { get; set; }

    public string? Text { get; set; }

    public Student Student { get; set; } = new Student();
}

class Course
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public ICollection<Student> Students { get; set; } = new List<Student>();
}

record StudentCourse (string Student, string? Course)
{ }

interface ISoftDelete
{
    bool IsDeleted { get; set; }
}

interface ITimeTrack
{
    DateTime CreatedDate { get; set; }
    
    DateTime UpdatedDate { get; set; }
}
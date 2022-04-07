using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        // Vincoli e proprietà.
        builder
            .HasKey(p => p.Id); // Implicito di default.

        builder
            .Property(p => p.Name)
            .HasMaxLength(100);

        // Relazione 1:1 dichiarata in modo esplicito.
        builder
            .HasOne(p => p.Address)
            .WithOne(p => p.Student)
            .HasForeignKey<StudentAddress>(p => p.StudentId);

        // Relazione 1:N dichiarata in modo esplicito.
        builder
            .HasMany(p => p.Notes)
            .WithOne(p => p.Student);

        // Relazione N:N dichiarata in modo esplicito.
        builder
            .HasMany(p => p.Courses)
            .WithMany(p => p.Students)
            .UsingEntity(join => join.ToTable("CoursesStudents"));

        //builder
        //    .HasQueryFilter(p => !p.IsDeleted);
    }
}

internal class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder
            .ToTable("Notes");
    }
}

internal class StudentAddressConfiguration : IEntityTypeConfiguration<StudentAddress>
{
    public void Configure(EntityTypeBuilder<StudentAddress> builder)
    {
        builder
            .ToTable("StudentAddresses");
    }
}

internal class StudentCourseConfiguration : IEntityTypeConfiguration<StudentCourse>
{
    public void Configure(EntityTypeBuilder<StudentCourse> builder)
    {
        builder
            .ToView("ViewStudentsCourses")
            .HasNoKey();
    }
}

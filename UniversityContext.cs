using Microsoft.EntityFrameworkCore.ChangeTracking;

class UniversityContext : DbContext
{
    public UniversityContext(DbContextOptions<UniversityContext> options)
        : base(options)
    { }

    public DbSet<Student> Students => Set<Student>();

    public DbSet<Course> Courses => Set<Course>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Applico la configurazione per ciascuna entità.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StudentConfiguration).Assembly);

        // Applico il filtro SoftDelete per le entità che implementano l'interfaccia ISoftDelete.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            //other automated configurations left out
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                entityType.AddSoftDeleteQueryFilter();
            }
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Abilito la scrittura del log.
        //optionsBuilder.LogTo(WriteLine);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        HandleEntities();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        HandleEntities();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void HandleEntities()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            HandleSoftDelete(entry);
            HandleTimeTrack(entry);
        }
    }

    private void HandleSoftDelete(EntityEntry? entry)
    {
        if (!(entry?.Entity is ISoftDelete entity)) return;

        switch (entry.State)
        {
            case EntityState.Added:
                entity.IsDeleted = false;
                break;

            case EntityState.Deleted:
                entry.State = EntityState.Modified;
                entity.IsDeleted = true;
                break;
        }
    }

    private void HandleTimeTrack(EntityEntry? entry)
    {
        if (!(entry?.Entity is ITimeTrack entity)) return;

        var now = DateTime.UtcNow;
        entity.UpdatedDate = now;
        if (entry.State == EntityState.Added) entity.CreatedDate = now;
    }
}
using SpotAnalysis.Data.Models;

namespace SpotAnalysis.Data;

public class AnalysisContext : DbContext {

    private static readonly DateTime UncompletedAttemptSentinelUtc = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

    #region DBSets

    #region Users, Roles, Groups
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Group> Groups { get; set; }

    #endregion Users, Roles, Groups


    #region Educts, Products, Additives
    public virtual DbSet<Chemical> Chemicals { get; set; }
    public virtual DbSet<Method> Methods { get; set; }
    public virtual DbSet<MethodOutput> MethodOutputs { get; set; }
    public virtual DbSet<Reaction> Reactions { get; set; }
    public virtual DbSet<Observation> Observations { get; set; }

    #endregion Educts, Products, Additives


    #region Quizzes
    public virtual DbSet<Quiz> Quizzes { get; set; }
    public virtual DbSet<QuizAttempt> QuizAttempts { get; set; }
    public virtual DbSet<Question> Questions { get; set; }
    public virtual DbSet<QuizQuestion> QuizQuestions { get; set; }
    public virtual DbSet<STLQuestion> STLQuestions { get; set; }
    public virtual DbSet<STQuestion> STQuestions { get; set; }
    public virtual DbSet<STAvailableChemical> STAvailableChemicals { get; set; }
    public virtual DbSet<STAvailableMethod> STAvailableMethods { get; set; }
    public virtual DbSet<STLAvailableReaction> STLAvailableReactions { get; set; }
    public virtual DbSet<STLResult> STLResults { get; set; }
    public virtual DbSet<STResult> STResults { get; set; }
    public virtual DbSet<STChemicalResult> STChemicalResults { get; set; }
    #endregion Quizzes


    #region SpotTest

    #endregion SpotTest


    #region SpotTestLight

    #endregion SpotTestLight


    #endregion DBSets

    // Connection String wird NICHT in diesem Projekt definiert.
    // Zur Laufzeit: DI-Konfiguration in SpotAnalysis.Web/Program.cs (AddDbContext + appsettings.json)
    // Für Migrations: --startup-project SpotAnalysis.Web (siehe EF-MIGRATIONS.md)
    public AnalysisContext(DbContextOptions<AnalysisContext> options)
        : base(options) {
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        NormalizeDateTimesToUtc();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        NormalizeDateTimesToUtc();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void NormalizeDateTimesToUtc()
    {
        foreach (var entry in ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified))
        {
            foreach (var property in entry.Properties)
            {
                var clrType = property.Metadata.ClrType;

                if (clrType == typeof(DateTime))
                {
                    if (property.CurrentValue is DateTime value)
                    {
                        property.CurrentValue = NormalizeDateTime(value);
                    }
                }
                else if (clrType == typeof(DateTime?))
                {
                    if (property.CurrentValue is DateTime nullableValue)
                    {
                        property.CurrentValue = NormalizeDateTime(nullableValue);
                    }
                }
            }
        }
    }

    private static DateTime NormalizeDateTime(DateTime value)
    {
        if (value == DateTime.MinValue)
        {
            return UncompletedAttemptSentinelUtc;
        }

        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            _ => value
        };
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Reaction>()
        .ToTable(t => t.HasCheckConstraint("CK_Reaction_ChemicalOrder", "\"Chemical1ID\" <= \"Chemical2ID\""));

        modelBuilder.Entity<User>()
            .HasMany(u => u.Groups)
            .WithMany(r => r.Users)
            .UsingEntity(
                // deleting a group is only possible, when it is not referenced by any user
                groupForeignKey => groupForeignKey
                    .HasOne(typeof(Group)).WithMany().HasForeignKey("GroupID").OnDelete(DeleteBehavior.Restrict),
                // deleting the user also deletes all references to the user in the join table, but not the groups themselves
                userForeignKey => userForeignKey
                    .HasOne(typeof(User)).WithMany().HasForeignKey("UserID").OnDelete(DeleteBehavior.Cascade)
            );

        modelBuilder.Entity<Quiz>()
        .HasMany(q => q.Groups)
        .WithMany(g => g.Quizzes)
        .UsingEntity(
            // deleting a quiz is only possible, when it is not referenced by any group
            q => q.HasOne(typeof(Group)).WithMany().HasForeignKey("GroupID").OnDelete(DeleteBehavior.Restrict),
            // deleting a group is only possible, when it is not referenced by any quiz
            g => g.HasOne(typeof(Quiz)).WithMany().HasForeignKey("QuizID").OnDelete(DeleteBehavior.Restrict)
         );
    }
}
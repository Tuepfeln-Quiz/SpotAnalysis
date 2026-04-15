using SpotAnalysis.Data.Models;

namespace SpotAnalysis.Data;

public class AnalysisContext : DbContext {

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

    #endregion Quizzes


    #region SpotTest
    public virtual DbSet<STAvailableChemical> STAvailableChemicals { get; set; }
    public virtual DbSet<STAvailableMethod> STAvailableMethods { get; set; }
    public virtual DbSet<STResult> STResults { get; set; }
    public virtual DbSet<STChemicalResult> STChemicalResults { get; set; }

    #endregion SpotTest


    #region SpotTestLight
    public virtual DbSet<STLAvailableReaction> STLAvailableReactions { get; set; }
    public virtual DbSet<STLResult> STLResults { get; set; }

    #endregion SpotTestLight


    #endregion DBSets

    // Connection String wird NICHT in diesem Projekt definiert.
    // Zur Laufzeit: DI-Konfiguration in SpotAnalysis.Web/Program.cs (AddDbContext + appsettings.json)
    // Für Migrations: --startup-project SpotAnalysis.Web (siehe EF-MIGRATIONS.md)
    public AnalysisContext(DbContextOptions<AnalysisContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<User>()
        .HasMany(u => u.Groups)
        .WithMany(r => r.Users)
        .UsingEntity(
            u => u.HasOne(typeof(Group)).WithMany().HasForeignKey("GroupID").OnDelete(DeleteBehavior.Restrict),
            g => g.HasOne(typeof(User)).WithMany().HasForeignKey("UserID").OnDelete(DeleteBehavior.Restrict)
            );

        modelBuilder.Entity<Quiz>()
        .HasMany(q => q.Groups)
        .WithMany(g => g.Quizzes)
        .UsingEntity(
            q => q.HasOne(typeof(Group)).WithMany().HasForeignKey("GroupID").OnDelete(DeleteBehavior.Restrict),
            g => g.HasOne(typeof(Quiz)).WithMany().HasForeignKey("QuizID").OnDelete(DeleteBehavior.Restrict)
            );
    }
}
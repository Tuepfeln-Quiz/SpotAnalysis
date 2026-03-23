namespace SpotAnalysis.Data;

public class AnalysisContext : DbContext {

    #region DBSets

    #region Users, Roles, Groups
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserRole> UserRoles { get; set; }
    public virtual DbSet<UserGroup> UserGroups { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<Group> Groups { get; set; }

    #endregion Users, Roles, Groups


    #region Educts, Products, Additives
    public virtual DbSet<Chemical> Chemicals { get; set; }
    public virtual DbSet<ChemicalType> ChemicalTypes { get; set; }
    public virtual DbSet<Method> Methods { get; set; }
    public virtual DbSet<MethodOutput> MethodOutputs { get; set; }
    public virtual DbSet<Reaction> Reactions { get; set; }
    public virtual DbSet<Observation> Observations { get; set; }

    #endregion Educts, Products, Additives


    #region Quizzes
    public virtual DbSet<Quiz> Quizzes { get; set; }
    public virtual DbSet<GroupQuiz> GroupQuizzes { get; set; }
    public virtual DbSet<QuizType> QuizTypes { get; set; }
    public virtual DbSet<QuizStatus> QuizStatus { get; set; }
    public virtual DbSet<QuizAttempt> QuizAttempts { get; set; }

    #endregion Quizzes


    #region SpotTest
    public virtual DbSet<STQuestion> STQuestion { get; set; }
    public virtual DbSet<STAvailableChemical> STAvailableChemicals { get; set; }
    public virtual DbSet<STAvailableMethod> STAvailableMethods { get; set; }
    public virtual DbSet<STResult> STResults { get; set; }
    public virtual DbSet<STChemicalResult> STChemicalResults { get; set; }
    public virtual DbSet<STLog> STLogs { get; set; }

    #endregion SpotTest


    #region SpotTestLight
    public virtual DbSet<STLQuestion> STLQuestions { get; set; }
    public virtual DbSet<STLAvailableReaction> STLAvailableReactions { get; set; }
    public virtual DbSet<STLResult> STLResults { get; set; }

    #endregion SpotTestLight


    #endregion DBSets

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        if (!optionsBuilder.IsConfigured) {
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SpotAnalysis;Connect Timeout= 30;Integrated Security=True;Encrypt=True;Trust Server Certificate=False;");
        }
    }
}
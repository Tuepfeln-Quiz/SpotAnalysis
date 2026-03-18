namespace SpotAnalysis_Api.Services;
using SpotAnalysis.Data;

public class LoginService : ILoginService
{
    private readonly AnalysisContext dbc;

    public LoginService(AnalysisContext dbc)
    {
        this.dbc = dbc;
    }
    public void Login(string userName, string password)
    {
          Console.WriteLine("Authenticate method called with username: " + userName + " and password: " + password);
    }

    public void ChangePassword(string userName, string oldPassword, string newPassword)
    {
        throw new NotImplementedException();
    }
}
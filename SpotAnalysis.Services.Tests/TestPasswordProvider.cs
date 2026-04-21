using SpotAnalysis.Services.Services;

namespace SpotAnalysis.Services.Tests;

public class TestPasswordProvider
{
    [Test, Order(1)]
    public void TestPasswordGeneration()
    {
        var provider = new PasswordProvider.Password("asdfghij", Guid.Parse("fe64042c-161b-4aa6-91d9-0a93211a7f41"));
        Console.WriteLine(provider.ParamString());
    }


    [Test, Order(2)]
    public void TestPasswordComparison()
    {
        const string refString =
            "$argon2id$v=19$m=4096,t=4,p=4$LARk/hsWpkqR2QqTIRp/QQ==$J18GjOSWAE45qNXk6DaOiThagAnPeBAu1HmA/3tBWtQ=\n";

        var reference = PasswordProvider.Password.FromParamString(refString);

        var newPassword = new PasswordProvider.Password("asdfghij", Guid.Parse("fe64042c-161b-4aa6-91d9-0a93211a7f41"));

        Assert.That(reference.Compare(newPassword), Is.True);
    }
}
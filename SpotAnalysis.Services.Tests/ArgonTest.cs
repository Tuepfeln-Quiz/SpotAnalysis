using SpotAnalysis.Services.Services;

namespace SpotAnalysis.Services.Tests;

public class ArgonTest
{
    [Test, Order(1)]
    public void TestArgonPasswordGeneration()
    {
        var provider = new ArgonProvider.ArgonOutput("asdfghij", Guid.Parse("fe64042c-161b-4aa6-91d9-0a93211a7f41"));

        Console.WriteLine(provider.ParamString());
    }

    [Test, Order(2)]
    public void TestArgonPasswordValidation()
    {
        const string refString =
            "$argon2id$v=19$m=4096,t=4,p=4$LARk/hsWpkqR2QqTIRp/QQ==$J18GjOSWAE45qNXk6DaOiThagAnPeBAu1HmA/3tBWtQ=\n";
        
        var reference = ArgonProvider.ArgonOutput.FromParamString(refString);
        
        var newPassword = new ArgonProvider.ArgonOutput("asdfghij", Guid.Parse("fe64042c-161b-4aa6-91d9-0a93211a7f41"));
        
        Assert.That(reference.Compare(newPassword), Is.True);
    }
}
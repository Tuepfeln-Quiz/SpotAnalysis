using SpotAnalysis.Services.Services;

namespace SpotAnalysis.Services.Tests;

public class ArgonTest
{
    [Test, Order(1)]
    public void TestArgonPasswordGeneration()
    {
        var provider = new ArgonProvider.ArgonOutput("asdfghij", "asdfghij");

        Console.WriteLine(provider.ParamString());
    }

    [Test, Order(2)]
    public void TestArgonPasswordValidation()
    {
        const string refString =
            "$argon2id$v=19$m=4096,t=4,p=4$YXNkZmdoaWo=$iLeJ+wSTpqj4JeHpb3NcdAg1L+Lt9gu8rTblqlV2pFA=";
        
        var reference = ArgonProvider.ArgonOutput.FromParamString(refString);
        
        var newPassword = new ArgonProvider.ArgonOutput("asdfghij", "asdfghij");
        
        Assert.That(reference.Compare(newPassword), Is.True);
    }
}
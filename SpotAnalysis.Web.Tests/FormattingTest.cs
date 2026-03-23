using SpotAnalysis.Web.Services;

namespace SpotAnalysis.Web.Tests;

public class Tests
{
    private readonly Dictionary<string, string> _chemicalStringTestCases = new Dictionary<string, string>()
    {
        {"FeCl3", "FeCl<sub>3</sub>"},
        {"Pb(NO3)2", "Pb(NO<sub>3</sub>)<sub>2</sub>"},
        {"KI", "KI"},
        {"NaCO3", "NaCO<sub>3</sub>"},
        {"AgNO3", "AgNO<sub>3</sub>"},
        {"SrCl2", "SrCl<sub>2</sub>"},
        {"Ba(OH)2", "Ba(OH)<sub>2</sub>"},
        {"Fe2(CO3)3", "Fe<sub>2</sub>(CO<sub>3</sub>)<sub>3</sub>"},
        {"Ag2CO3", "Ag<sub>2</sub>CO<sub>3</sub>"}
    };
    
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestChemicalStringFormatting()
    {
        foreach (var cases in _chemicalStringTestCases)
        {
            var result = ChemicalStringFormatter.Format(cases.Key);
            Assert.That(result, Is.EqualTo(cases.Value));
        }
    }
}
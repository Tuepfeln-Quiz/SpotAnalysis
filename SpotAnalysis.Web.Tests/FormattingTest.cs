using SpotAnalysis.Web.Services;

namespace SpotAnalysis.Web.Tests;

public class Tests
{
    [Test]
    public void Format_PureFormulas()
    {
        Assert.That(ChemicalStringFormatter.Format("FeCl3"), Is.EqualTo("FeCl₃"));
        Assert.That(ChemicalStringFormatter.Format("Pb(NO3)2"), Is.EqualTo("Pb(NO₃)₂"));
        Assert.That(ChemicalStringFormatter.Format("KI"), Is.EqualTo("KI"));
        Assert.That(ChemicalStringFormatter.Format("NaCO3"), Is.EqualTo("NaCO₃"));
        Assert.That(ChemicalStringFormatter.Format("AgNO3"), Is.EqualTo("AgNO₃"));
        Assert.That(ChemicalStringFormatter.Format("SrCl2"), Is.EqualTo("SrCl₂"));
        Assert.That(ChemicalStringFormatter.Format("Ba(OH)2"), Is.EqualTo("Ba(OH)₂"));
        Assert.That(ChemicalStringFormatter.Format("Fe2(CO3)3"), Is.EqualTo("Fe₂(CO₃)₃"));
        Assert.That(ChemicalStringFormatter.Format("Ag2CO3"), Is.EqualTo("Ag₂CO₃"));
    }

    [Test]
    public void FormatText_FormulasInText()
    {
        // Formulas get formatted
        Assert.That(ChemicalStringFormatter.FormatText("gelber Niederschlag Pb(NO3)2"),
            Is.EqualTo("gelber Niederschlag Pb(NO₃)₂"));

        Assert.That(ChemicalStringFormatter.FormatText("Light Q1 - gelber Niederschlag Pb(NO3)2"),
            Is.EqualTo("Light Q1 - gelber Niederschlag Pb(NO₃)₂"));

        Assert.That(ChemicalStringFormatter.FormatText("Reaktion von AgNO3 mit FeCl3"),
            Is.EqualTo("Reaktion von AgNO₃ mit FeCl₃"));
    }

    [Test]
    public void FormatText_NonFormulasStayUnchanged()
    {
        // Regular abbreviations with digits stay unchanged
        Assert.That(ChemicalStringFormatter.FormatText("Q1"), Is.EqualTo("Q1"));
        Assert.That(ChemicalStringFormatter.FormatText("Light Q1 - test"), Is.EqualTo("Light Q1 - test"));
        Assert.That(ChemicalStringFormatter.FormatText("4 Reaktionen"), Is.EqualTo("4 Reaktionen"));
        Assert.That(ChemicalStringFormatter.FormatText("in 1 Quizzes"), Is.EqualTo("in 1 Quizzes"));

        // Plain text stays unchanged
        Assert.That(ChemicalStringFormatter.FormatText("Niederschlag"), Is.EqualTo("Niederschlag"));
        Assert.That(ChemicalStringFormatter.FormatText("Blei(II)nitrat"), Is.EqualTo("Blei(II)nitrat"));
    }

    [Test]
    public void FormatText_FormulasWithoutDigits_StayUnchanged()
    {
        Assert.That(ChemicalStringFormatter.FormatText("KI"), Is.EqualTo("KI"));
        Assert.That(ChemicalStringFormatter.FormatText("AgCl"), Is.EqualTo("AgCl"));
    }
}

using System.Text;

namespace SpotAnalysis.Web.Services;

public static class ChemicalStringFormatter
{
    private const string SubOpen = "<sub>";
    private const string SubClose = "</sub>";

    public static string Format(string input)
    {
        var sb = new StringBuilder();

        foreach (var c in input)
        {
            if (char.IsDigit(c))
            {
                sb.Append(SubOpen);
                sb.Append(c);
                sb.Append(SubClose);
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }
}
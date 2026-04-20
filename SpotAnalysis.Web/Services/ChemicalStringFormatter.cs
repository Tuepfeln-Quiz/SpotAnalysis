using System.Text;
using System.Text.RegularExpressions;

namespace SpotAnalysis.Web.Services;

public static partial class ChemicalStringFormatter
{
    private static readonly char[] Subscripts = ['₀', '₁', '₂', '₃', '₄', '₅', '₆', '₇', '₈', '₉'];

    // Matches sequences that look like chemical formulas:
    // Start with an element symbol (uppercase + optional lowercase),
    // followed by one or more of: element symbols, digits, or parenthesized groups.
    [GeneratedRegex(@"[A-Z][a-z]?(?:[A-Z][a-z]?|\d+|\([A-Za-z\d]+\)\d*)+")]
    private static partial Regex FormulaPattern();

    /// <summary>
    /// Converts ALL digits to Unicode subscript characters.
    /// Use only on pure chemical formula strings (e.g. "Pb(NO3)2" → "Pb(NO₃)₂").
    /// </summary>
    public static string Format(string input)
    {
        var sb = new StringBuilder(input.Length);

        foreach (var c in input)
        {
            if (char.IsDigit(c))
            {
                sb.Append(Subscripts[c - '0']);
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Finds chemical formulas embedded in free text and formats only those.
    /// "Q1" stays "Q1", but "Pb(NO3)2" becomes "Pb(NO₃)₂".
    /// A formula must have 2+ uppercase letters or parentheses, plus at least one digit.
    /// </summary>
    public static string FormatText(string input)
    {
        return FormulaPattern().Replace(input, match =>
        {
            var val = match.Value;

            // Must contain at least one digit (otherwise nothing to subscript)
            if (!val.AsSpan().ContainsAny("0123456789"))
                return val;

            // Must have 2+ uppercase letters or parentheses to distinguish
            // chemical formulas (AgNO3, Pb(OH)2) from regular abbreviations (Q1, A4)
            if (val.Count(char.IsUpper) < 2 && !val.Contains('('))
                return val;

            return Format(val);
        });
    }
}

namespace PandaClaus.Web;

class CustomComparer : IComparer<string>
{
    private const string Unknown = "X";

    public int Compare(string x, string y)
    {
        if (string.IsNullOrWhiteSpace(x) && string.IsNullOrWhiteSpace(y)) return 0;
        if (string.IsNullOrWhiteSpace(x)) return 1;
        if (string.IsNullOrWhiteSpace(y)) return -1;

        if (x == Unknown &&  y == Unknown) return 0;
        if (x == Unknown) return 1;
        if (y == Unknown) return -1;

        // Extract numeric and non-numeric parts
        string[] xParts = System.Text.RegularExpressions.Regex.Split(x, @"([0-9]+)");
        string[] yParts = System.Text.RegularExpressions.Regex.Split(y, @"([0-9]+)");

        // Compare numeric parts as integers
        int xNumeric = int.Parse(xParts[1]);
        int yNumeric = int.Parse(yParts[1]);
        int numericComparison = xNumeric.CompareTo(yNumeric);

        if (numericComparison != 0)
        {
            return numericComparison;
        }

        // If numeric parts are equal, compare non-numeric parts lexicographically
        return string.Compare(xParts[2], yParts[2]);
    }
}
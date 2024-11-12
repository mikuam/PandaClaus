namespace PandaClaus.Web.Core;

public static class StringExtensions
{
    public static string FormatAsAge(this int age)
    {
        return age switch
        {
            <= 1 => "1 rok",
            <= 4 => $"{age} lata",
            _ => $"{age} lat"
        };
    }
}

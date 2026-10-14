namespace PandaClaus.Web;

public static class SheetEligibilityColumns
{
    public const string RelationHeader = "DeceasedRelation";
    public const string TimeHeader = "LossTime";

    public static bool AreReady(int columnCount, string? relationHeader, string? timeHeader) =>
        columnCount >= 30
        && relationHeader == RelationHeader
        && timeHeader == TimeHeader;
}

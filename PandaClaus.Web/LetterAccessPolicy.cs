namespace PandaClaus.Web;

public static class LetterAccessPolicy
{
    public static bool CanReadPublicly(bool isVisible, bool isDeleted) => isVisible && !isDeleted;
    public static bool CanReserve(bool isVisible, bool isDeleted, bool isAssigned) =>
        CanReadPublicly(isVisible, isDeleted) && !isAssigned;
}

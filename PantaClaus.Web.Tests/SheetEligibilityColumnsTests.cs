using PandaClaus.Web;

namespace PantaClaus.Web.Tests;

[TestClass]
public class SheetEligibilityColumnsTests
{
    [TestMethod]
    public void OldSheetAndUnallocatedColumnsAreNotReadyForNewWrites()
    {
        Assert.IsFalse(SheetEligibilityColumns.AreReady(28, null, null));
        Assert.IsFalse(SheetEligibilityColumns.AreReady(30, null, null));
        Assert.IsFalse(SheetEligibilityColumns.AreReady(30, "Obca kolumna", "Inna kolumna"));
    }

    [TestMethod]
    public void NewHeadersUseModelPropertyNamesInPascalCase()
    {
        Assert.AreEqual("DeceasedRelation", SheetEligibilityColumns.RelationHeader);
        Assert.AreEqual("LossTime", SheetEligibilityColumns.TimeHeader);
        Assert.IsTrue(SheetEligibilityColumns.AreReady(30, "DeceasedRelation", "LossTime"));
        Assert.IsFalse(SheetEligibilityColumns.AreReady(30,
            "Kogo straciło dziecko", "Kiedy zmarła ta osoba"));
    }
}

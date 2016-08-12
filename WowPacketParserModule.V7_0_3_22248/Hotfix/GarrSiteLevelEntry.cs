using WowPacketParser.Enums;
using WowPacketParser.Hotfix;

namespace WowPacketParserModule.V7_0_3_22248.Hotfix
{
    [HotfixStructure(DB2Hash.GarrSiteLevel, HasIndexInData = false)]
    public class GarrSiteLevelEntry
    {
        [HotfixArray(2)]
        public float[] TownHall { get; set; }
        public ushort MapID { get; set; }
        public ushort SiteID { get; set; }
        public ushort UpgradeResourceCost { get; set; }
        public ushort UpgradeMoneyCost { get; set; }
        public byte Level { get; set; }
        public byte UITextureKitID { get; set; }
        public byte MovieID { get; set; }
        public byte Level2 { get; set; }
    }
}
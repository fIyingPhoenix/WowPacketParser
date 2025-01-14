using System;
using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using WowPacketParser.Store;
using WowPacketParser.Store.Objects;
using CoreParsers = WowPacketParser.Parsing.Parsers;

namespace WowPacketParserModule.V8_0_1_27101.Parsers
{
    public static class NpcHandler
    {
        public static uint LastGossipPOIEntry;

        [Parser(Opcode.SMSG_GOSSIP_POI)]
        public static void HandleGossipPoi(Packet packet)
        {
            var protoPoi = packet.Holder.GossipPoi = new();
            PointsOfInterest gossipPOI = new PointsOfInterest();

            gossipPOI.ID = protoPoi.Id = packet.ReadInt32("ID");

            Vector2 pos = packet.ReadVector2("Coordinates");
            gossipPOI.PositionX = pos.X;
            gossipPOI.PositionY = pos.Y;
            protoPoi.Coordinates = pos;

            gossipPOI.Icon = packet.ReadInt32E<GossipPOIIcon>("Icon");
            gossipPOI.Importance = protoPoi.Importance = (uint)packet.ReadInt32("Importance");
            protoPoi.Icon = (uint)gossipPOI.Icon;

            packet.ResetBitReader();
            gossipPOI.Flags = protoPoi.Flags = packet.ReadBits("Flags", 14);
            uint bit84 = packet.ReadBits(6);
            gossipPOI.Name = protoPoi.Name = packet.ReadWoWString("Name", bit84);

            var lastGossipOption = CoreParsers.NpcHandler.LastGossipOption;
            var tempGossipOptionPOI = CoreParsers.NpcHandler.TempGossipOptionPOI;

            lastGossipOption.ActionPoiId = gossipPOI.ID;
            tempGossipOptionPOI.ActionPoiId = gossipPOI.ID;

            Storage.GossipPOIs.Add(gossipPOI, packet.TimeSpan);

            if (tempGossipOptionPOI.HasSelection)
            {
                if ((packet.TimeSpan - tempGossipOptionPOI.TimeSpan).Duration() <= TimeSpan.FromMilliseconds(2500))
                {
                    if (tempGossipOptionPOI.ActionMenuId != null)
                    {
                        Storage.GossipMenuOptionActions.Add(new GossipMenuOptionAction { MenuId = tempGossipOptionPOI.MenuId, OptionIndex = tempGossipOptionPOI.OptionIndex, ActionMenuId = tempGossipOptionPOI.ActionMenuId, ActionPoiId = gossipPOI.ID }, packet.TimeSpan);
                        //clear temp
                        tempGossipOptionPOI.Reset();
                    }
                }
                else
                {
                    lastGossipOption.Reset();
                    tempGossipOptionPOI.Reset();
                }
            }
        }

        [Parser(Opcode.SMSG_VENDOR_INVENTORY)]
        public static void HandleVendorInventory(Packet packet)
        {
            uint entry = packet.ReadPackedGuid128("VendorGUID").GetEntry();
            packet.ReadByte("Reason");
            int count = packet.ReadInt32("VendorItems");

            for (int i = 0; i < count; ++i)
            {
                NpcVendor vendor = new NpcVendor
                {
                    Entry = entry,
                    Slot = packet.ReadInt32("Muid", i),
                    Type = (uint)packet.ReadInt32("Type", i)
                };

                int maxCount = packet.ReadInt32("Quantity", i);
                packet.ReadInt64("Price", i);
                packet.ReadInt32("Durability", i);
                int buyCount = packet.ReadInt32("StackCount", i);
                vendor.ExtendedCost = packet.ReadUInt32("ExtendedCostID", i);
                vendor.PlayerConditionID = packet.ReadUInt32("PlayerConditionFailed", i);

                vendor.Item = Substructures.ItemHandler.ReadItemInstance(packet, i).ItemID;
                packet.ResetBitReader();
                vendor.IgnoreFiltering = packet.ReadBit("DoNotFilterOnVendor", i);
                if (ClientVersion.AddedInVersion(ClientVersionBuild.V8_1_0_28724))
                    packet.ReadBit("Refundable", i);

                vendor.MaxCount = maxCount == -1 ? 0 : (uint)maxCount; // TDB
                if (vendor.Type == 2)
                    vendor.MaxCount = (uint)buyCount;

                Storage.NpcVendors.Add(vendor, packet.TimeSpan);
            }

            CoreParsers.NpcHandler.LastGossipOption.Reset();
            CoreParsers.NpcHandler.TempGossipOptionPOI.Reset();
        }

        [Parser(Opcode.SMSG_GOSSIP_QUEST_UPDATE)]
        public static void HandleGossipQuestUpdate(Packet packet)
        {
            packet.ReadPackedGuid128("GossipGUID");

            packet.ReadInt32<QuestId>("QuestID");
            packet.ReadInt32("QuestType");
            packet.ReadInt32("QuestLevel");
            packet.ReadInt32("QuestMaxScalingLevel");

            for (int i = 0; i < 2; ++i)
                packet.ReadInt32("QuestFlags", i);

            packet.ResetBitReader();

            packet.ReadBit("Repeatable");
            uint questTitleLen = packet.ReadBits(9);

            packet.ReadWoWString("QuestTitle", questTitleLen);
        }
    }
}

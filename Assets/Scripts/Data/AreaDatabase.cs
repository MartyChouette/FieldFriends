using System.Collections.Generic;

namespace FieldFriends.Data
{
    /// <summary>
    /// Hardcoded area definitions for all 7 areas in Bramble Vale.
    /// Connectivity, encounter tables, and metadata.
    /// </summary>
    public static class AreaDatabase
    {
        public struct AreaDef
        {
            public AreaID ID;
            public string Name;
            public bool HasEncounters;
            public float EncounterRate;
            public EncounterSlot[] Encounters;
            public AreaID[] Connections;
        }

        public static readonly AreaDef[] All = new AreaDef[]
        {
            new AreaDef {
                ID = AreaID.WillowEnd,
                Name = "Willow End",
                HasEncounters = false,
                EncounterRate = 0f,
                Encounters = new EncounterSlot[0],
                Connections = new[] { AreaID.SouthField, AreaID.CreekPath }
            },
            new AreaDef {
                ID = AreaID.SouthField,
                Name = "South Field",
                HasEncounters = true,
                EncounterRate = 0.10f,
                Encounters = new[] {
                    new EncounterSlot { creatureName = "Mossbit",  rarity = EncounterRarity.Common },
                    new EncounterSlot { creatureName = "Bramblet", rarity = EncounterRarity.Uncommon },
                },
                Connections = new[] { AreaID.WillowEnd, AreaID.HillRoad }
            },
            new AreaDef {
                ID = AreaID.CreekPath,
                Name = "Creek Path",
                HasEncounters = true,
                EncounterRate = 0.10f,
                Encounters = new[] {
                    new EncounterSlot { creatureName = "Plen",    rarity = EncounterRarity.Common },
                    new EncounterSlot { creatureName = "Ripplet", rarity = EncounterRarity.Uncommon },
                    new EncounterSlot { creatureName = "Drift",   rarity = EncounterRarity.Rare },
                },
                Connections = new[] { AreaID.WillowEnd, AreaID.HillRoad }
            },
            new AreaDef {
                ID = AreaID.HillRoad,
                Name = "Hill Road",
                HasEncounters = true,
                EncounterRate = 0.08f,
                Encounters = new[] {
                    new EncounterSlot { creatureName = "Skirl", rarity = EncounterRarity.Common },
                    new EncounterSlot { creatureName = "Flit",  rarity = EncounterRarity.Uncommon },
                },
                Connections = new[] { AreaID.SouthField, AreaID.CreekPath, AreaID.Stonebridge }
            },
            new AreaDef {
                ID = AreaID.Stonebridge,
                Name = "Stonebridge",
                HasEncounters = true,
                EncounterRate = 0.06f,
                Encounters = new[] {
                    new EncounterSlot { creatureName = "Loamle", rarity = EncounterRarity.Common },
                    new EncounterSlot { creatureName = "Wispin", rarity = EncounterRarity.Uncommon },
                },
                Connections = new[] { AreaID.HillRoad, AreaID.NorthMeadow }
            },
            new AreaDef {
                ID = AreaID.NorthMeadow,
                Name = "North Meadow",
                HasEncounters = true,
                EncounterRate = 0.12f,
                Encounters = new[] {
                    new EncounterSlot { creatureName = "Petalyn", rarity = EncounterRarity.Uncommon },
                    new EncounterSlot { creatureName = "Bloomel", rarity = EncounterRarity.Rare },
                },
                Connections = new[] { AreaID.Stonebridge, AreaID.QuietGrove }
            },
            new AreaDef {
                ID = AreaID.QuietGrove,
                Name = "Quiet Grove",
                HasEncounters = false, // No random encounters
                EncounterRate = 0f,
                Encounters = new EncounterSlot[0],
                // Still is a single optional encounter, handled via special NPC/trigger
                Connections = new[] { AreaID.NorthMeadow }
            },
        };

        static Dictionary<AreaID, AreaDef> _lookup;

        public static AreaDef Get(AreaID id)
        {
            if (_lookup == null)
            {
                _lookup = new Dictionary<AreaID, AreaDef>();
                for (int i = 0; i < All.Length; i++)
                    _lookup[All[i].ID] = All[i];
            }
            return _lookup[id];
        }
    }
}

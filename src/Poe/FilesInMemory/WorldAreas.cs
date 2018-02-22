﻿using PoeHUD.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Runtime.InteropServices;
using PoeHUD.Controllers;

namespace PoeHUD.Poe.FilesInMemory
{
    public class WorldAreas : FileInMemory
    {
        public Dictionary<string, WorldArea> WorldAreaDictionary = new Dictionary<string, WorldArea>(StringComparer.OrdinalIgnoreCase);

        public WorldAreas(Memory m, long address)
            : base(m, address)
        {
        }

        public WorldArea Translate(string areaId)
        {
            if(WorldAreaDictionary.Count == 0)
            {
                ReloadCache();
                DebugPlug.DebugPlugin.LogMsg("Debug (Temporary): Reloading WorldAreas.dat cache", 10);
            }

            WorldArea result;
            if (WorldAreaDictionary.TryGetValue(areaId, out result))
                result.ReadData();

            return result;
        }

        //If the game pointer got changed we can call this to update cache, just WorldAreas should be updated
        public void ReloadCache()
        {
            int counter = 0;
            WorldAreaDictionary.Clear();

            foreach (long addr in RecordAddresses())
            {
                var r = new WorldArea(M, addr);
                if (!WorldAreaDictionary.ContainsKey(r.Id))
                    WorldAreaDictionary.Add(r.Id, r);
            }
        }


        public class WorldArea
        { 
            public readonly string Id;
            public long Address { get; private set; }
            public string Name { get; private set; }
            public int Act { get; private set; }
            public bool IsTown { get; private set; }
            public bool HasWaypoint { get; private set; }
            public int AreaLevel { get; private set; }
            public int WorldAreaId { get; private set; }

            private int ConnectionsCount;
            private long ConnectionsPointer;

            private int CorruptedAreasCount;
            private long CorruptedAreasPtr;
            
            public WorldArea(Memory m, long addr)
            {
                Address = addr;
                Id = m.ReadStringU(m.ReadLong(Address), 255);
            }

            internal void ReadData()
            {
                var m = GameController.Instance.Memory;

                Name = m.ReadStringU(m.ReadLong(Address + 8), 255);
                Act = m.ReadInt(Address + 0x10);
                IsTown = m.ReadByte(Address + 0x14) == 1;
                HasWaypoint = m.ReadByte(Address + 0x15) == 1;
                ConnectionsCount = m.ReadInt(Address + 0x16);
                ConnectionsPointer = m.ReadLong(Address + 0x1e);

                AreaLevel = m.ReadInt(Address + 0x26);
                WorldAreaId = m.ReadInt(Address + 0x2a);
                //var Unknown6 = m.ReadInt(addr + 0x2a);
                //var Unknown7 = m.ReadInt(addr + 0x2e);
                //var Unknown8 = m.ReadInt(addr + 0x32);
                //LoadingScreenImage = m.ReadStringU(m.ReadLong(addr + 0x36), 255);

                CorruptedAreasCount = m.ReadInt(Address + 0xfb);
                CorruptedAreasPtr = m.ReadLong(Address + 0x103);
            }

            private List<WorldArea> connections;
            public List<WorldArea> Connections
            {
                get
                {
                    if (connections == null)
                    {
                        connections = new List<WorldArea>();
                        var m = GameController.Instance.Memory;

                        var addr = ConnectionsPointer;
                        for (int i = 0; i < ConnectionsCount; i++)
                        {
                            var newAres = new WorldArea(m, m.ReadLong(addr));
                            newAres.ReadData();
                            connections.Add(newAres);
                            addr += 8;
                        }
                    }
                    return connections;
                }
            }


            private List<WorldArea> corruptedAreas;
            public List<WorldArea> CorruptedAreas
            {
                get
                {
                    if (corruptedAreas == null)
                    {
                        corruptedAreas = new List<WorldArea>();
                        var m = GameController.Instance.Memory;

                        var addr = CorruptedAreasPtr;
                        for (int i = 0; i < CorruptedAreasCount; i++)
                        {
                            var newAres = new WorldArea(m, m.ReadLong(addr));
                            newAres.ReadData();
                            corruptedAreas.Add(newAres);
                            addr += 8;
                        }
                    }
                    return corruptedAreas;
                }
            }
        }
    }
}
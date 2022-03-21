using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Input;
using EvoEditApp;

namespace EvoEditApp
{
    [MessagePackObject(false)]
    public class BrickEntityMp
    {
    
        [Key(0)]
        public string Sguid { get; set; }

        [Key(1)]
        public string EntityName { get; set; }

        [Key(2)]
        public object ShipyardRespawnPoint { get; set; }

        [Key(3)]
        public Dictionary<int, Dictionary<int, object>> LegacyCommands { get; set; }

        [Key(4)]
        public int EntityType { get; set; }

        [Key(5)]
        public ulong SteamFileId { get; set; }
        [Key(6)]
        public object InventoryMp { get; set; }

        [Key(7)]
        public List<BrickDatasSave> BrickDatasChildrens { get; set; }

    
        [Key(8)]
        public BrickDatasSave BrickDatas { get; set; }


        [Key(9)]
        public object Commands { get; set; }


        [Key(10)]
        public object ClipboardData { get; set; }
        
        [Key(11)]
        public int Version { get; set; }

        [Key(12)]
        public object BrickTagDatas { get; set; }

        public void ClearInstanceData()
        {
            this.Sguid = null;
        }

        public static BrickEntityMp GetSaveFromFile(FileStream file, bool isNewInstance)
        {
            BrickEntityMp result;
            try
            {
                //var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);
                BrickEntityMp brickEntityMp = LZ4MessagePackSerializer.Deserialize<BrickEntityMp>(file);
                if (isNewInstance)
                {
                    brickEntityMp.ClearInstanceData();
                }

                result = brickEntityMp;
            }
            catch (Exception arg)
            {
                MessageBox.Show("Issue with blueprint load: " + arg);
                result = null;
            }

            return result;
        }

    }

   

    [MessagePackObject(false)]
    public struct BrickDatasSave
    {
        [IgnoreMember]
        public int Length
        {
            get
            {
                BrickInstanceData[] array = this.Datas;
                if (array == null)
                {
                    return 0;
                }

                return array.Length;
            }
        }

        [Key(0)] public BrickInstanceData[] Datas;


        [Key(1)] public int[] IdsToRecycle;

        [Key(2)] public Dictionary<int, byte[]> AdditionalDatas;
    }
}
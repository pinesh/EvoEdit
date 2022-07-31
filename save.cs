using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;

namespace EvoEditApp
{
    public class ParentEntity
    {
    
        private BrickEntityMp _srcdata;

        public ParentEntity(BrickEntityMp src)
        {
            _srcdata = src;
        }

        public void Append(BrickDatasSave b)
        {
            _srcdata.BrickDatas = b;
        }
        // Token: 0x17000420 RID: 1056
        // (get) Token: 0x060014D2 RID: 5330 RVA: 0x00072959 File Offset: 0x00070B59
        public BrickEntityMp GetMessagePack()
        {
            return GetSave(this);
        }

        // Token: 0x060012D6 RID: 4822 RVA: 0x000689EC File Offset: 0x00066BEC
        public static BrickEntityMp GetSave(ParentEntity parent)
        {
            parent._srcdata.Sguid = Makeid();
            return parent._srcdata;
        }

        private static readonly System.Random Random = new System.Random();

        public static string Makeid()
        {
            DateTime d = new DateTime(2018, 1, 1, 8, 0, 0, DateTimeKind.Utc);
            double num = (double)(DateTime.UtcNow - d).Ticks;
            return string.Format("{0:X}", Convert.ToInt32(num % 2147483647.0)) + "-" +
                   string.Format("{0:X}", Random.Next(1000000));
        }
        
        // Token: 0x06001537 RID: 5431 RVA: 0x000740C0 File Offset: 0x000722C0
        public void SaveToDiskAtPath(string path, bool clearSguid)
        {
            string directoryName = Path.GetDirectoryName(path);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            if (File.Exists(path))
            {
                if (new FileInfo(path).Length > 1L)
                {
                    File.Copy(path, path + "bu", true);
                }
                else
                {
                    throw new Exception("Error, file is null");
                }
            }

            byte[] array;
            try
            {
                BrickEntityMp messagePack = this.GetMessagePack();
                if (clearSguid)
                {
                    messagePack.ClearInstanceData();
                }
                // var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);
                array = LZ4MessagePackSerializer.Serialize<BrickEntityMp>(messagePack);
            }
            catch
            {
                throw new Exception("save failed");
            }

            if (array != null && array.Length != 0)
            {
                var baseD = new DirectoryInfo(Path.GetDirectoryName(path));

                //if we are not already in target folder
                if (baseD.Name != Path.GetFileNameWithoutExtension(path))
                {
                    var d = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(path),
                        Path.GetFileNameWithoutExtension(path)));
                    if (!d.Exists)
                    {
                        d.Create();
                    }
                    path = Path.Combine(d.FullName, Path.GetFileName(path));
                }

           
                using (FileStream fileStream = File.Create(path))
                {
                    fileStream.Write(array, 0, array.Length);
                }
            }
        }
        public void SaveToDiskAtPathChild(string path, bool clearSguid,List<BrickDatasSave> children)
        {
            string directoryName = Path.GetDirectoryName(path);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            if (File.Exists(path))
            {
                if (new FileInfo(path).Length > 1L)
                {
                    File.Copy(path, path + "bu", true);
                }
                else
                {
                    throw new Exception("Error, file is null");
                }
            }

            byte[] array;
            try
            {
                BrickEntityMp messagePack = this.GetMessagePack();
                messagePack.BrickDatasChildrens = children;
                if (clearSguid)
                {
                    messagePack.ClearInstanceData();
                }
                // var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);
                array = LZ4MessagePackSerializer.Serialize<BrickEntityMp>(messagePack);
            }
            catch
            {
                throw new Exception("save failed");
            }

            if (array != null && array.Length != 0)
            {
                var baseD = new DirectoryInfo(Path.GetDirectoryName(path));

                //if we are not already in target folder
                if (baseD.Name != Path.GetFileNameWithoutExtension(path))
                {
                    var d = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(path),
                        Path.GetFileNameWithoutExtension(path)));
                    if (!d.Exists)
                    {
                        d.Create();
                    }
                    path = Path.Combine(d.FullName, Path.GetFileName(path));
                }

            
                using (FileStream fileStream = File.Create(path))
                {
                    fileStream.Write(array, 0, array.Length);
                }
            }
        }
    }
}

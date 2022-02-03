using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;

namespace testapp1
{
    public class ParentEntity
    {
    
        private BrickEntity_mp srcdata;

        public ParentEntity(BrickEntity_mp src)
        {
            srcdata = src;
        }

        public void Append(BrickDatasSave b)
        {
            srcdata.brickDatas = b;
        }
        // Token: 0x17000420 RID: 1056
        // (get) Token: 0x060014D2 RID: 5330 RVA: 0x00072959 File Offset: 0x00070B59
        public BrickEntity_mp GetMessagePack()
        {
            return GetSave(this);
        }

        // Token: 0x060012D6 RID: 4822 RVA: 0x000689EC File Offset: 0x00066BEC
        public static BrickEntity_mp GetSave(ParentEntity parent)
        {
            parent.srcdata.SGUID = makeid();
            return parent.srcdata;
        }

        private static readonly System.Random random = new System.Random();

        public static string makeid()
        {
            DateTime d = new DateTime(2018, 1, 1, 8, 0, 0, DateTimeKind.Utc);
            double num = (double)(DateTime.UtcNow - d).Ticks;
            return string.Format("{0:X}", Convert.ToInt32(num % 2147483647.0)) + "-" +
                   string.Format("{0:X}", random.Next(1000000));
        }
        
        // Token: 0x06001537 RID: 5431 RVA: 0x000740C0 File Offset: 0x000722C0
        public void SaveToDiskAtPath(string path, bool clearSGUID)
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
                BrickEntity_mp messagePack = this.GetMessagePack();
                if (clearSGUID)
                {
                    messagePack.ClearInstanceData();
                }
                // var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);
                array = LZ4MessagePackSerializer.Serialize<BrickEntity_mp>(messagePack);
            }
            catch
            {
                throw new Exception("save failed");
            }

            if (array != null && array.Length != 0)
            {
                using (FileStream fileStream = File.Create(path))
                {
                    fileStream.Write(array, 0, array.Length);
                }
            }
        }
    }
}

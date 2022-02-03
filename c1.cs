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
    public class BrickEntity_mp
    {
        // Token: 0x17000394 RID: 916
        // (get) Token: 0x060012BB RID: 4795 RVA: 0x000688C6 File Offset: 0x00066AC6
        // (set) Token: 0x060012BA RID: 4794 RVA: 0x000688BD File Offset: 0x00066ABD
        [Key(0)]
        public string SGUID { get; set; }

        // Token: 0x17000395 RID: 917
        // (get) Token: 0x060012BD RID: 4797 RVA: 0x000688D7 File Offset: 0x00066AD7
        // (set) Token: 0x060012BC RID: 4796 RVA: 0x000688CE File Offset: 0x00066ACE
        [Key(1)]
        public string entityName { get; set; }

        // Token: 0x17000396 RID: 918
        // (get) Token: 0x060012BF RID: 4799 RVA: 0x000688E8 File Offset: 0x00066AE8
        // (set) Token: 0x060012BE RID: 4798 RVA: 0x000688DF File Offset: 0x00066ADF
        [Key(2)]
        public object ShipyardRespawnPoint { get; set; }

        // Token: 0x17000397 RID: 919
        // (get) Token: 0x060012C1 RID: 4801 RVA: 0x000688F9 File Offset: 0x00066AF9
        // (set) Token: 0x060012C0 RID: 4800 RVA: 0x000688F0 File Offset: 0x00066AF0
        [Key(3)]
        public Dictionary<int, Dictionary<int, object>> legacy_commands { get; set; }

        // Token: 0x17000398 RID: 920
        // (get) Token: 0x060012C3 RID: 4803 RVA: 0x0006890A File Offset: 0x00066B0A
        // (set) Token: 0x060012C2 RID: 4802 RVA: 0x00068901 File Offset: 0x00066B01
        [Key(4)]
        public int entityType { get; set; }

        // Token: 0x17000399 RID: 921
        // (get) Token: 0x060012C5 RID: 4805 RVA: 0x0006891B File Offset: 0x00066B1B
        // (set) Token: 0x060012C4 RID: 4804 RVA: 0x00068912 File Offset: 0x00066B12
        [Key(5)]
        public ulong steamFileId { get; set; }

        // Token: 0x1700039A RID: 922
        // (get) Token: 0x060012C7 RID: 4807 RVA: 0x0006892C File Offset: 0x00066B2C
        // (set) Token: 0x060012C6 RID: 4806 RVA: 0x00068923 File Offset: 0x00066B23
        [Key(6)]
        public object inventory_mp { get; set; }

        // Token: 0x1700039B RID: 923
        // (get) Token: 0x060012C9 RID: 4809 RVA: 0x0006893D File Offset: 0x00066B3D
        // (set) Token: 0x060012C8 RID: 4808 RVA: 0x00068934 File Offset: 0x00066B34
        [Key(7)]
        public List<BrickDatasSave> brickDatasChildrens { get; set; }

        // Token: 0x1700039C RID: 924
        // (get) Token: 0x060012CB RID: 4811 RVA: 0x0006894E File Offset: 0x00066B4E
        // (set) Token: 0x060012CA RID: 4810 RVA: 0x00068945 File Offset: 0x00066B45
        [Key(8)]
        public BrickDatasSave brickDatas { get; set; }

        // Token: 0x1700039D RID: 925
        // (get) Token: 0x060012CD RID: 4813 RVA: 0x0006895F File Offset: 0x00066B5F
        // (set) Token: 0x060012CC RID: 4812 RVA: 0x00068956 File Offset: 0x00066B56
        [Key(9)]
        public object commands { get; set; }

        // Token: 0x1700039E RID: 926
        // (get) Token: 0x060012CF RID: 4815 RVA: 0x00068970 File Offset: 0x00066B70
        // (set) Token: 0x060012CE RID: 4814 RVA: 0x00068967 File Offset: 0x00066B67
        [Key(10)]
        public object clipboardData { get; set; }

        // Token: 0x1700039F RID: 927
        // (get) Token: 0x060012D1 RID: 4817 RVA: 0x00068981 File Offset: 0x00066B81
        // (set) Token: 0x060012D0 RID: 4816 RVA: 0x00068978 File Offset: 0x00066B78
        [Key(11)]
        public int version { get; set; }

        // Token: 0x170003A0 RID: 928
        // (get) Token: 0x060012D3 RID: 4819 RVA: 0x00068992 File Offset: 0x00066B92
        // (set) Token: 0x060012D2 RID: 4818 RVA: 0x00068989 File Offset: 0x00066B89
        [Key(12)]
        public object brickTagDatas { get; set; }

        public void ClearInstanceData()
        {
            this.SGUID = null;
        }

        public static BrickEntity_mp GetSaveFromFile(FileStream file, bool isNewInstance)
        {
            BrickEntity_mp result;
            try
            {
                //var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);
                BrickEntity_mp brickEntity_mp = LZ4MessagePackSerializer.Deserialize<BrickEntity_mp>(file);
                if (isNewInstance)
                {
                    brickEntity_mp.ClearInstanceData();
                }

                result = brickEntity_mp;
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
    [Serializable]
    public class Vector3i
    {
        // Token: 0x060000FF RID: 255 RVA: 0x000075AE File Offset: 0x000057AE
        public Vector3 ToVector3()
        {
            return new Vector3((float)this.x, (float)this.y, (float)this.z);
        }


        // Token: 0x060000F2 RID: 242 RVA: 0x00007370 File Offset: 0x00005570
        public Vector3i(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public Vector3i()
        {
            this.x = 0;
            this.y = 0;
            this.z = 0;
        }

        

        // Token: 0x060000F3 RID: 243 RVA: 0x00007387 File Offset: 0x00005587
        public Vector3i(int x, int y)
        {
            this.x = x;
            this.y = y;
            this.z = 0;
        }

        // Token: 0x060000F4 RID: 244 RVA: 0x0000739E File Offset: 0x0000559E
        public Vector3i(Vector3 a)
        {
            this.x = (int)Math.Round(a.X);
            this.y = (int)Math.Round(a.Y);
            this.z = (int)Math.Round(a.Z);
        }
        
        // Token: 0x060000F8 RID: 248 RVA: 0x0000744C File Offset: 0x0000564C
        public static int DistanceSquared(Vector3i a, Vector3i b)
        {
            int num = b.x - a.x;
            int num2 = b.y - a.y;
            int num3 = b.z - a.z;
            return num * num + num2 * num2 + num3 * num3;
        }

        // Token: 0x060000F9 RID: 249 RVA: 0x0000748C File Offset: 0x0000568C
        public int DistanceSquared(Vector3i v)
        {
            return Vector3i.DistanceSquared(this, v);
        }

        // Token: 0x060000FA RID: 250 RVA: 0x0000749A File Offset: 0x0000569A
        public override int GetHashCode()
        {
            return this.x ^ this.y << 2 ^ this.z >> 2;
        }

        // Token: 0x060000FB RID: 251 RVA: 0x000074B4 File Offset: 0x000056B4
        public ulong GetSeed()
        {
            return (ulong)((long)this.x ^ (long)this.y << 2 ^ (long)((ulong)((long)this.z) >> 2));
        }

        // Token: 0x060000FC RID: 252 RVA: 0x000074D4 File Offset: 0x000056D4
        public override bool Equals(object other)
        {
            if (!(other is Vector3i))
            {
                return false;
            }
            Vector3i vector3i = (Vector3i)other;
            return this.x == vector3i.x && this.y == vector3i.y && this.z == vector3i.z;
        }

        // Token: 0x060000FD RID: 253 RVA: 0x0000751E File Offset: 0x0000571E
        public bool Equals(Vector3i other)
        {
            return this.x == other.x && this.y == other.y && this.z == other.z;
        }

        // Token: 0x060000FE RID: 254 RVA: 0x0000754C File Offset: 0x0000574C
        public override string ToString()
        {
            return string.Concat(new object[]
            {
            "(",
            this.x,
            " ",
            this.y,
            " ",
            this.z,
            ")"
            });
        }


        // Token: 0x06000102 RID: 258 RVA: 0x000075FF File Offset: 0x000057FF
        public ushort ToUshort()
        {
            return (ushort)(this.x | this.y << 5 | this.z << 10);
        }

        // Token: 0x06000103 RID: 259 RVA: 0x0000761B File Offset: 0x0000581B
        public static Vector3i Min(Vector3i a, Vector3i b)
        {
            return a == null && b == null ? null :
                a == null ? b :
                b == null ? a : new Vector3i(Math.Min(a.x, b.x), Math.Min(a.y, b.y), Math.Min(a.z, b.z));
        }

        // Token: 0x06000104 RID: 260 RVA: 0x00007655 File Offset: 0x00005855
        public static Vector3i Max(Vector3i a, Vector3i b)
        {
            return a == null && b == null ? null :
                a == null ? b :
                b == null ? a : new Vector3i(Math.Max(a.x, b.x), Math.Max(a.y, b.y), Math.Max(a.z, b.z));
        }

       
        // Token: 0x06000106 RID: 262 RVA: 0x000076E7 File Offset: 0x000058E7
        public static Vector3i CeilToInt(Vector3 a)
        {
            return new Vector3i((int)Math.Round(Math.Ceiling(a.X)), (int)Math.Round(Math.Ceiling(a.Y)), (int)Math.Round(Math.Ceiling(a.Z)) );
        }

        // Token: 0x06000107 RID: 263 RVA: 0x0000770F File Offset: 0x0000590F
        public static Vector3i FloorToInt(Vector3 a)
        {
            return new Vector3i((int)Math.Round(Math.Floor(a.X)), (int)Math.Round(Math.Floor(a.Y)), (int)Math.Round(Math.Floor(a.Z)));
        }

        // Token: 0x06000108 RID: 264 RVA: 0x00007738 File Offset: 0x00005938
        public int CompareTo(Vector3i other)
        {
            if (this.x < other.x)
            {
                return -1;
            }
            if (this.x > other.x)
            {
                return 1;
            }
            if (this.y < other.y)
            {
                return -1;
            }
            if (this.y > other.y)
            {
                return 1;
            }
            if (this.z < other.z)
            {
                return -1;
            }
            if (this.z > other.z)
            {
                return 1;
            }
            return this.z;
        }
        // Token: 0x06000109 RID: 265 RVA: 0x0000751E File Offset: 0x0000571E
        public static bool operator ==(Vector3i a, Vector3i b)
        {
            if (!(a is null) && !(b is null)) return a.x == b.x && a.y == b.y && a.z == b.z;
            return a is null && b is null;
            // if(a ==null || b== null) return false;
        }

        // Token: 0x0600010A RID: 266 RVA: 0x000077AB File Offset: 0x000059AB
        public static bool operator !=(Vector3i a, Vector3i b)
        {
            if (!(a is null) && !(b is null)) return a.x != b.x || a.y != b.y || a.z != b.z;
            return !(a is null) || !(b is null);
        }

        // Token: 0x0600010B RID: 267 RVA: 0x000077DC File Offset: 0x000059DC
        public static Vector3i operator -(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        // Token: 0x0600010C RID: 268 RVA: 0x0000780A File Offset: 0x00005A0A
        public static Vector3i operator +(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        // Token: 0x0600010D RID: 269 RVA: 0x00007838 File Offset: 0x00005A38
        public static Vector3i operator -(Vector3i b)
        {
            return new Vector3i(-b.x, -b.y, -b.z);
        }

        // Token: 0x0600010E RID: 270 RVA: 0x00007854 File Offset: 0x00005A54
        public static Vector3 operator +(Vector3 a, Vector3i b)
        {
            return new Vector3(a.X + (float)b.x, a.Y + (float)b.y, a.Z + (float)b.z);
        }

        // Token: 0x0600010F RID: 271 RVA: 0x00007885 File Offset: 0x00005A85
        public static Vector3 operator +(Vector3i a, Vector3 b)
        {
            return new Vector3((float)a.x + b.X, (float)a.y + b.Y, (float)a.z + b.Z);
        }

        // Token: 0x06000110 RID: 272 RVA: 0x000078B6 File Offset: 0x00005AB6
        public static Vector3 operator -(Vector3i a, Vector3 b)
        {
            return new Vector3((float)a.x - b.X, (float)a.y - b.Y, (float)a.z - b.Z);
        }

        // Token: 0x06000111 RID: 273 RVA: 0x000078E7 File Offset: 0x00005AE7
        public static Vector3 operator -(Vector3 a, Vector3i b)
        {
            return new Vector3(a.X - (float)b.x, a.Y - (float)b.y, a.Z - (float)b.z);
        }

        // Token: 0x06000112 RID: 274 RVA: 0x00007918 File Offset: 0x00005B18
        public static Vector3i operator *(int a, Vector3i b)
        {
            return new Vector3i(a * b.x, a * b.y, a * b.z);
        }

        // Token: 0x06000114 RID: 276 RVA: 0x00007959 File Offset: 0x00005B59
        public static Vector3i operator %(Vector3i b, int a)
        {
            return new Vector3i(b.x % a, b.y % a, b.z % a);
        }

        // Token: 0x06000115 RID: 277 RVA: 0x00007978 File Offset: 0x00005B78
        public static Vector3i operator /(Vector3i a, int i)
        {
            int num = a.x / i;
            int num2 = a.y / i;
            int num3 = a.z / i;
            return new Vector3i(num, num2, num3);
        }

        // Token: 0x06000116 RID: 278 RVA: 0x000079A6 File Offset: 0x00005BA6
        public static int Dot(Vector3i a, Vector3i b)
        {
            return (int)Vector3.Dot(a.ToVector3(), b.ToVector3());
        }

        public int this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return this.x;
                    case 1:
                        return this.y;
                    case 2:
                        return this.z;
                }
                return 0;
            }
            set
            {
                switch (i)
                {
                    case 0:
                        this.x = value;
                        break;
                    case 1:
                        this.y = value;
                        break;
                    case 2:
                        this.z = value;
                        break;
                }
            }
        }

        // Token: 0x06000117 RID: 279 RVA: 0x000079BC File Offset: 0x00005BBC
                public static Vector3i Scale(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.x * b.x, a.y * b.y, a.z * b.z);
        }
        // Token: 0x06000117 RID: 279 RVA: 0x000079BC File Offset: 0x00005BBC
        public static Vector3i Scale(Vector3i a,int n, int d)
        {
            return new Vector3i((a.x*n)/d , (a.y*n)/d , (a.z*n)/d );
        }

        // Token: 0x06000118 RID: 280 RVA: 0x000079EA File Offset: 0x00005BEA
        public static Vector3i Abs(Vector3i a)
        {
            return new Vector3i(Math.Abs(a.x), Math.Abs(a.y), Math.Abs(a.z));
        }

        [Key(0)] public int x;

        // Token: 0x04000090 RID: 144
        [Key(1)] public int y;

        // Token: 0x04000091 RID: 145
        [Key(2)] public int z;

        // Token: 0x04000092 RID: 146
        public static readonly Vector3i zero = new Vector3i(0, 0, 0);

        // Token: 0x04000093 RID: 147
        public static readonly Vector3i one = new Vector3i(1, 1, 1);

        // Token: 0x04000094 RID: 148
        public static readonly Vector3i forward = new Vector3i(0, 0, 1);

        // Token: 0x04000095 RID: 149
        public static readonly Vector3i back = new Vector3i(0, 0, -1);

        // Token: 0x04000096 RID: 150
        public static readonly Vector3i up = new Vector3i(0, 1, 0);

        // Token: 0x04000097 RID: 151
        public static readonly Vector3i down = new Vector3i(0, -1, 0);

        // Token: 0x04000098 RID: 152
        public static readonly Vector3i left = new Vector3i(-1, 0, 0);

        // Token: 0x04000099 RID: 153
        public static readonly Vector3i right = new Vector3i(1, 0, 0);

        // Token: 0x0400009A RID: 154
        public static readonly Vector3i blockOffset = new Vector3i(0, -16, 0);

        // Token: 0x0200002A RID: 42
        [Serializable]
        public class Vector3iComparer : IEqualityComparer<Vector3i>
        {
            // Token: 0x0600011A RID: 282 RVA: 0x00007A97 File Offset: 0x00005C97
            public bool Equals(Vector3i a, Vector3i b)
            {
                return a.x == b.x && a.y == b.y && a.z == b.z;
            }

            // Token: 0x0600011B RID: 283 RVA: 0x00007AC5 File Offset: 0x00005CC5
            public int GetHashCode(Vector3i obj)
            {
                return obj.x ^ obj.y << 2 ^ obj.z >> 2;
            }
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
                BrickInstanceData[] array = this.datas;
                if (array == null)
                {
                    return 0;
                }

                return array.Length;
            }
        }

        [Key(0)] public BrickInstanceData[] datas;


        [Key(1)] public int[] idsToRecycle;

        [Key(2)] public Dictionary<int, byte[]> additionalDatas;
    }
}
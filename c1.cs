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
    [Serializable]
    public class Vector3i
    {
        public Vector3 ToVector3()
        {
            return new Vector3((float)this.X, (float)this.Y, (float)this.Z);
        }


        public Vector3i(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
        public Vector3i()
        {
            this.X = 0;
            this.Y = 0;
            this.Z = 0;
        }

        public Vector3i(int x, int y)
        {
            this.X = x;
            this.Y = y;
            this.Z = 0;
        }

        public Vector3i(Vector3 a)
        {
            this.X = (int)Math.Round(a.X);
            this.Y = (int)Math.Round(a.Y);
            this.Z = (int)Math.Round(a.Z);
        }
        
        public static int DistanceSquared(Vector3i a, Vector3i b)
        {
            int num = b.X - a.X;
            int num2 = b.Y - a.Y;
            int num3 = b.Z - a.Z;
            return num * num + num2 * num2 + num3 * num3;
        }

        public int DistanceSquared(Vector3i v)
        {
            return Vector3i.DistanceSquared(this, v);
        }

        public override int GetHashCode()
        {
            return this.X ^ this.Y << 2 ^ this.Z >> 2;
        }

        public ulong GetSeed()
        {
            return (ulong)((long)this.X ^ (long)this.Y << 2 ^ (long)((ulong)((long)this.Z) >> 2));
        }

        public override bool Equals(object other)
        {
            if (!(other is Vector3i))
            {
                return false;
            }
            Vector3i Vector3i = (Vector3i)other;
            return this.X == Vector3i.X && this.Y == Vector3i.Y && this.Z == Vector3i.Z;
        }

        public bool Equals(Vector3i other)
        {
            return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
        }

        public override string ToString()
        {
            return string.Concat(new object[]
            {
            "(",
            this.X,
            " ",
            this.Y,
            " ",
            this.Z,
            ")"
            });
        }


        public ushort ToUshort()
        {
            return (ushort)(this.X | this.Y << 5 | this.Z << 10);
        }

        public static Vector3i Min(Vector3i a, Vector3i b)
        {
            return a == null && b == null ? null :
                a == null ? b :
                b == null ? a : new Vector3i(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));
        }

        public static Vector3i Max(Vector3i a, Vector3i b)
        {
            return a == null && b == null ? null :
                a == null ? b :
                b == null ? a : new Vector3i(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));
        }

        
        public static Vector3i CeilToInt(Vector3 a)
        {
            return new Vector3i((int)Math.Round(Math.Ceiling(a.X)), (int)Math.Round(Math.Ceiling(a.Y)), (int)Math.Round(Math.Ceiling(a.Z)) );
        }

        public static Vector3i FloorToInt(Vector3 a)
        {
            return new Vector3i((int)Math.Round(Math.Floor(a.X)), (int)Math.Round(Math.Floor(a.Y)), (int)Math.Round(Math.Floor(a.Z)));
        }

        public int CompareTo(Vector3i other)
        {
            if (this.X < other.X)
            {
                return -1;
            }
            if (this.X > other.X)
            {
                return 1;
            }
            if (this.Y < other.Y)
            {
                return -1;
            }
            if (this.Y > other.Y)
            {
                return 1;
            }
            if (this.Z < other.Z)
            {
                return -1;
            }
            if (this.Z > other.Z)
            {
                return 1;
            }
            return this.Z;
        }

        public static bool operator ==(Vector3i a, Vector3i b)
        {
            if (!(a is null) && !(b is null)) return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
            return a is null && b is null;
            // if(a ==null || b== null) return false;
        }


        public static bool operator !=(Vector3i a, Vector3i b)
        {
            if (!(a is null) && !(b is null)) return a.X != b.X || a.Y != b.Y || a.Z != b.Z;
            return !(a is null) || !(b is null);
        }

     
        public static Vector3i operator -(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }


        public static Vector3i operator +(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }


        public static Vector3i operator -(Vector3i b)
        {
            return new Vector3i(-b.X, -b.Y, -b.Z);
        }


        public static Vector3 operator +(Vector3 a, Vector3i b)
        {
            return new Vector3(a.X + (float)b.X, a.Y + (float)b.Y, a.Z + (float)b.Z);
        }


        public static Vector3 operator +(Vector3i a, Vector3 b)
        {
            return new Vector3((float)a.X + b.X, (float)a.Y + b.Y, (float)a.Z + b.Z);
        }


        public static Vector3 operator -(Vector3i a, Vector3 b)
        {
            return new Vector3((float)a.X - b.X, (float)a.Y - b.Y, (float)a.Z - b.Z);
        }


        public static Vector3 operator -(Vector3 a, Vector3i b)
        {
            return new Vector3(a.X - (float)b.X, a.Y - (float)b.Y, a.Z - (float)b.Z);
        }

        public static Vector3i operator *(int a, Vector3i b)
        {
            return new Vector3i(a * b.X, a * b.Y, a * b.Z);
        }


        public static Vector3i operator %(Vector3i b, int a)
        {
            return new Vector3i(b.X % a, b.Y % a, b.Z % a);
        }


        public static Vector3i operator /(Vector3i a, int i)
        {
            int num = a.X / i;
            int num2 = a.Y / i;
            int num3 = a.Z / i;
            return new Vector3i(num, num2, num3);
        }

        public static int Dot(Vector3i a, Vector3i b)
        {
            return (int)Vector3.Dot(a.ToVector3(), b.ToVector3());
        }

        
 
        public static Vector3i Scale(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }
        
        public static Vector3i Scale(Vector3i a,int n, int d)
        {
            return new Vector3i((a.X*n)/d , (a.Y*n)/d , (a.Z*n)/d );
        }

  
        public static Vector3i Abs(Vector3i a)
        {
            return new Vector3i(Math.Abs(a.X), Math.Abs(a.Y), Math.Abs(a.Z));
        }

        [Key(0)] public int X;


        [Key(1)] public int Y;

    
        [Key(2)] public int Z;

 
        public static readonly Vector3i Zero = new Vector3i(0, 0, 0);

      
        public static readonly Vector3i One = new Vector3i(1, 1, 1);

        // Token: 0x04000094 RID: 148
        public static readonly Vector3i Forward = new Vector3i(0, 0, 1);

        // Token: 0x04000095 RID: 149
        public static readonly Vector3i Back = new Vector3i(0, 0, -1);

        // Token: 0x04000096 RID: 150
        public static readonly Vector3i Up = new Vector3i(0, 1, 0);

        // Token: 0x04000097 RID: 151
        public static readonly Vector3i Down = new Vector3i(0, -1, 0);

        // Token: 0x04000098 RID: 152
        public static readonly Vector3i Left = new Vector3i(-1, 0, 0);

        // Token: 0x04000099 RID: 153
        public static readonly Vector3i Right = new Vector3i(1, 0, 0);

        // Token: 0x0400009A RID: 154
        public static readonly Vector3i BlockOffset = new Vector3i(0, -16, 0);

        // Token: 0x0200002A RID: 42
        [Serializable]
        public class Vector3iComparer : IEqualityComparer<Vector3i>
        {
            // Token: 0x0600011A RID: 282 RVA: 0x00007A97 File Offset: 0x00005C97
            public bool Equals(Vector3i a, Vector3i b)
            {
                return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
            }

            // Token: 0x0600011B RID: 283 RVA: 0x00007AC5 File Offset: 0x00005CC5
            public int GetHashCode(Vector3i obj)
            {
                return obj.X ^ obj.Y << 2 ^ obj.Z >> 2;
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
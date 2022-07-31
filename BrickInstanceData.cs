using MessagePack;
using System;
using System.Numerics;

namespace EvoEditApp
{
    // Token: 0x02000102 RID: 258
    [MessagePackObject(false)]
    public struct BrickInstanceData
    {
        [IgnoreMember]
        public Brick brick => Brick.BRICK_INDEX[(int)this.brickId];

        [IgnoreMember]
        public float healthFactor => (float)this.healthScore / 255f;

        [IgnoreMember]
        public int Material => (int)this.material;

        [IgnoreMember]
        public Vector3 Position => new Vector3((float)this.gridPosition.X / 32f, (float)this.gridPosition.Y / 32f, (float)this.gridPosition.Z / 32f);

        [IgnoreMember]
        public Vector3i Scale => new Vector3i((int)(this.scale & 15), this.scale >> 4 & 15, this.scale >> 8 & 15);

        [IgnoreMember]
        public Vector3 scaleFloat => new Vector3((float)(this.scale & 15), (float)(this.scale >> 4 & 15), (float)(this.scale >> 8 & 15));

        [IgnoreMember]
        public Vector3 scaleFloatCollision => this.brick.CollisionScaleFactor * this.scaleFloat;

        [IgnoreMember]
        public int Quaternion => (int)this.rotation;

        [IgnoreMember]
        public int quaternionInv => (int)this.rotation;

        [IgnoreMember]
        public bool IsMaxHealth => this.healthScore == byte.MaxValue;

        [IgnoreMember]
        public bool IsValid => this.instanceId != 0 && this.brickId > 0;

        [IgnoreMember]
        public float GridSize => (float)Math.Pow(2f, (float)this.gridSize) * 0.125f;

        //return GetGridSize(this.brick, (int)this.gridSize);
        [IgnoreMember]
        public int GridSizeUnit =>
            //return BrickCommon.GetGridSizeUnit((int)this.gridSize);
            (int)(4f * Math.Pow(2f, (float)this.gridSize));

        [IgnoreMember]
        public int UnitGrid => 0;

        //return (int)Math.Round(this.GridSize * (float)this.brick.VoxelMesh.ResizeMode.unitStud);
        [IgnoreMember]
        public int MaximumHealthPoint => 255;

        [IgnoreMember]
        public int CurrentHealthPoint => (int)Math.Ceiling(this.healthFactor * (float)this.MaximumHealthPoint);

        public float GetGridSize(bool isDynamicGridSize)
        {
            if (!isDynamicGridSize)
            {
                return 1f;
            }
            return (float)Math.Pow(2f, (float)this.gridSize) * 0.125f;
        }

        public override bool Equals(object other)
        {
            if (!(other is BrickInstanceData))
            {
                return false;
            }
            BrickInstanceData brickInstanceData = (BrickInstanceData)other;
            return this.instanceId == brickInstanceData.instanceId;
        }

        public bool Equals(BrickInstanceData other)
        {
            return this.instanceId == other.instanceId;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static float GetGridSize(Brick brick, int grid)
        {
            if (!brick.isDynamicGridSize)
            {
                return 1f;
            }
            return (float)Math.Pow(2f, (float)grid) * 0.125f;
        }

        public static bool operator ==(BrickInstanceData a, BrickInstanceData b)
        {
            return a.gridPosition == b.gridPosition & a.brickId == b.brickId & a.rotation == b.rotation & a.scale == b.scale;
        }

        public static bool operator !=(BrickInstanceData a, BrickInstanceData b)
        {
            return a.gridPosition != b.gridPosition | a.brickId != b.brickId | a.rotation != b.rotation | a.scale != b.scale;
        }

        public const byte MAX_HEALTH = 255;

        [Key(0)]
        public ushort brickId;

        [Key(1)]
        public Vector3i gridPosition;

        [Key(2)]
        public ushort scale;

        [Key(3)]
        public byte rotation;

        [Key(4)]
        public object color;

        [Key(5)]
        public uint material;

        [Key(6)]
        public byte healthScore;

        [Key(7)]
        public int instanceId;

        [Key(8)]
        public byte gridSize;


    }
}
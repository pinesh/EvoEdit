using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using EvoEditApp.Properties;

namespace EvoEditApp
{
	// Token: 0x020000FD RID: 253
	public class Brick
	{
		// Token: 0x170000FB RID: 251
		// (get) Token: 0x060008B7 RID: 2231 RVA: 0x000342E6 File Offset: 0x000324E6
		public ushort ItemID => (ushort)this.brickID;

        // Token: 0x170000FC RID: 252
		// (get) Token: 0x060008B8 RID: 2232 RVA: 0x0000DD30 File Offset: 0x0000BF30
		public byte ItemType => 1;

        // Token: 0x170000FE RID: 254
		// (get) Token: 0x060008BA RID: 2234 RVA: 0x0000DD30 File Offset: 0x0000BF30
		public bool ShowEntityInfo => true;

        // Token: 0x17000102 RID: 258
		// (get) Token: 0x060008BE RID: 2238 RVA: 0x00034327 File Offset: 0x00032527
		public bool hasRepeatBrick => this.repeatBrickId > 0 | this.repeatEvenBrickId > 0 | this.repeatOddBrickId > 0;


        private string name;
		private string nameMesh;

		// Token: 0x060008D3 RID: 2259 RVA: 0x00034424 File Offset: 0x00032624
		public Brick(int id, string name, string nameMesh, ResizeMode resizeMode, params object[] args)
		{
			this.name = name;
			this.resizeMode = resizeMode;
			this.args = args;
			this.nameMesh = nameMesh;
			if (Brick.BRICK_INDEX[id] != null)
			{
			}
			Brick.BRICK_INDEX[id] = this;
			this.brickID = id;
		}


		// Token: 0x060008EB RID: 2283 RVA: 0x00034AE2 File Offset: 0x00032CE2
		public virtual Brick GetBrick()
		{
			return this;
		}

		// Token: 0x04000686 RID: 1670
		public const int BRICK_COUNT = 1024;

		// Token: 0x04000687 RID: 1671
		public const float MIN_GRID_SIZE = 0.125f;

		// Token: 0x04000688 RID: 1672
		public const float MIN_GRID_SIZE_UNIT = 4f;

		// Token: 0x04000689 RID: 1673
		public const int MIN_GRID_HALF_SIZE_UNIT = 2;

		// Token: 0x0400068A RID: 1674
		public const int UNIT_GRID = 3;

		// Token: 0x0400068B RID: 1675
		public const int MAX_GRID = 8;

		// Token: 0x0400068C RID: 1676
		public static Brick[] BRICK_INDEX = new Brick[1024];

		// Token: 0x0400068E RID: 1678
		public readonly int brickID;


		// Token: 0x04000692 RID: 1682
		public float weight = -1f;

		// Token: 0x04000693 RID: 1683
		public int baseWeightUnit = 1;

		// Token: 0x040006A0 RID: 1696
		public bool useMaterial;

		// Token: 0x040006B2 RID: 1714
		public ushort repeatBrickId;

		// Token: 0x040006B3 RID: 1715
		public ushort repeatEvenBrickId;

		// Token: 0x040006B4 RID: 1716
		public ushort repeatOddBrickId;

		// Token: 0x040006B5 RID: 1717
		public bool isRepeatBrick;



		// Token: 0x040006B7 RID: 1719
		public bool isDynamicGridSize;

		// Token: 0x040006B8 RID: 1720
		public int gridSize = 1;


		// Token: 0x040006BA RID: 1722
		public int minGridSize;

		// Token: 0x040006BB RID: 1723
		public int maxGridSize = 8;

		// Token: 0x040006BC RID: 1724
		public int placementSubGrid;


		// Token: 0x040006BE RID: 1726
		public float CollisionWidth = 1f;

		// Token: 0x040006BF RID: 1727
		public float CollisionScaleFactor = 1f;


		// Token: 0x040006C8 RID: 1736
		private readonly ResizeMode resizeMode;

		// Token: 0x040006C9 RID: 1737
		private readonly object[] args;
	}
}
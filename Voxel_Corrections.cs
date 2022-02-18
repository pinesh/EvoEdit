using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoEditApp
{
    internal static class VoxelCorrections
    {
        /// <summary>
        /// This class permits the comparison of two vectors to determine who is lower in the entity.
        /// </summary>
        internal class Vector3IComparer : IComparer<Vector3i>
        {
            public int Compare(Vector3i x, Vector3i y)
            {
                if (object.ReferenceEquals(x, y))
                {
                    return 0;
                }

                if (x == null)
                {
                    return -1;
                }

                if (y == null)
                {
                    return 1;
                }

                //easy lower y is always smaller
                if (x.Y < y.Y)
                {
                    return -1;
                }

                if (x.Y > y.Y)
                {
                    return 1;
                }
                //order on same y plane

                if (x.Z < y.Z)
                {
                    return -1;
                }

                if (x.Z > y.Z)
                {
                    return 1;
                }

                //we want least x first
                if (x.X < y.X)
                {
                    return -1;
                }

                return 1;

            }
        }

        /// <summary>
        /// This class permits the comparison of two vectors to determine who is lower major entity.
        /// </summary>
        internal class Vector3IMajorComparer : IComparer<Vector3i>
        {
            public int Compare(Vector3i x, Vector3i y)
            {
                if (object.ReferenceEquals(x, y))
                {
                    return 0;
                }

                if (x == null)
                {
                    return -1;
                }

                if (y == null)
                {
                    return 1;
                }

                //easy lower y is always smaller
                if (x.Y < y.Y)
                {
                    return -1;
                }

                if (x.Y > y.Y)
                {
                    return 1;
                }
                //order on same y plane

                if (x.Z < y.Z)
                {
                    return -1;
                }

                if (x.Z > y.Z)
                {
                    return 1;
                }

                //we want most x first
                if (x.X < y.X)
                {
                    return 1;
                }

                return -1;

            }
        }

        public static Tuple<ushort, Vector3i> get_scale_mirror(Vector3i start, Vector3i end, int rot,int Scale)
        {
            DeconstructVector(start, end,Scale).Deconstruct(out int x, out int y, out int z);

            //we need to shunt left relative.
            switch (rot)
            {
                case 2:
                    return new Tuple<ushort, Vector3i>(range_to_scale(x, y, z), new Vector3i(0, 0, 4 * z));
                default:
                    return new Tuple<ushort, Vector3i>(range_to_scale(x, y, z), new Vector3i(0, 0, 0));
            }

        }

        /// <summary>
        /// Calculates the scale and required offset data for a wedge
        /// </summary>
        /// <param name="start">start pos</param>
        /// <param name="end">end pos</param>
        /// <param name="rot">rotation</param>
        /// <returns>The offset and scale as a vector</returns>
        public static Tuple<ushort, Vector3i> get_scale_wedge(Vector3i start, Vector3i end, int rot,int Scale)
        {
            DeconstructVector(start, end, Scale).Deconstruct(out int x, out int y, out int z);
            int offset = Math.Max(x, Math.Max(y, z)) * 4;
            switch (rot)
            {
                case 0:
                    //we're fine, no shunt required.
                    return new Tuple<ushort, Vector3i>(range_to_scale(x, y, z), new Vector3i(0, 0, 0));
                case 1:
                    //we're fine, no shunt required
                    return new Tuple<ushort, Vector3i>(range_to_scale(z, y, x), new Vector3i(0, 0, 0));
                case 2:
                    //we need to shunt left relative.
                    return new Tuple<ushort, Vector3i>(range_to_scale(x, y, z), new Vector3i(0, 0, offset));
                case 3:
                    //we need to shunt left relative.
                    return new Tuple<ushort, Vector3i>(range_to_scale(z, y, x), new Vector3i(offset, 0, 0));
                case 4:
                    //we need to shunt right relative. //bottom (red)
                    return new Tuple<ushort, Vector3i>(range_to_scale(x, y, z), new Vector3i(0, 0, offset));
                case 5:
                    //we're fine, no need to shunt,
                    return new Tuple<ushort, Vector3i>(range_to_scale(z, y, x), new Vector3i(0, 0, 0));
                case 6:
                    //we're fine, no shunt required.
                    return new Tuple<ushort, Vector3i>(range_to_scale(x, y, z), new Vector3i(0, 0, 0));
                case 7:
                    //we need to shunt right relative.
                    return new Tuple<ushort, Vector3i>(range_to_scale(z, y, x), new Vector3i(offset, 0, 0));
                case 21:
                    //we're fine, no shunt required.
                    return new Tuple<ushort, Vector3i>(range_to_scale(z, x, y), new Vector3i(0, 0, 0));
                case 23:
                    //we need to move it up however long the scale is.
                    return new Tuple<ushort, Vector3i>(range_to_scale(z, x, y), new Vector3i(0, offset, 0));
                case 17:
                    //needs to go up however long scale is
                    return new Tuple<ushort, Vector3i>(range_to_scale(z, x, y), new Vector3i(0, offset, 0));
                case 19:
                    //we're fine, no shunt required.
                    return new Tuple<ushort, Vector3i>(range_to_scale(z, x, y), new Vector3i(0, 0, 0));
            }

            //case y is large, scale should be x or z depending on rotation.
            return new Tuple<ushort, Vector3i>(range_to_scale(z, y, x), new Vector3i(0, 0, 0));
        }

        /// <summary>
        /// Deconstructs and normalizes a vector distance.
        /// </summary>
        /// <param name="start">start pos</param>
        /// <param name="end"> end pos</param>
        /// <returns>A tuple containing the difference / scale in order x y z</returns>
        internal static Tuple<int, int, int> DeconstructVector(Vector3i start, Vector3i end, int Scale)
        {
            return new Tuple<int, int, int>(item1: Math.Abs(start.X - end.X) / Scale,
                item2: Math.Abs(start.Y - end.Y) / Scale, item3: Math.Abs(start.Z - end.Z) / Scale);

        }

        /// <summary>
        /// Calculates the scale and required offset data for a slab
        /// </summary>
        /// <param name="start">start pos</param>
        /// <param name="end">end pos</param>
        /// <param name="rot">rotation</param>
        /// <returns>The offset and scale as a vector</returns>
        public static Tuple<ushort, Vector3i> get_scale_slab(Vector3i start, Vector3i end, int rot, int Scale)
        {
            DeconstructVector(start, end,Scale).Deconstruct(out int x, out int y, out int z);
            switch (rot)
            {
                case 8: //side slab needs to go up 1. when shorter side (z? //SIDE CORRECT
                    return new Tuple<ushort, Vector3i>(range_to_scale(x, z, y), new Vector3i(0, 4 * y, 0)); //DONE
                case 4: //BOTTOM needs to go forwards
                    return new Tuple<ushort, Vector3i>(range_to_scale(x, y, z),
                        new Vector3i(0, 0, 4 * z)); //check again
                case 16: //back needs to go up
                    return new Tuple<ushort, Vector3i>(range_to_scale(y, x, y), new Vector3i(0, 4 * y, 0));
                case 0: //TOP CORRECT
                    return new Tuple<ushort, Vector3i>(range_to_scale(x, y, z), new Vector3i(0, 0, 0)); //done
                case 12: //SIDE CORRECT
                    return new Tuple<ushort, Vector3i>(range_to_scale(x, z, y), new Vector3i(0, 0, 0)); //done
                case 22:
                    return new Tuple<ushort, Vector3i>(range_to_scale(y, x, z), new Vector3i(0, 4 * y, 4 * z));
                    //  case 23:
                    //return new Tuple<ushort, Vector3i>(range_to_scale(y, x, z), new Vector3i(0, 0, 0));
            }

            //case y is large, scale should be x or z depending on rotation.
            return new Tuple<ushort, Vector3i>(range_to_scale(x, y, z), new Vector3i(0, 0, 0));
        }

        /// <summary>
        /// Returns the offset position for an additional 1/4 slab to form over a 1/2
        /// </summary>
        /// <param name="start">start pos</param>
        /// <param name="end">end pos</param>
        /// <param name="rot">rotation</param>
        /// <returns>The offset as a vector</returns>
        public static Vector3i get_scale_thiccslab(Vector3i start, Vector3i end, int rot, int Scale)
        {
            DeconstructVector(start, end,Scale).Deconstruct(out int x, out int y, out int z);
            switch (rot) //when this is 1, we want to add 2
            {
                case 0: //TOP CORRECT
                    return new Vector3i(0, 2, 0); //done
                case 4: //BOTTOM needs to go forwards
                    return new Vector3i(0, -2, 4 * z); //check again
                case 8: //side slab needs to go up 1. when shorter side (z? //SIDE CORRECT
                    return new Vector3i(0, 4 * y, 2); //DONE //not right
                case 12: //SIDE CORRECT
                    return new Vector3i(0, 0, -2); //done // not right
                case 16: //back needs to go up
                    return new Vector3i(2, 4 * y, 0);
                case 22:
                    return new Vector3i(-2, 4 * y, 4 * z);
            }
            return new Vector3i(0, 0, 0);
        }

        /// <summary>
        /// Convert a vector distance to equiv Starship Evo Scale Bit.
        /// </summary>
        /// <param name="start">Star pos</param>
        /// <param name="end">End pos</param>
        /// <returns>The scale as a packed ushort</returns>
        public static ushort get_scale(Vector3i start, Vector3i end,int Scale)
        {
            return range_to_scale(Math.Abs(start.X - end.X) / Scale, Math.Abs(start.Y - end.Y) / Scale,
                Math.Abs(start.Z - end.Z) / Scale);
        }

        /// <summary>
        /// bit packs a vector
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="z">z</param>
        /// <returns>bit packed ushort</returns>
        internal static ushort range_to_scale(int x, int y, int z)
        {
            var u = (ushort)z;
            u = (ushort)(u << (ushort)4 | (ushort)y);
            u = (ushort)(u << (ushort)4 | (ushort)x);
            return u;
        }

        /// <summary>
        /// This class corrects the import grid to align with the expected Stevo grid
        /// </summary>
        internal static class BlockOffsets
        {
            private static readonly Dictionary<byte, Vector3i> ScaleOffsets = new Dictionary<byte, Vector3i>
            {
                { 0, new Vector3i(2, 0, 2) },
                { 1, new Vector3i(4, 0, 4) },
                { 2, new Vector3i(8, 0, 8) },
                { 3, new Vector3i(0, 16, 0) },//1m
                { 4, new Vector3i(16, -16, 16) },//2m //8 = 0.375m
                { 5, new Vector3i(48, -16, 48) },//4m
                { 6, new Vector3i(112,-16, 112) },//8m
                { 7, new Vector3i(240, -16, 240) },//16m
                { 8, new Vector3i(496, -16, 496) } //32m
            };

            private static readonly Dictionary<byte, Vector3i> Offsets = new Dictionary<byte, Vector3i>
            {
                {0,new Vector3i(0,0,0)},
                {1,new Vector3i(0,0,0)},
                {2,new Vector3i(0,0,0)},
                {3,new Vector3i(0,0,0)},

                {4,new Vector3i(0,4,0)},
                {5,new Vector3i(0,4,0)},
                {6,new Vector3i(0,4,0)},
                {7,new Vector3i(0,4,0)},

                {8,new Vector3i(0,2,-2)},
                {9,new Vector3i(0,2,-2)},
                {10,new Vector3i(0,2,-2)},// {10,new Vector3i(0,8,-8)},
                {11,new Vector3i(0,2,-2)},// {11,new Vector3i(0,8,-8)},

                {12,new Vector3i(0,2,2)},
                {13,new Vector3i(0,2,2)},
                {14,new Vector3i(0,2,2)},
                {15,new Vector3i(0,2,2)},

                {16,new Vector3i(-2,2,0)},//{2,new Vector3i(-8,8,0)},
                {17,new Vector3i(-2,2,0)},// {17,new Vector3i(-8,8,0)},
                {18,new Vector3i(-2,2,0)},
                {19,new Vector3i(-2,2,0)},//  {19,new Vector3i(-8,8,0)},

                {20,new Vector3i(2,2,0)},
                {21,new Vector3i(2,2,0)},//  {21,new Vector3i(8,8,0)},
                {22,new Vector3i(2,2,0)},// {22,new Vector3i(8,8,0)},
                {23,new Vector3i(2,2,0)},
            };

            public static Vector3i GetScaleOffsets(byte key)
            {
                try
                {
                    return ScaleOffsets[key];
                }
                catch (Exception e)
                {
                    Console.WriteLine(key);
                    return new Vector3i(0, 0, 0);
                }

            }

            public static Vector3i GetOffsets(byte key)
            {

                return Offsets[key];
            }
        }
    }
}

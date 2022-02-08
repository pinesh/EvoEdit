using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Akavache;
using testapp1;

namespace EvoEditApp
{
    /// <summary>
    /// The purpose of this set of classes will be to convert voxel data, optimize it and parse it.
    ///
    /// The goal will be to take a dictionary of blockbits, and export a list of brickinstancedatas
    /// </summary>
    class Voxel_Import
    {
        public Dictionary<Vector3i, BlockBit> Master;
        public Dictionary<Vector3i, byte> MasterDif;
        public List<MasterBlock> Mblist;
        private SortedDictionary<Vector3i, BlockBit> _ok;
        public Vector3i Min;
        private bool _ignorePaintFlag;
        public int Scale;
        public int grid_scale;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String info)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(info));
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Return a new instance of brick data
        /// </summary>
        /// <param name="sc">the stretch dimensions of the block as a bitpacked ushort</param>
        /// <param name="paint">the paint code for the target block</param>
        /// <param name="rot">the Stevo-Rotation</param>
        /// <param name="pos">the vector position</param>
        /// <param name="id">the block id</param>
        /// <param name="i">instance pointer</param>
        /// <returns>a new BrickInstance</returns>
        internal BrickInstanceData GetBrick(ushort sc,Vector3i paint,byte rot,Vector3i pos,ushort id,int i)
        {
            return new BrickInstanceData()
            {
                scale = sc,
                rotation = rot,
                gridPosition = pos,
                brickId = id,
                color = (object)new object[] { paint.X, paint.Y, paint.Z, 255 },
                material = 0,
                healthScore = 255,
                gridSize = (byte)grid_scale,
                instanceId = i
            };
        }

    

        public List<BrickInstanceData> ExportBricks(int i)
        {
            int multiplier = (int)Math.Pow(2, grid_scale);
            List<BrickInstanceData> d = new List<BrickInstanceData>();
            foreach (var newBlock in Mblist)
            {

                Vector3i additional = new Vector3i(0, 0, 0);
                Vector3i paint = BlockTypes.Sevo_Paint((short)newBlock.BaseType);
                byte rot = (byte)(sbyte)newBlock.Rot;
                var s = (ushort)BlockTypes.Sevo_ID((short)newBlock.BaseType);
                //if (s == 244) rot = 0;
                var sc = get_scale(newBlock.Startpos, newBlock.Endpos);

                switch (s)
                {
                    case 196://Hull
                    case 166://Glass Block
                        rot = 0;
                        break;
                    case 168://Glass Wedge
                    case 197://Hull Wedge
                        get_scale_wedge(newBlock.Startpos, newBlock.Endpos, rot)
                            .Deconstruct(out sc, out additional);
                        break;
                    case 244://Slab 1/2
                    case 243://Slab 1/4
                        get_scale_slab(newBlock.Startpos, newBlock.Endpos, rot)
                            .Deconstruct(out sc, out additional);
                        break;
                    case 245://Special Slab 3/4
                        get_scale_slab(newBlock.Startpos, newBlock.Endpos, rot)
                            .Deconstruct(out sc, out additional);
                        var extra = get_scale_thiccslab(newBlock.Startpos, newBlock.Endpos, rot);//this slab will have the same scalar as the parent.
                        s = 243;
                        d.Add(GetBrick(sc,paint,rot,  newBlock.Startpos + multiplier * BlockOffsets.GetOffsets(rot) +
                                                     multiplier * extra + BlockOffsets.GetScaleOffsets((byte)grid_scale), 244,i++));
                        break;
                }
                d.Add(GetBrick(sc,paint,rot, newBlock.Startpos + multiplier * BlockOffsets.GetOffsets(rot) + multiplier * additional + BlockOffsets.GetScaleOffsets((byte)grid_scale),s,i++));
            }
            return d;
        }

        public bool useBlob = false;
        public Voxel_Import(Dictionary<Vector3i, BlockBit> l, Vector3i u, int s = 1, bool paint = false,int capS =1)
        {
            //_ok = new SortedDictionary<Vector3i, BlockBit>(new Vector3IComparer());
            Master = l;
            MasterDif = new Dictionary<Vector3i, byte>(Master.Count*10);
           // BlobCache.InMemory.Invalidate(string key)
            this.Min = u;
            this.grid_scale = s;
            this.Scale = (int)(4 * Math.Pow(2, s));
            Mblist = new List<MasterBlock>();
            _ignorePaintFlag = paint;
            Console.WriteLine(Master.Count);
            Console.WriteLine($"min = [{Min.X},{Min.Y},{Min.Z}]");
           
        }
    

        /// <summary>
        /// Calculates the scale and required offset data for a wedge
        /// </summary>
        /// <param name="start">start pos</param>
        /// <param name="end">end pos</param>
        /// <param name="rot">rotation</param>
        /// <returns>The offset and scale as a vector</returns>
        internal Tuple<ushort, Vector3i> get_scale_wedge(Vector3i start, Vector3i end, int rot)
        {
            DeconstructVector(start, end).Deconstruct(out int x, out int y, out int z);
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
        internal Tuple<int, int, int> DeconstructVector(Vector3i start, Vector3i end)
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
        internal Tuple<ushort, Vector3i> get_scale_slab(Vector3i start, Vector3i end, int rot)
        {
            DeconstructVector(start,end).Deconstruct(out int x,out int y, out int z);
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
        internal Vector3i get_scale_thiccslab(Vector3i start, Vector3i end, int rot)
        {
            DeconstructVector(start, end).Deconstruct(out int x, out int y, out int z);
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
        internal ushort get_scale(Vector3i start, Vector3i end)
        {
            return range_to_scale(Math.Abs(start.X - end.X)/Scale, Math.Abs(start.Y - end.Y) / Scale,
                Math.Abs(start.Z - end.Z) / Scale);
        }

        /// <summary>
        /// bit packs a vector
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="z">z</param>
        /// <returns>bit packed ushort</returns>
        internal ushort range_to_scale(int x, int y, int z)
        {
            var u = (ushort)z;
            u = (ushort)(u << (ushort)4 | (ushort)y);
            u = (ushort)(u << (ushort)4 | (ushort)x);
            return u;
        }

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


        public void Optimize(bool flag = true)
        {
            Mblist = new List<MasterBlock>();
            if (flag)
            {
                this.Merge();
                Console.WriteLine($"Reduced to {Mblist.Count} Blocks");
                return;
            }
        
            foreach (var keyPair in Master)
            {
                var type = keyPair.Value.get_id();
                var r = (byte)keyPair.Value.get_Sevo_rot();
                Mblist.Add(GetMasterBlock(keyPair.Key,keyPair.Key,type,r));
            }

        }

        internal void Merge()
        {
            _ok = new SortedDictionary<Vector3i, BlockBit>(new Vector3IComparer());
            foreach (var keyPair in Master)
            {
                //Console.WriteLine($"block type {keypair.Value.get_id()}, block axis {keypair.Value.get_axis_rotation()}, block rotation {keypair.Value.get_rotations()}");
                _ok.Add(keyPair.Key, keyPair.Value);
            }
            int og = _ok.Count;
            var cur = 0;
            int i = 0;
            foreach (var key in _ok.Keys.ToList())
            {
                cur += 1;

                double m = (float)(MasterDif.Count/ (float)og) * (float)100;
                if (m>i)
                {
                    i++;
                    NotifyPropertyChanged($"{i}");
                }

                if (MasterDif.ContainsKey(key)) continue;
                findOne(key);
            }
            NotifyPropertyChanged($"{100}");
        }


        /// <summary>
        /// GREEDY MERGER, from the bottom corner, greedily take every block reachable and return the number of blocks grabbed
        /// </summary>
        /// <param name="start">start search position</param>
        /// <param name="end">end search position</param>
        /// <param name="type">type of origin block</param>
        /// <param name="r"> rotation of origin block</param>
        /// <returns></returns>
        internal List<Vector3i> GetAllPoints(Vector3i start, Vector3i end, int type, int r)
        {
            var points = new List<Vector3i>();
            for (var yIndex = start.Y; yIndex <= end.Y; yIndex ++)
            {
                //z
                for (var zIndex = start.Z; zIndex <= end.Z; zIndex ++)
                {
                    //x
                    for (var xIndex = start.X; xIndex <= end.X; xIndex ++)
                    {
                        var v = new Vector3i(xIndex, yIndex, zIndex);
                        if (!Comp(v, type, r)) return points;
                        points.Add(v);
                    }
                }
            }

            return points;
        }



        internal Tuple<List<Vector3i>,int> IterateStretch(int c, Vector3i start,Vector3i end, List<Vector3i> mastlist,int type,byte r,int m,int dirX = 0,int dirY = 0,int dirZ = 0)
        {
            while (c < 16)
            {
                var lv = GetAllPoints(new Vector3i(start.X + (c * dirX), start.Y + (c * dirY), start.Z + (c *dirZ)), new Vector3i(end.X + (c * dirX), end.Y+ (c * dirY), end.Z+ (c * dirZ)), type, r:r);
                if (mastlist.Count + lv.Count == (c+1)*m)
                {
                    mastlist.AddRange(lv);
                }
                else
                {
                    break;
                }
                c++;
            }
            return new Tuple<List<Vector3i>, int>(mastlist,c-1);
        }

        internal void findOne(Vector3i start)
        {
            var b = Master[start];
            //all blocks in merge must have same rotation and blockid.
            //to handle wedges and corners, we only want a singular merge direction, if we get an list bigger than one, we return.
            var type = b.get_id();
            var style = (ushort)BlockTypes.Sevo_ID((short)type);
            var r = (byte)b.get_Sevo_rot();

            if (style >= 198 && style <= 200) //don't merge tetra or hepta 198-200
            {
                Mblist.Add(GetMasterBlock(start,start,type,r));
                if (!MasterDif.ContainsKey(start))
                    MasterDif.Add(start, 0);
                //Master.Remove(start);
                return;
            }

            List<Vector3i> mastlist = new List<Vector3i>();
            //loop through every key.
            //y
            IterateStretch(0, start,start, mastlist, type, r,m:1, dirX: 1).Deconstruct(out mastlist,out _);
            
            var xcount = mastlist.Count - 1; //take off self.

            //we have wedges done.
            if ((style == 197 || style == 168) && xcount != 0)
            {
                SwapBlock(mastlist,start,type,r,xcount);
                return;
            }
            IterateStretch(1, start, new Vector3i(start.X+(xcount),start.Y,start.Z), mastlist, type, r, m: (xcount + 1), dirZ: 1).Deconstruct(out mastlist,out int zcount);

            //we have wedges done.
            if ((style == 197 || style == 168) && zcount != 0)
            {
                SwapBlock(mastlist,start,type,r,xcount,0,zcount);
                return;
            }

            if (style >= 243 && style <= 245 && zcount != 0 && xcount != 0) //slabs can only have two stretches
            {
                SwapBlock(mastlist,start,type,r,xcount,0,zcount);
                return;
            }

            IterateStretch(1,start, new Vector3i(start.X + (xcount), start.Y, start.Z + (zcount)),mastlist,type,r,m: (xcount + 1) * (zcount + 1), dirY:1).Deconstruct(out mastlist, out int ycount);
            //every point in this list becomes an mblock. 
            SwapBlock(mastlist, start, type, r, xcount, ycount, zcount);
            return;
        }
        internal void SwapBlock(List<Vector3i> mastlist,Vector3i start,int type, byte r, int offX = 0, int offY = 0, int offZ = 0)
        {
            Mblist.Add(GetMasterBlock(start, new Vector3i((start.X + offX),  (start.Y + ( offY)), (start.Z + (offZ))), type, r));
            foreach (var v in mastlist)
            {
                if(!MasterDif.ContainsKey(v))
                    MasterDif.Add(v, 0);
                //Master.Remove(v);
            }
        }

        internal MasterBlock GetMasterBlock(Vector3i start,Vector3i end, int type, byte rot)
        {
            if (!_ignorePaintFlag)
                return new MasterBlock()
                {
                    Startpos = Scale*start,
                    Endpos = Scale * end,
                    Type = type,
                    BaseType = type,
                    Rot = rot
                    
                };

            return new MasterBlock()
            {
                Startpos = Scale * start,
                Endpos = Scale * end,
                Type = type,
                BaseType = BlockTypes.GetDefault((short)type),
                Rot = rot
            };
        }

        internal bool Comp(Vector3i c, int type, int rot)
        {
            if(!Master.ContainsKey(c)) return false;
            if (MasterDif.ContainsKey(c)) return false;
            if (_ignorePaintFlag)
                return Master[c].get_Sevo_rot() == rot &&
                       BlockTypes.GetDefault((short)Master[c].get_id()) == BlockTypes.GetDefault((short)type);
            return Master[c].get_Sevo_rot() == rot && Master[c].get_id() == type;
        }

        internal struct MasterBlock
        {
            public Vector3i Startpos;
            public Vector3i Endpos;
            public int BaseType;
            public int Type;
            public byte Rot;
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
                catch(Exception e)
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

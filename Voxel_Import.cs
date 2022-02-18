using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Documents;
using ICSharpCode.SharpZipLib.Zip;
using testapp1;
using Xceed.Wpf.AvalonDock.Converters;

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
                var sc = VoxelCorrections.get_scale(newBlock.Startpos, newBlock.Endpos,Scale);

                switch (s)
                {
                    case 196://Hull
                    case 166://Glass Block
                        VoxelCorrections.get_scale_mirror(newBlock.Startpos, newBlock.Endpos, rot,Scale).Deconstruct(out sc, out additional);
                        //Console.WriteLine(rot);
                        break;
                    case 168://Glass Wedge
                    case 197://Hull Wedge
                        VoxelCorrections.get_scale_wedge(newBlock.Startpos, newBlock.Endpos, rot,Scale)
                            .Deconstruct(out sc, out additional);
                        break;
                    case 244://Slab 1/2
                    case 243://Slab 1/4
                        VoxelCorrections.get_scale_slab(newBlock.Startpos, newBlock.Endpos, rot,Scale)
                            .Deconstruct(out sc, out additional);
                        break;
                    case 245://Special Slab 3/4
                        VoxelCorrections.get_scale_slab(newBlock.Startpos, newBlock.Endpos, rot, Scale)
                            .Deconstruct(out sc, out additional);
                        var extra = VoxelCorrections.get_scale_thiccslab(newBlock.Startpos, newBlock.Endpos, rot, Scale);//this slab will have the same scalar as the parent.
                        s = 243;
                        d.Add(GetBrick(sc,paint,rot,  newBlock.Startpos + multiplier * VoxelCorrections.BlockOffsets.GetOffsets(rot) +
                                                     multiplier * extra + VoxelCorrections.BlockOffsets.GetScaleOffsets((byte)grid_scale), 244,i++));
                        break;
                }
                //d.Add(GetBrick(sc,paint,rot, newBlock.Startpos + multiplier * BlockOffsets.GetOffsets(rot) + multiplier * additional + BlockOffsets.GetScaleOffsets((byte)grid_scale),s,i++));
                d.Add(GetBrick(sc, paint, rot, newBlock.Startpos + multiplier * VoxelCorrections.BlockOffsets.GetOffsets(rot) + multiplier * additional + VoxelCorrections.BlockOffsets.GetScaleOffsets((byte)grid_scale), s, i++));
            }
            return d;
        }

        public bool useBlob = false;
        private bool fast = true;
        private bool _axis = false;
        public Voxel_Import(Dictionary<Vector3i, BlockBit> l, Vector3i u, int s = 1, bool paint = false,int capS =1,bool fast = true,bool ax = false)
        {
            //_ok = new SortedDictionary<Vector3i, BlockBit>(new Vector3IComparer());
            this.fast = fast;
            Master = l;
            if (!fast)
                MasterDif = new Dictionary<Vector3i, byte>(Master.Count * 10);
            this._axis = ax;
            // BlobCache.InMemory.Invalidate(string key)
            this.Min = u;
            this.grid_scale = s;
            this.Scale = (int)(4 * Math.Pow(2, s));
            Mblist = new List<MasterBlock>();
            _ignorePaintFlag = paint;
            Console.WriteLine(Master.Count);
            Console.WriteLine($"min = [{Min.X},{Min.Y},{Min.Z}]");
           
        }


      
        public void Optimize(int axis = 0)
        {
            Mblist = new List<MasterBlock>();
            if(_axis)
                this.MergeWithSym(axis);
            else
                this.Merge();
            Console.WriteLine($"Reduced to {Mblist.Count} Blocks");
        }


        /// <summary>
        /// Experimental, we want to determine the block width of our data, this might be tricky.
        /// </summary>
        internal void MergeWithSym(int axis)
        {
            int minAxis;
            int maxAxis = minAxis = Min[axis];

            var _orderedKeys = new SortedDictionary<Vector3i, BlockBit>(new VoxelCorrections.Vector3IComparer());
            foreach (var keyPair in Master)
            {
                if (keyPair.Key[axis] > maxAxis)
                    maxAxis = keyPair.Key[axis];
                _orderedKeys.Add(keyPair.Key, keyPair.Value);
            }
            int og = _orderedKeys.Count;
            Console.WriteLine($"Max = {maxAxis}");
            double median = 0;

            //if odd brick, we need to break into 3 parts.
          
            //we break into two parts, all keys on each side of the median.
            median = minAxis + (double)(maxAxis - minAxis) / 2;

            //_ordered keys contains all keys at this point, we want to split with less than greater than median.

            //middle and minor remain the same approach, we only flip major. 
          
            var minor = new SortedDictionary<Vector3i, BlockBit>(new VoxelCorrections.Vector3IComparer());
            var major = new SortedDictionary<Vector3i, BlockBit>(new VoxelCorrections.Vector3IMajorComparer());
            var middle = new SortedDictionary<Vector3i, BlockBit>(new VoxelCorrections.Vector3IComparer());
            foreach (var keyPair in _orderedKeys)
            {
               if (keyPair.Key[axis] > median && keyPair.Value.GetSevoId() == 196)
               {
                //   var orderedKey = _orderedKeys[keyPair.Key];
                 //  orderedKey.invert = true;
                   major.Add(keyPair.Key, keyPair.Value);
               }
               else if (keyPair.Key[axis] < median)
               {
                   minor.Add(keyPair.Key, keyPair.Value);
               }
               else
               {
                   middle.Add(keyPair.Key, keyPair.Value);
               }
            }

            int cur = 0;
            int i = 0;
            //merge the middle.
           foreach (var key in middle.Keys.ToList())
           {
               cur += 1;
               double m = (float)((og - Master.Count) / (float)og) * (float)100;
               if (m > i)
               {
                   i++;
                   NotifyPropertyChanged($"{i}");
               }
               if (!Master.ContainsKey(key)) continue;
               findOne(key);
           }

           NotifyPropertyChanged($"{100}");
           //merge the minor (old behavior) 
           foreach (var key in minor.Keys.ToList())
           {
               cur += 1;
               double m = (float)((og - Master.Count) / (float)og) * (float)100;
               if (m > i)
               {
                   i++;
                   NotifyPropertyChanged($"{i}");
               }

               if (!Master.ContainsKey(key)) continue;
               findOne(key);
           }
           //Console.WriteLine($"{major.Keys.ToList()[0].X},{major.Keys.ToList()[0].Y},{major.Keys.ToList()[0].Z}");
          
           foreach (var keyPair in major)
           {
               cur += 1;
               double m = (float)((og - Master.Count) / (float)og) * (float)100;
               if (m > i)
               {
                   i++;
                   NotifyPropertyChanged($"{i}");
               }

               if (!Master.ContainsKey(keyPair.Key)) continue;
               findOneInvert(keyPair.Key, axis, keyPair.Value);
           }
        }
        internal void Merge()
        {
            _ok = new SortedDictionary<Vector3i, BlockBit>(new VoxelCorrections.Vector3IComparer());
            foreach (var keyPair in Master)
            {
                //Console.WriteLine($"block type {keypair.Value.get_id()}, block axis {keypair.Value.get_axis_rotation()}, block rotation {keypair.Value.get_rotations()}");
                _ok.Add(keyPair.Key, keyPair.Value);
            }
            int og = _ok.Count;
            var cur = 0;
            int i = 0;
            if (fast)
            {
                foreach (var key in _ok.Keys.ToList())
                {
                    cur += 1;

                    double m = (float)((og-Master.Count) / (float)og) * (float)100;
                    if (m > i)
                    {
                        i++;
                        NotifyPropertyChanged($"{i}");
                    }
                    if (!Master.ContainsKey(key)) continue;
                    findOne(key);
                }
                NotifyPropertyChanged($"{100}");
            }
            else
            {
                foreach (var key in _ok.Keys.ToList())
                {
                    cur += 1;

                    double m = (float)(MasterDif.Count / (float)og) * (float)100;
                    if (m > i)
                    {
                        i++;
                        NotifyPropertyChanged($"{i}");
                    }
                    if (MasterDif.ContainsKey(key)) continue;
                    findOne(key);
                }
                NotifyPropertyChanged($"{100}");
            }
           
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

        internal List<Vector3i> GetAllPointsInverted(Vector3i start, Vector3i end, int type, int r,int axis)
        {//whatever axis is, is the way we wish to traverse
            var points = new List<Vector3i>();

            switch (axis)
            {
                case 0://x axis
                    for (var yIndex = start.Y; yIndex <= end.Y; yIndex++)
                    {
                        //z
                        for (var zIndex = start.Z; zIndex <= end.Z; zIndex++)
                        {
                            //x
                            for (var xIndex = start.X; xIndex >= end.X; xIndex--)
                            {
                                var v = new Vector3i(xIndex, yIndex, zIndex);
                                if (!Comp(v, type, r)) return points;
                                points.Add(v);
                            }
                        }
                    }
                    break;
                case 1://y axis
                    for (var yIndex = start.Y; yIndex >= end.Y; yIndex--)
                    {
                        //z
                        for (var zIndex = start.Z; zIndex <= end.Z; zIndex++)
                        {
                            //x
                            for (var xIndex = start.X; xIndex <= end.X; xIndex++)
                            {
                                var v = new Vector3i(xIndex, yIndex, zIndex);
                                if (!Comp(v, type, r)) return points;
                                points.Add(v);
                            }
                        }
                    }
                    break;
                default://z axis
                    for (var yIndex = start.Y; yIndex <= end.Y; yIndex++)
                    {
                        //z
                        for (var zIndex = start.Z; zIndex >= end.Z; zIndex--)
                        {
                            //x
                            for (var xIndex = start.X; xIndex <= end.X; xIndex++)
                            {
                                var v = new Vector3i(xIndex, yIndex, zIndex);
                                if (!Comp(v, type, r)) return points;
                                points.Add(v);
                            }
                        }
                    }
                    break;
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

        internal Tuple<List<Vector3i>, int> IterateStretchInvert(int c, Vector3i start, Vector3i end, List<Vector3i> mastlist, int type, byte r, int m,int axis, int dirX = 0, int dirY = 0, int dirZ = 0)
        {
            while (c < 16)
            {
                List<Vector3i> lv;
                switch (axis)
                {
                    case 0:
                         lv = GetAllPointsInverted(new Vector3i(start.X - (c * dirX), start.Y + (c * dirY), start.Z + (c * dirZ)), new Vector3i(end.X - (c * dirX), end.Y + (c * dirY), end.Z + (c * dirZ)), type, r: r, axis: axis);
                        break;
                    case 1:
                         lv = GetAllPointsInverted(new Vector3i(start.X + (c * dirX), start.Y - (c * dirY), start.Z + (c * dirZ)), new Vector3i(end.X + (c * dirX), end.Y - (c * dirY), end.Z + (c * dirZ)), type, r: r, axis: axis);
                        break;
                    default:
                         lv = GetAllPointsInverted(new Vector3i(start.X + (c * dirX), start.Y + (c * dirY), start.Z - (c * dirZ)), new Vector3i(end.X + (c * dirX), end.Y + (c * dirY), end.Z - (c * dirZ)), type, r: r, axis: axis);
                        break;
                }
                if (mastlist.Count + lv.Count == (c + 1) * m)
                {
                    mastlist.AddRange(lv);
                }
                else
                {
                    break;
                }
                c++;
            }
            return new Tuple<List<Vector3i>, int>(mastlist, c - 1);
        }
        internal void findOne(Vector3i start,bool inverse = false)
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
                if (fast)
                {
                    if (Master.ContainsKey(start))
                        Master.Remove(start);
                        //MasterDif.Add(start, 0);
                }
                else
                {
                    if (!MasterDif.ContainsKey(start))
                        MasterDif.Add(start, 0);
                }
                //Master.Remove(start);
                return;
            }

            List<Vector3i> mastlist = new List<Vector3i>();
            //loop through every key.
            //y

            //will affect blocks



            IterateStretch(0, start,start, mastlist, type, r,m:1, dirX: 1).Deconstruct(out mastlist,out _);
            
            var xcount = mastlist.Count - 1; //take off self.

            //we have wedges done.
            if ((style == 197 || style == 168) && xcount != 0)
            {
                SwapBlock(mastlist,start,type,r,xcount);
                return;
            }

            //will affect blocks
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

            //will affect blocks

            IterateStretch(1,start, new Vector3i(start.X + (xcount), start.Y, start.Z + (zcount)),mastlist,type,r,m: (xcount + 1) * (zcount + 1), dirY:1).Deconstruct(out mastlist, out int ycount);
            //every point in this list becomes an mblock. 
            SwapBlock(mastlist, start, type, r, xcount, ycount, zcount);
            return;
        }


        internal void findOneInvert(Vector3i start, int axis,BlockBit b)
        {
           
            //all blocks in merge must have same rotation and blockid.
            //to handle wedges and corners, we only want a singular merge direction, if we get an list bigger than one, we return.
            var type = b.get_id();
            var style = (ushort)BlockTypes.Sevo_ID((short)type);
            var r = (byte)b.get_Sevo_rot();


            List<Vector3i> mastlist = new List<Vector3i>();
            //loop through every key.
            //y

            //will affect blocks

            IterateStretchInvert(0, start, start, mastlist, type, r,axis:axis, m: 1, dirX: 1).Deconstruct(out mastlist, out _);

            var xcount = mastlist.Count - 1; //take off self.

            //we have wedges done.
            //will affect blocks
            int zcount = 0;
            if (axis == 0 )
            {
                IterateStretchInvert(1, start, new Vector3i(start.X - (xcount), start.Y, start.Z), mastlist, type, r, axis: axis, m: (xcount + 1), dirZ: 1).Deconstruct(out mastlist, out  zcount);
            }
            else
            {
                IterateStretchInvert(1, start, new Vector3i(start.X + (xcount), start.Y, start.Z), mastlist, type, r, axis: axis, m: (xcount + 1), dirZ: 1).Deconstruct(out mastlist, out  zcount);
            }
           
            //will affect blocks
            int ycount = 0;
            switch (axis)
            {
                case 0:
                    IterateStretchInvert(1, start, new Vector3i(start.X - (xcount), start.Y, start.Z + (zcount)), mastlist, type, r, axis: axis, m: (xcount + 1) * (zcount + 1), dirY: 1).Deconstruct(out mastlist, out  ycount);
                    break;
                case 2:
                    IterateStretchInvert(1, start, new Vector3i(start.X + (xcount), start.Y, start.Z - (zcount)), mastlist, type, r, axis: axis, m: (xcount + 1) * (zcount + 1), dirY: 1).Deconstruct(out mastlist, out  ycount);
                    break;
                default:
                    IterateStretchInvert(1, start, new Vector3i(start.X + (xcount), start.Y, start.Z + (zcount)), mastlist, type, r, axis: axis, m: (xcount + 1) * (zcount + 1), dirY: 1).Deconstruct(out mastlist, out  ycount);
                    break;
            }
            //  IterateStretchInvert(1, start, new Vector3i(start.X + (xcount), start.Y, start.Z + (zcount)), mastlist, type, r, axis: axis, m: (xcount + 1) * (zcount + 1), dirY: 1).Deconstruct(out mastlist, out int ycount);
            //every point in this list becomes an mblock. 
            SwapBlockINV(mastlist, start, type, r,axis:axis, xcount, ycount, zcount);
            return;
        }


        internal void SwapBlock(List<Vector3i> mastlist,Vector3i start,int type, byte r, int offX = 0, int offY = 0, int offZ = 0)
        {
            Mblist.Add(BlockTypes.IsHull((short)type)
                ? GetMasterBlock(start, new Vector3i((start.X + offX), (start.Y + (offY)), (start.Z + (offZ))), type, 0)
                : GetMasterBlock(start, new Vector3i((start.X + offX), (start.Y + (offY)), (start.Z + (offZ))), type, r));
            foreach (var v in mastlist)
            {
                if(fast)
                    Master.Remove(v);
                else
                    if(!MasterDif.ContainsKey(v))
                        MasterDif.Add(v, 0);
            }
        }
        internal void SwapBlockINV(List<Vector3i> mastlist, Vector3i start, int type, byte r,int axis, int offX = 0, int offY = 0, int offZ = 0)
        {
            switch (axis)
            {
                case 0:
                    Mblist.Add(GetMasterBlock(start, new Vector3i((start.X - offX), (start.Y + (offY)), (start.Z + (offZ))), type, 2));
                    break;
                case 1:
                    Mblist.Add(GetMasterBlock(start, new Vector3i((start.X + offX), (start.Y - (offY)), (start.Z + (offZ))), type, 2));
                    break;
                default:
                    Mblist.Add(GetMasterBlock(start, new Vector3i((start.X + offX), (start.Y + (offY)), (start.Z - (offZ))), type, 2));
                    break;
            }
            //Mblist.Add(GetMasterBlock(start, new Vector3i((start.X + offX), (start.Y + (offY)), (start.Z + (offZ))), type, r));
            foreach (var v in mastlist)
            {
                if (fast)
                    Master.Remove(v);
                else
                if (!MasterDif.ContainsKey(v))
                    MasterDif.Add(v, 0);
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
            if(!fast)
                if (MasterDif.ContainsKey(c)) return false;
            if (_ignorePaintFlag)
                return Master[c].get_Sevo_rot() == rot && BlockTypes.GetDefault((short)Master[c].get_id()) == BlockTypes.GetDefault((short)type);
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
    }
}

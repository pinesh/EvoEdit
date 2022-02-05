using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using testapp1;

namespace EvoEditApp
{
    public class Mergenew : INotifyPropertyChanged
    { 
        public Dictionary<Vector3i, BlockBit> Master;
        public List<Mblocks> Mblist;
        private SortedDictionary<Vector3i, BlockBit> _ok;
        public Vector3i Max;
        public Vector3i Min;
        private bool _ignorePaintFlag;
        private bool _igoreSlabsFlag;
        public int Scale;

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

        public Mergenew(Dictionary<Vector3i, BlockBit> l, Vector3i u, Vector3i v, int s = 1, bool paint = false)
        {
            _ok = new SortedDictionary<Vector3i, BlockBit>(new Vector3icompare());
            Master = l;
            this.Min = u;
            this.Max = v;
            this.Scale = (int)(4 * Math.Pow(2, s));
            Mblist = new List<Mblocks>();
            _ignorePaintFlag = paint;
            Console.WriteLine(Master.Count);
            Console.WriteLine($"min = [{Min.X},{Min.Y},{Min.Z}]");
            Console.WriteLine($"max = [{Max.X},{Max.Y},{Max.Z}]");
        }
        
        public Tuple<ushort,Vector3i> get_scale_wedge(Vector3i start, Vector3i end,int rot)
        {
            int x = Math.Abs(start.X - end.X) / Scale;
            int y = Math.Abs(start.Y - end.Y) / Scale;
            int z = Math.Abs(start.Z - end.Z) / Scale;
            int offset = Math.Max(x, Math.Max(y, z))*4;
            switch (rot)
            {
                case 0:
                    //we're fine, no shunt required.
                    return new Tuple<ushort, Vector3i>(range_to_scale(x, y, z),new Vector3i(0,0,0));
                case 1:
                    //we're fine, no shunt required
                    return new Tuple<ushort, Vector3i>(range_to_scale(z, y, x),new Vector3i(0, 0, 0));
                case 2:
                    //we need to shunt left relative.
                    return new Tuple<ushort, Vector3i>(range_to_scale(x, y, z),new Vector3i(0, 0, offset));
                case 3:
                    //we need to shunt left relative.
                    return new Tuple<ushort, Vector3i>(range_to_scale(z, y, x),new Vector3i(offset, 0, 0));
                case 4:
                    //we need to shunt right relative. //bottom (red)
                    return new Tuple<ushort, Vector3i>(range_to_scale(x, y, z),new Vector3i(0, 0, offset));
                case 5:
                    //we're fine, no need to shunt,
                    return new Tuple<ushort, Vector3i>(range_to_scale(z, y, x),new Vector3i(0, 0, 0));
                case 6:
                    //we're fine, no shunt required.
                    return new Tuple<ushort, Vector3i>(range_to_scale(x, y, z),new Vector3i(0, 0, 0));
                case 7:
                    //we need to shunt right relative.
                    return new Tuple<ushort, Vector3i>(range_to_scale(z, y, x),new Vector3i(offset, 0, 0));
                case 21:
                    //we're fine, no shunt required.
                    return new Tuple<ushort, Vector3i>(range_to_scale(z, x, y),new Vector3i(0, 0, 0));
                case 23:
                    //we need to move it up however long the scale is.
                     return new Tuple<ushort, Vector3i>(range_to_scale(z, x, y),new Vector3i(0, offset, 0));
                case 17:
                    //needs to go up however long scale is
                    return new Tuple<ushort, Vector3i>(range_to_scale(z, x, y),new Vector3i(0, offset, 0));
                case 19:
                    //we're fine, no shunt required.
                    return new Tuple<ushort, Vector3i>(range_to_scale(z, x, y),new Vector3i(0, 0, 0));
            }

            //case y is large, scale should be x or z depending on rotation.
            return new Tuple<ushort, Vector3i>(range_to_scale(z, y, x),new Vector3i(0,0,0));
        }

        public Tuple<ushort, Vector3i> get_scale_slab(Vector3i start, Vector3i end, int rot)
        {
            int x = Math.Abs(start.X - end.X) / Scale;
            int y = Math.Abs(start.Y - end.Y) / Scale;
            int z = Math.Abs(start.Z - end.Z) / Scale;

            //Y is not possible for a slab, so the other must be unzero.
           // Console.WriteLine($"rot:{rot}x:{x},y:{y},z:{z}");
           
                switch (rot)
                {
                    case 8://side slab needs to go up 1. when shorter side (z? //SIDE CORRECT
                        return new Tuple<ushort, Vector3i>(range_to_scale(x, z, y), new Vector3i(0, 4*y, 0));//DONE
                    case 4://BOTTOM needs to go forwards
                        return new Tuple<ushort, Vector3i>(range_to_scale(x, y, z), new Vector3i(0, 0, 4*z));//check again
                    case 16://back needs to go up
                        return new Tuple<ushort, Vector3i>(range_to_scale(y, x, y), new Vector3i(0, 4*y, 0));
                    case 0://TOP CORRECT
                        return new Tuple<ushort, Vector3i>(range_to_scale(x, y, z), new Vector3i(0, 0, 0));//done
                    case 12://SIDE CORRECT
                        return new Tuple<ushort, Vector3i>(range_to_scale(x, z, y), new Vector3i(0, 0, 0));//done
                    case 22:
                        return new Tuple<ushort, Vector3i>(range_to_scale(y, x, z), new Vector3i(0, 4*y, 4*z));
                    //  case 23:
                    //return new Tuple<ushort, Vector3i>(range_to_scale(y, x, z), new Vector3i(0, 0, 0));
            }

            
            //case y is large, scale should be x or z depending on rotation.
            return new Tuple<ushort, Vector3i>(range_to_scale(x, y, z), new Vector3i(0, 0, 0));
        }
        
        //insert a thin slab at the same place. with the same scale. 
        public Tuple<ushort, Vector3i> get_scale_thiccslab(Vector3i start, Vector3i end, int rot)
        {
            int x = Math.Abs(start.X - end.X) / Scale;
            int y = Math.Abs(start.Y - end.Y) / Scale;
            int z = Math.Abs(start.Z - end.Z) / Scale;
            //Y is not possible for a slab, so the other must be unzero.
            //Console.WriteLine($"rot:{rot}x:{x},y:{y},z:{z}");

            switch (rot)//when this is 1, we want to add 2
            {
                case 8://side slab needs to go up 1. when shorter side (z? //SIDE CORRECT
                    return new Tuple<ushort, Vector3i>(range_to_scale(x, z, y), new Vector3i(0, 4 * y, 2));//DONE //not right
                case 4://BOTTOM needs to go forwards
                    return new Tuple<ushort, Vector3i>(range_to_scale(x, y, z), new Vector3i(0, -2, 4 * z));//check again
                case 16://back needs to go up
                    return new Tuple<ushort, Vector3i>(range_to_scale(y, x, y), new Vector3i(2, 4 * y, 0));
                case 0://TOP CORRECT
                    return new Tuple<ushort, Vector3i>(range_to_scale(x, y, z), new Vector3i(0, 2, 0));//done
                case 12://SIDE CORRECT
                    return new Tuple<ushort, Vector3i>(range_to_scale(x, z, y), new Vector3i(0, 0, -2));//done // not right
                case 22:
                    return new Tuple<ushort, Vector3i>(range_to_scale(y, x, z), new Vector3i(-2, 4 * y, 4 * z));
                //  case 23:
                //return new Tuple<ushort, Vector3i>(range_to_scale(y, x, z), new Vector3i(0, 0, 0));
            }


            //case y is large, scale should be x or z depending on rotation.
            return new Tuple<ushort, Vector3i>(range_to_scale(x, y, z), new Vector3i(0, 0, 0));
        }

        public ushort get_scale(Vector3i start, Vector3i end)
        {
            return range_to_scale(Math.Abs(start.X - end.X)/Scale, Math.Abs(start.Y - end.Y) / Scale, Math.Abs(start.Z - end.Z) / Scale);
        }
        public ushort range_to_scale(int x,int y,int z)
        {
            ushort u = (ushort)z;
            u = (ushort)(u << (ushort)4 | (ushort)y);
            u = (ushort)(u << (ushort)4 | (ushort)x);
            return u;

        }

        public class Vector3icompare : IComparer<Vector3i>
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
                else//if a bigger x
                {
                    return 1;
                }
            }
        }
        public void Merge()
        {
            var startpos = Min;
            if (startpos.X % Scale != 0)
            {
                startpos.X -= startpos.X % Scale;
            }
            if (startpos.Y % Scale != 0)
            {
                startpos.Y -= startpos.Y % Scale;
            }
            if (startpos.Z % Scale != 0)
            {
                startpos.Z -= startpos.Z % Scale;
            }
            _ok = new SortedDictionary<Vector3i, BlockBit>(new Vector3icompare());
            foreach (var keypair in Master)
            {
                //Console.WriteLine($"block type {keypair.Value.get_id()}, block axis {keypair.Value.get_axis_rotation()}, block rotation {keypair.Value.get_rotations()}");
                _ok.Add(keypair.Key,keypair.Value);
            }
            Min = startpos;
            
           
            List<Vector3i> keys = _ok.Keys.ToList();
            int cur = 0;
            foreach (var key in keys)
            {
               
                cur += 1;
                int m = Master.Count;
                if (cur % 100 == 0)
                {
                    NotifyPropertyChanged($"Optimized: {cur}/{m} Blocks");
                }
                if (!Master.ContainsKey(key)) continue;
                findOne(key);
            }

        // Console.WriteLine($"reduced to {Mblist.Count}");
        }

        public List<Vector3i> GetAllPoints(Vector3i start, Vector3i end, int type,int r)
        {
            List<Vector3i> points = new List<Vector3i>();
            for (int yIndex = start.Y; yIndex <= end.Y; yIndex += Scale)
            {
                //z
                for (int zIndex = start.Z ; zIndex <= end.Z; zIndex += Scale)
                {
                    //x
                    for (int xIndex = start.X; xIndex <= end.X; xIndex += Scale)
                    {
                        var v = new Vector3i(xIndex, yIndex, zIndex);
                        if (!Comp(v, type, r)) return points;
                        points.Add(v);
                    }
                }
            }
            return points;
        }
        public void findOne(Vector3i start)
        {
            var b = Master[start];
            //all blocks in merge must have same rotation and blockid.

            //to handle wedges and corners, we only want a singular merge direction, if we get an list bigger than one, we return.
            var type = b.get_id();
            var style = (ushort)BlockTypes.Sevo_ID((short)type);
            var r = b.get_Sevo_rot();

            if (style >= 198 && style <= 200)//don't merge tetra or hepta 198-200
            {
                Mblocks non = new Mblocks
                {
                    Startpos = start,
                    Endpos = start,
                    Type = type,
                    Rot = r
                };
                Mblist.Add(non);
                Master.Remove(start);
                return;
            }
            List<Vector3i> mastlist = new List<Vector3i>();
            int max = 16 * Scale;
            var u = start;
            //loop through every key.
            //y
            int c = 0;
            while (c < 16)
            {
                List<Vector3i> lv = GetAllPoints(new Vector3i(start.X + (Scale * c), start.Y, start.Z),
                    new Vector3i(u.X + (Scale * c), u.Y, u.Z), type, r);
                if (mastlist.Count + lv.Count > c)
                {
                    mastlist.AddRange(lv);
                }
                else
                {
                    break;
                }
                c++;

            }
            var xcount = mastlist.Count-1;//take off self.

            //we have wedges done.
            if (style == 197|| style == 168 && xcount != 0)
            {
                Mblocks zm = new Mblocks
                {
                    Startpos = start,
                    Endpos = new Vector3i(u.X + (Scale * xcount), u.Y, u.Z),
                    Type = type,
                    Rot = r
                };
                Mblist.Add(zm);

                foreach (var v in mastlist)
                {
                    Master.Remove(v);
                }

                return;
            }

            c = 1;
            while (c < 16)
            {
                List<Vector3i> lv = GetAllPoints(new Vector3i(start.X, start.Y, start.Z + (Scale * (c))),
                    new Vector3i(u.X + (Scale * xcount), u.Y, u.Z + (Scale * c)), type, r);
                if (mastlist.Count + lv.Count == (xcount+1)*(c+1))
                {
                    mastlist.AddRange(lv);
                }
                else 
                {
                    break;
                }

                c++;
            }
            var zcount = c-1;
            //we have wedges done.
            if (style == 197 || style == 168 && zcount != 0)
            {
                Mblocks zm = new Mblocks
                {
                    Startpos = start,
                    Endpos = new Vector3i(u.X + (Scale * xcount), u.Y, u.Z + (Scale * zcount)),
                    Type = type,
                    Rot = r
                };
                Mblist.Add(zm);

                foreach (var v in mastlist)
                {
                    Master.Remove(v);
                }
                return;
            }

            if (style >= 243 && style <= 245 && zcount != 0 && xcount != 0) //slabs can only have two stretches
            {
                Mblocks zm = new Mblocks
                {
                    Startpos = start,
                    Endpos = new Vector3i(u.X + (Scale * xcount), u.Y, u.Z + (Scale * zcount)),
                    Type = type,
                    Rot = r
                };
                Mblist.Add(zm);

                foreach (var v in mastlist)
                {
                    Master.Remove(v);
                }
                return;
            }

            c = 1;
            while (c < 16)
            {
                List<Vector3i> lv = GetAllPoints(new Vector3i(start.X, start.Y + (Scale * (c)), start.Z),
                    new Vector3i(u.X + (Scale * xcount), u.Y + (Scale * c), u.Z + (Scale * zcount)),
                    type, r);
                
                if (mastlist.Count + lv.Count == (c+1)*(xcount+1)*(zcount+1))
                {
                    mastlist.AddRange(lv);
                }
                else
                {
                    break;
                }
                c++;
            }

            //every point in this list becomes an mblock. 
            Mblocks m = new Mblocks
            {
                Startpos = start,
                Endpos = new Vector3i(u.X + (Scale * xcount), u.Y + (Scale * (c-1)), u.Z + (Scale * zcount)),
                Type = type,
                Rot = r
            };
            Mblist.Add(m);
            
            foreach (var v in mastlist)
            {
                Master.Remove(v);
            }
           

        }
        public bool Comp(Vector3i c,int type, int rot)
        {
            if (!Master.ContainsKey(c)) return false;
            return Master[c].get_Sevo_rot() == rot && Master[c].get_id() == type;
        }

        public struct Mblocks
        {
            public Vector3i Startpos;
            public Vector3i Endpos;
            public int Type;
            public int Rot;

        }
    }
}

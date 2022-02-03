using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace EvoEditApp
{
    public class mergenew
    {
        public Dictionary<Vector3i, BlockBit> master;
        public List<mblocks> mblist;
        private SortedDictionary<Vector3i, BlockBit> ok;
        public Vector3i max;
        public Vector3i min;
        public int scale;

        public mergenew(Dictionary<Vector3i, BlockBit> l,  Vector3i u, Vector3i v , int s = 1)
        {
            ok = new SortedDictionary<Vector3i, BlockBit>(new Vector3icompare());
            master = l;
            this.min = u;
            this.max = v;
            this.scale = (int)(4 * Math.Pow(2, s));
            mblist = new List<mblocks>();
            Console.WriteLine(master.Count);
            Console.WriteLine($"min = [{min.x},{min.y},{min.z}]");
            Console.WriteLine($"max = [{max.x},{max.y},{max.z}]");
        }
        
        public Tuple<ushort,Vector3i> get_scale_wedge(Vector3i start, Vector3i end,int rot)
        {
            int x = Math.Abs(start.x - end.x) / scale;
            int y = Math.Abs(start.y - end.y) / scale;
            int z = Math.Abs(start.z - end.z) / scale;
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

        public ushort get_scale(Vector3i start, Vector3i end)
        {
            return range_to_scale(Math.Abs(start.x - end.x)/scale, Math.Abs(start.y - end.y) / scale, Math.Abs(start.z - end.z) / scale);
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
                if (x.y < y.y)
                {
                    return -1;
                }

                if (x.y > y.y)
                {
                    return 1;
                }
                //order on same y plane

                
                if (x.z < y.z)
                {
                    return -1;
                }
                if (x.z > y.z)
                {
                    return 1;
                }

                //we want least x first
                if (x.x < y.x)
                {
                    return -1;
                }
                else//if a bigger x
                {
                    return 1;
                }
            }
        }
        public void merge()
        {
            var startpos = min;
            if (startpos.x % scale != 0)
            {
                startpos.x -= startpos.x % scale;
            }
            if (startpos.y % scale != 0)
            {
                startpos.y -= startpos.y % scale;
            }
            if (startpos.z % scale != 0)
            {
                startpos.z -= startpos.z % scale;
            }
            ok = new SortedDictionary<Vector3i, BlockBit>(new Vector3icompare());
            foreach (var keypair in master)
            {
                ok.Add(keypair.Key,keypair.Value);
            }
            min = startpos;

            List<Vector3i> keys = ok.Keys.ToList();
            foreach (var key in keys)
            {
                if (!master.ContainsKey(key)) continue;
                findOne(key);
            }
            Console.WriteLine($"reduced to {mblist.Count}");
        }

        public List<Vector3i> getAllPoints(Vector3i start, Vector3i end, int type,int r)
        {
            List<Vector3i> points = new List<Vector3i>();
            for (int yIndex = start.y; yIndex <= end.y; yIndex += scale)
            {
                //z
                for (int zIndex = start.z ; zIndex <= end.z; zIndex += scale)
                {
                    //x
                    for (int xIndex = start.x; xIndex <= end.x; xIndex += scale)
                    {
                        var v = new Vector3i(xIndex, yIndex, zIndex);
                        if (!comp(v, type, r)) return points;
                        points.Add(v);
                    }
                }
            }
            return points;
        }
        public void findOne(Vector3i start)
        {
            var b = master[start];
            //all blocks in merge must have same rotation and blockid.

            //to handle wedges and corners, we only want a singular merge direction, if we get an list bigger than one, we return.
            var type = b.get_id();
            var style = (ushort)BlockTypes.Sevo_ID((short)type);
            var r = b.get_Sevo_rot();

            if (style != 196 && style != 197)
            {
                mblocks non = new mblocks
                {
                    startpos = start,
                    endpos = start,
                    type = type,
                    rot = r
                };
                mblist.Add(non);
                master.Remove(start);
                return;
            }
            List<Vector3i> mastlist = new List<Vector3i>();
            int max = 16 * scale;
            var u = start;
            //loop through every key.
            //y
            int c = 0;
            while (c < 16)
            {
                List<Vector3i> lv = getAllPoints(new Vector3i(start.x + (scale * c), start.y, start.z),
                    new Vector3i(u.x + (scale * c), u.y, u.z), type, r);
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
            if (style == 197 && xcount != 0)
            {
                mblocks zm = new mblocks
                {
                    startpos = start,
                    endpos = new Vector3i(u.x + (scale * xcount), u.y, u.z),
                    type = type,
                    rot = r
                };
                mblist.Add(zm);

                foreach (var v in mastlist)
                {
                    master.Remove(v);
                }

                return;
            }

            c = 1;
            while (c < 16)
            {
                List<Vector3i> lv = getAllPoints(new Vector3i(start.x, start.y, start.z + (scale * (c))),
                    new Vector3i(u.x + (scale * xcount), u.y, u.z + (scale * c)), type, r);
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
            if (style == 197 && zcount != 0)
            {
                mblocks zm = new mblocks
                {
                    startpos = start,
                    endpos = new Vector3i(u.x + (scale * xcount), u.y, u.z + (scale * zcount)),
                    type = type,
                    rot = r
                };
                mblist.Add(zm);

                foreach (var v in mastlist)
                {
                    master.Remove(v);
                }

                return;
            }
            c = 1;
            while (c < 16)
            {
                List<Vector3i> lv = getAllPoints(new Vector3i(start.x, start.y + (scale * (c)), start.z),
                    new Vector3i(u.x + (scale * xcount), u.y + (scale * c), u.z + (scale * zcount)),
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
            mblocks m = new mblocks
            {
                startpos = start,
                endpos = new Vector3i(u.x + (scale * xcount), u.y + (scale * (c-1)), u.z + (scale * zcount)),
                type = type,
                rot = r
            };
            mblist.Add(m);
            
            foreach (var v in mastlist)
            {
                master.Remove(v);
            }
           

        }
        public bool comp(Vector3i c,int type, int rot)
        {
            if (!master.ContainsKey(c)) return false;
            return master[c].get_Sevo_rot() == rot && master[c].get_id() == type;
        }

        public struct mblocks
        {
            public Vector3i startpos;
            public Vector3i endpos;
            public int type;
            public int rot;

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvoEditApp;

namespace testapp1
{

    public struct BlockBit
    {
        private readonly int _int24;
        private readonly int _version;
        public bool Visited;
        public bool invert;
        public BlockBit(int i, int v, bool u = false,bool inv=false)
        {
            _int24 = i;
            _version = v;
            Visited = u;
            invert = inv;
        }

        public int get_int_24()
        {
            return _int24;
        }

        public int GetSevoId()
        {
            return BlockTypes.Sevo_ID((short)get_id());
        }

        public Vector3i GetSevoPaint()
        {
            return BlockTypes.Sevo_Paint((short)get_id());
        }

        private static int bits_parse(int x, int start, int length)
        {
            var tmp = x >> start;
            return tmp & ((int)Math.Pow(2, length) - 1);
        }

        public int get_id()
        {
            return bits_parse(_int24, 0, 11);
        }

        public int get_axis_rotation()
        {
            if (_version >= 3) return bits_parse(_int24, 21, 3);
            var blockId = (short)get_id();
            if (BlockTypes.IsHepta(blockId) || BlockTypes.IsTetra(blockId)) //TYPE 4/5
                return bits_parse(_int24, 22, 1);
            var bit2223 = bits_parse(_int24, 22, 2);
            if (BlockTypes.IsCorner(blockId)) //TYPE 2
                return bit2223 | (bits_parse(_int24, 19, 1) << 2);
            return bit2223;
        }

        public int get_rotations()
        {
            return bits_parse(_int24, this._version < 3 ? 20 : 19, 2);
        }

        private int get_face()
        {
            if (this._version < 3)
                return bits_parse(this._int24, 20, 3);

            return bits_parse(this._int24, 19, 3);
        }

        private int get_block_side_id()
        {
            return bits_parse(_int24, this._version < 3 ? 20 : 19, 3);
        }

        public int get_Sevo_rot()
        {
            return invert ? 2 : get_Sevo_rot2();
        }

        public void markInvert()
        {
            invert = true;
        }

        public int get_sevo_slabs(int r, int a)
        {
            switch (a)
            {
                case 0: //done
                    switch (r)
                    {
                        case 1:
                            return 12; //RIGHT HAND SIDE DONE
                        case 2:
                            return 0; //TOP SIDE CORRECT
                        case 3:
                            return 4; //BOTTOM SIDE CORRECT
                        default:
                            return 8; //might be ok //LEFT SIDE?
                    }
                case 1:
                    switch (r) //front and back?
                    {
                        case 1:
                            return 22;//FRONT SIDE
                        default:
                            return 16;//16
                    }
            }

            return 0;
        }

        public int get_sevo_corners2(int r, int a)
        {
            switch (a)
            {
                case 0://done
                    switch (r)
                    {
                        case 1:
                            return 3;//x
                        case 2:
                            return 0; //2/20
                        case 3:
                            return 1; //2/20
                        default:
                            return 2; //2/20
                    }
                case 1:
                    switch (r)//NOT RIGHT
                    {
                        case 1:
                            return 6;
                        case 2:

                            return 5; //5
                        case 3:

                            return 4;//4
                        default:
                            return 7;
                    }
                case 2:
                    switch (r)//WEDGES DONE
                    {
                        case 1:
                            return 8;
                        case 2:
                            return 9;
                        case 3:
                            return 10;
                        default:
                            return 11;
                    }
                case 3:
                    switch (r)//WEDGES - most likely just corners?
                    {
                        case 1:
                            return 14;
                        case 2:
                            return 13;
                        case 3:
                            return 12;
                        default:
                            return 15;
                    }
                case 4:
                    switch (r)//ONLY CORNERS
                    {
                        case 1:
                            return 23;
                        case 2:
                            return 22;
                        case 3:
                            return 21;
                        default:
                            return 20;
                    }
                case 5:
                    switch (r)//ONLY CORNERS
                    {
                        case 1:
                            return 16;
                        case 2:
                            return 17;
                        case 3:
                            return 18;
                        default:
                            return 19;
                    }
                default:
                    return 0;
            }
        }

        public int get_Sevo_rot2()
        {
            int axis = get_axis_rotation();
            int rot = get_rotations();
            short id = 0;
            id = (short)this.get_id();
            if (BlockTypes.IsCorner(id))
                return get_sevo_corners2(rot, axis);
            if (BlockTypes.IsSlab(id))
                return get_sevo_slabs(rot, axis);
            switch (axis)
            {
                case 0:
                    switch (rot)//this is the top
                    {

                        case 1://trialing 3
                            if (BlockTypes.IsWedge(id)) return 0;
                            return 3;
                        case 2:
                            //one of the cases here should be directed to 3,
                            //if()
                            if (BlockTypes.IsWedge(id)) return 3;
                            return 0; // one of these 0's should be a 1 in a single case?
                        case 3:
                            if (BlockTypes.IsWedge(id)) return 2;
                            return 1;//changed from 1
                        default:
                            if (BlockTypes.IsWedge(id)) return 1;
                            return 2; //X
                    }
                case 1:
                    switch (rot)// this is the bottom
                    {//0 FIXED THE HEPTA
                        case 1:
                            if (BlockTypes.IsWedge(id)) return 6;
                            return 11;//6//FUCKING HEPTA (NO 7)22 //23 left green was used?
                        case 2:
                            if (BlockTypes.IsWedge(id)) return 5;
                            return 8; //5
                        case 3:
                            if (BlockTypes.IsWedge(id)) return 4;
                            return 17;//4
                        default:
                            if (BlockTypes.IsWedge(id)) return 7;
                            return 14;
                    }
                case 2:
                    switch (rot)//WEDGES
                    {
                        case 1:
                            return 23;
                        case 2:
                            return 17;
                        case 3:
                            return 19;
                        default:
                            return 21;
                    }
                case 3:
                    switch (rot)//WEDGES - most likely just corners?
                    {
                        case 1:
                            return 9;
                        case 2:
                            return 9; //might be right was 22
                        case 3:
                            return 9;
                        default:
                            return 9; //was 20/5
                    }
                default:
                    return 0;
            }
        }
    }

}

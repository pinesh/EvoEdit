using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Ionic.Zip;
using Ionic.Zlib;
using EvoEditApp;
using SeekOrigin = System.IO.SeekOrigin;

namespace EvoEditApp
{
    class SM_Load
    {
        public string name;
        public smd3 smd;
        public int v;
        public int scale;

        public SM_Load(int s = 1)
        {
            scale = s;
            v = 0;
            smd = new smd3(s:scale);
        }
        public List<Vector3i> Dump()
        {
            return smd.getBlockList().blist.Keys.ToList();
        }
        public Dictionary<Vector3i,BlockBit> DumpData()
        {
            return smd.getBlockList().blist;
        }

        public void Read(DirectoryInfo directory_bp)
        {
            name = directory_bp.Name;
            smd.ReadFolder(directory_bp);
        }
    }

    public class smd3
    {
        private int segments_in_line_region;
        private int blocks_in_line_segment;
        private Vector3i _position_core;
        private BlockList blist;
        private readonly int scale;
        public int v;
        public BlockList getBlockList()
        {
            return blist;
        }

        public Tuple<Vector3i,Vector3i> get_min_max_vector()
        {
            var min_t = new Vector3i(16, 16, 16);
            var max_t = new Vector3i(16, 16, 16);

            foreach (Vector3i v in blist.blist.Keys)
            {
                min_t = Vector3i.Min(min_t, v);
                max_t = Vector3i.Max(max_t, v);
            }

            return new Tuple<Vector3i, Vector3i>(min_t, max_t);
        }
        public smd3(int a = 16,int b = 32,int s = 1)
        {
            v = 0;
            segments_in_line_region = a;
            blocks_in_line_segment = b;
            _position_core = new Vector3i(16, 16, 16);
            blist = new BlockList();
            this.scale = s;
        }

        public void ReadFolder(DirectoryInfo directoryBp)
        {
            if (!directoryBp.Name.StartsWith("ATTACHED") && !directoryBp.Name.StartsWith("DATA")) return;
            var fileList = directoryBp.GetFiles().OrderBy(fi => fi.Name).ToList();
            if (fileList.Count() < 0)
            {
                Console.WriteLine(@"No smd files found");
                return;
            }
            var fName = fileList[0];
            if (directoryBp.Name.StartsWith("ATTACHED"))
            {
                //nesting this stuff is bad, we're only going to take the primary entity of an attached entity. 
                var directoryBpData = new DirectoryInfo(Path.Combine(directoryBp.FullName, "DATA"));
                if (directoryBpData.Exists)
                {
                    fileList = directoryBpData.GetFiles().OrderBy(fi => fi.Name).ToList();
                    if (fileList.Count() < 0)
                    {
                        Console.WriteLine(@"No smd files found");
                        return;
                    }
                    fName = fileList[0];
                }
            }
            
            switch (fName.Extension)
            {
                case ".smd3":
                {
                    v = 3;
                    var smdRegion = new SmdRegion(s:scale);
                    foreach (var fileName in fileList)
                    {
                        smdRegion.read(fileName, blist);
                    }

                    break;
                }
                case ".smd2":
                {
                    v = 2;
                    Console.WriteLine(@"'smd2' file format found");
                    var smd2 = new Smd2(s:scale);
                    smd2.Read(directoryBp); Console.WriteLine(@"Finished Read");
                    this.blist = smd2.GetBlockList();
                    break;
                }
                default:
                    throw new Exception("Unknown smd format");
            }
        }



        public void Read(DirectoryInfo directory_bp)
        {
            var directory_data = new DirectoryInfo(System.IO.Path.Combine(directory_bp.FullName, "DATA"));
            //directory_data = os.path.join(directory_blueprint, "DATA");
            var fileList = directory_data.GetFiles().OrderBy( fi => fi.Name).ToList();
            if (fileList.Count() < 0)
            {
                Console.WriteLine("no smd files found");
                return;
            }
            //assert len(file_list) > 0, "No smd files found"
            var f_name = fileList[0];
            if ((f_name.Attributes & FileAttributes.Directory) != 0 && f_name.Name.StartsWith("ATTACHED"))
            {
                var dp = new DirectoryInfo(f_name.FullName);
                fileList = dp.GetFiles().OrderBy(fi => fi.Name).ToList();
                if (fileList.Count() < 0)
                {
                    Console.WriteLine("no smd files found");
                    return;
                }
                f_name = fileList[0];
            }
            if (f_name.Extension.Equals(".smd3"))
            {
               v = 3;
                var smd_region =new SmdRegion();
                foreach(var file_name in fileList)
                {
                    //this._file_name_prefix, x, y, z = os.path.splitext(file_name)[0].rsplit('.', 3) TODO CHECK IF THIS COMES UP AGAIN
                    smd_region.read(file_name, blist);
                }
            }
            else if (f_name.Extension.Equals(".smd2"))
            {
                v = 2;
                Console.WriteLine("'smd2' file format found");
                var smd2 = new Smd2();
                smd2.Read(directory_bp); Console.WriteLine("done");
                //var offset = (8, 8, 8);
                this.blist = smd2.GetBlockList();
                //this.blist.move_positions(offset);
            }
            else
            {
                throw new Exception("Unknown smd format");
            }
        }
    }

    public struct BlockBit
    {
        private readonly int int_24;
        private readonly int version;
        public bool visited; 

        public BlockBit(int i, int v, bool u = false)
        {
            int_24 = i;
            version = v;
            visited = u;
        }

        public int get_int_24()
        {
            return int_24;
        }

        public int GetSevoID()
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
            return bits_parse(int_24, 0, 11);
        }

        private int get_axis_rotation()
        {
            if (version >= 3) return bits_parse(int_24, 21, 3);
            var block_id = (short)get_id();
            if (BlockTypes.isHepta(block_id) || BlockTypes.isTetra(block_id)) //TYPE 4/5
                return bits_parse(int_24, 22, 1);
            var bit_22_23 = bits_parse(int_24, 22, 2);
            if (BlockTypes.isCorner(block_id)) //TYPE 2
                return bit_22_23 | (bits_parse(int_24, 19, 1) << 2);
            return bit_22_23;
        }

        private int get_rotations()
        {
            return bits_parse(int_24, this.version < 3 ? 20 : 19, 2);
        }

        private int get_face()
        {
            if (this.version < 3)
                return bits_parse(this.int_24, 20, 3);

            return bits_parse(this.int_24, 19, 3);
        }

        private int get_block_side_id()
        {
            return bits_parse(int_24, this.version < 3 ? 20 : 19, 3);
        }

        public int get_Sevo_rot()
        {
            return get_Sevo_rot2();
        }


        public int get_sevo_corners2(int r,int a)
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
            if (BlockTypes.isCorner(id))
                return get_sevo_corners2(rot, axis);
            //  Console.WriteLine("fucked");

            switch (axis)
            {
                case 0:
                    switch (rot)//this is the top
                    {
                        case 1://trialing 3
                            if (BlockTypes.isWedge(id)) return 0;
                                return 3; 
                        case 2:
                            //one of the cases here should be directed to 3,
                            //if()
                            if (BlockTypes.isWedge(id)) return 3;
                            return 0; // one of these 0's should be a 1 in a single case?
                        case 3:
                            if (BlockTypes.isWedge(id)) return 2;
                            return 1;//changed from 1
                        default:
                            if (BlockTypes.isWedge(id)) return 1; 
                            return 2; //X
                    }
                case 1:
                    switch (rot)// this is the bottom
                    {//0 FIXED THE HEPTA
                        case 1:
                            if (BlockTypes.isWedge(id)) return 6;
                            return 23;//6//FUCKING HEPTA (NO 7)22 
                        case 2:
                            if (BlockTypes.isWedge(id)) return 5;
                            return 8; //5
                        case 3:
                            if (BlockTypes.isWedge(id)) return 4;
                            return 17;//4
                        default:
                            if (BlockTypes.isWedge(id)) return 7;
                            return 14; //7//FUCKING HEPTA INVERSE 7 13 <- THIS WAS THE ISSUE
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



internal class SmdRegion
    {
        private Tuple<int, int, int, int> version;
        private readonly int _segments_in_a_cube;
        private readonly int _segments_in_an_area;
        private readonly int _segments_in_a_line;
        private readonly int _blocks_in_a_line_in_a_segment;
        private readonly int scale;
        public SmdRegion(int segments_in_a_line= 16, int blocks_in_a_line= 32,int s =1)
        {
            this._blocks_in_a_line_in_a_segment = blocks_in_a_line;
            this._segments_in_a_line = segments_in_a_line;
            this._segments_in_an_area = this._segments_in_a_line * this._segments_in_a_line;
            this._segments_in_a_cube = this._segments_in_an_area * this._segments_in_a_line;
            this.version = new Tuple<int,int,int,int>(3, 0, 0, 0);
            this.scale = s;
            //this.position_to_segment = { }
            //this.position_to_region = new Dictionary<int, SmdRegion>();
            // this._block_list = new BlockList();
        }

        private static bool _isEOF(SMBinary s)
        {
            if (s.BaseStream.Position < s.BaseStream.Length)
                return true;
            s.BaseStream.Seek(-1, SeekOrigin.Current);
            return false;
        }

        private Tuple<ushort, ushort> _read_segment_index(SMBinary s)
        {
            var indentifier = s.ReadUInt16(true);
            var size = s.ReadUInt16(true);
            return new Tuple<ushort, ushort>(indentifier, size);
        }

        private Dictionary<ushort, ushort> _read_region_header(SMBinary s)
        {

            //this._version = //
            this.version = s.read_vector_4_byte();
            var segment_id_to_size = new Dictionary<ushort, ushort>();
            for (var i = 0; i < this._segments_in_a_cube; i++)
            {
                this._read_segment_index(s).Deconstruct(out ushort identifier, out ushort size);
                if (identifier > 0)
                {
                    segment_id_to_size[identifier] = size;
                }
               
            }
            return segment_id_to_size;
        }
        private void _read_file(BlockList block_list, SMBinary s)
        {
            var segment_id_to_size = this._read_region_header(s);
            ushort segment_id = 0;
            var segement = new SmdSegment(this._blocks_in_a_line_in_a_segment,scale);
            while (_isEOF(s))
            {
                segment_id += 1;
                if (!segment_id_to_size.ContainsKey(segment_id))
                {
                    s.BaseStream.Seek(49152, SeekOrigin.Current);
                    continue;
                }
                if (segment_id_to_size[segment_id] == 0)
                {
                    s.BaseStream.Seek(49152, SeekOrigin.Current);
                    continue;
                }
                segement.read(block_list, s);
            }
        }

        public void read(FileInfo file, BlockList block_list)
        {
            Console.WriteLine($"Reading file {file.Name}");
            using (FileStream f = new FileStream(file.FullName, FileMode.Open))
            {
                this._read_file(block_list, new SMBinary(f));
                Console.WriteLine("read1");
            }
        }

        internal class SmdSegment
        {
            private int _blocks_in_a_line;
            private int _blocks_in_an_area;
            private int _blocks_in_a_cube;
            private int _version;
            private int scale;
            private ulong _timestamp;
            private Vector3i _position;
            private bool has_valid_data;
            private uint _compressed_size;
            private readonly int _data_size;

            public SmdSegment(int blocks_in_a_line = 32,int s = 1)
            {
                this._blocks_in_a_line = blocks_in_a_line;
                this._blocks_in_an_area = this._blocks_in_a_line * this._blocks_in_a_line;
                this._blocks_in_a_cube = this._blocks_in_an_area * this._blocks_in_a_line;
                this._version = 2;
                this._timestamp = 0;
                this._position = null;
                this.has_valid_data = false;
                this._compressed_size = 0;
                this.scale = s;
                // this.block_index_to_block = { }
            }
            public static byte[] DecompressZlib(Stream source)
            {
                byte[] result = null;
                using (MemoryStream outStream = new MemoryStream())
                {
                    using (InflaterInputStream inf = new InflaterInputStream(source))
                    {
                        inf.CopyTo(outStream);
                    }
                    result = outStream.ToArray();
                }
                return result;
            }
            private void _read_header(SMBinary s)
            {
                this._version = Math.Abs((int)((int)s.ReadByte()));
                if(_version >3 || _version <2)
                    Console.WriteLine("unsupported version");

                this._timestamp = s.ReadUInt64();
                this._position = s.read_vector_3_int32();
                this.has_valid_data = s.ReadBoolean();
                this._compressed_size = s.ReadUInt32();

            }
            private void _read_block_data(BlockList block_list, SMBinary s)
            {
                var decompressed_data = DecompressZlib(new MemoryStream(s.ReadBytes((int)this._compressed_size)));
                for (int index = 0; index < (int)(decompressed_data.Length / 3); index++)
                {
                    var pos = index * 3;
                    byte[] padded = new byte[3];
                    var int_24 = 0;
                    Array.Copy(decompressed_data, pos, padded, 0, 3);
                    if (this._version < 3)
                    {
                         int_24 = this.toUnsignedInt(padded);
                    }
                    else
                    {
                         int_24 = this.toUnsignedIntB(padded);
                    }
                    if (int_24 == 0) continue;
                    BlockBit block = new BlockBit(int_24, this._version);
                    if (!BlockTypes.isAnyHull((short)block.get_id()))
                    {
                        continue;
                    }
                    block_list.blist.Add(get_block_position_by_block_index(index), block);
                }
                s.BaseStream.Seek(49126 - this._compressed_size, SeekOrigin.Current);
            }
            public void read(BlockList block_list, SMBinary s)
            {
                this._read_header(s);
                if (!has_valid_data)
                {
                    s.BaseStream.Seek(49126, SeekOrigin.Current);
                }
                else
                {
                    this._read_block_data(block_list, s);
                }
            }

            public int toUnsignedInt(byte[] bytes)
            {
                return toUnsignedInt(bytes, 0, bytes.Length);
            }
            public int toUnsignedInt(byte[] bytes, int o, int l)
            {
                long v = 0;
                for (int i = 0; i < l; i++)
                    v = (v << 8) | (uint)(bytes[o + i] & 0xff);
                return (int)v;
            }
            public int toUnsignedIntB(byte[] bytes)
            {
                return bytes[0] | bytes [1] << 8 | bytes[2] << 16;
            }
            public Vector3i get_block_position_by_block_index(int block_index)
            {
                var z = (int)(block_index / this._blocks_in_an_area);
                var rest = block_index % this._blocks_in_an_area;
                var y = (int)(rest / this._blocks_in_a_line);
                var x = rest % this._blocks_in_a_line;
                return new Vector3i(-(x + _position.x)*32*scale, (y + _position.y)*32 * scale, (z + _position.z)*32 * scale);
            }
    }
    }

    public class BlockList
    {
        public Dictionary<Vector3i, BlockBit> blist;

        public BlockList()
        {
            blist = new Dictionary<Vector3i, BlockBit>();
        }
    }

    internal class SMBinary : BinaryReader
    {

        public Vector3i read_vector_3_int32()
        {
            var x = this.ReadInt32();
            var y = this.ReadInt32();
            var z = this.ReadInt32();
            return new Vector3i(x, y, z);
        }

        public int ReadInt32(bool mode = false)
        {
            //if (mode)
            //{
                var data = base.ReadBytes(4);
                Array.Reverse(data);
                return BitConverter.ToInt32(data, 0);
           // }

            //return base.ReadInt32();
        }
        public override uint ReadUInt32()
        {
            var data = base.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToUInt32(data, 0);
        }
        public override long ReadInt64()
        {
            var data = base.ReadBytes(8);
            Array.Reverse(data);
            return BitConverter.ToInt64(data, 0);
        }
        public override ulong ReadUInt64()
        {
            var data = base.ReadBytes(8);
            Array.Reverse(data);
            return BitConverter.ToUInt64(data, 0);
        }

        public Tuple<int,int,int,int> read_vector_4_byte()
        {
            var x = this.ReadByte();
            var y = this.ReadByte();
            var z = this.ReadByte();
            var w = this.ReadByte();
            return new Tuple<int, int, int, int>(x, y, z, w);
        }

        public ushort ReadUInt16(bool v)
        {
            var data = base.ReadBytes(2);
            Array.Reverse(data);
            return BitConverter.ToUInt16(data, 0);
        }

        public SMBinary(Stream input) : base(input)
        {
        }
    }
    public static class BlockTypes
    {
        public static short CORE_ID = 1;

        public static short GLASS_WEDGE_ID = 329;
        public static short GLASS_CORNER_ID = 330;

        private static Dictionary<short, Vector3i> WC = new Dictionary<short, Vector3i>()
        {
             {599, SM_COLOR.GREY},
            {293, SM_COLOR.GREY},
            {311, SM_COLOR.GREY},

            //BLACK
            {604, SM_COLOR.BLACK},
            {296, SM_COLOR.BLACK},
            {312, SM_COLOR.BLACK},
            {594, SM_COLOR.BLACK},

            //WHITE
            {609, SM_COLOR.WHITE},
            {301, SM_COLOR.WHITE},
            {319, SM_COLOR.WHITE},
            {508, SM_COLOR.WHITE},

            //PURPLE
            {614, SM_COLOR.PURPLE},
            {294, SM_COLOR.PURPLE},
            {314, SM_COLOR.PURPLE},
            {538, SM_COLOR.PURPLE},

            //BLUE
            {619, SM_COLOR.BLUE},
            {298, SM_COLOR.BLUE},
            {315, SM_COLOR.BLUE},
            {533, SM_COLOR.BLUE},

            // GREEN +HAZARRD
            {624, SM_COLOR.GREEN},
            {299, SM_COLOR.GREEN},
            {316, SM_COLOR.GREEN},
            {528, SM_COLOR.GREEN},
            {439, SM_COLOR.GREEN},//haz

            //YELLOW +HAZARD
            {629, SM_COLOR.YELLOW},
            {300, SM_COLOR.YELLOW},
            {318, SM_COLOR.YELLOW},
            {523, SM_COLOR.YELLOW},
            {437, SM_COLOR.YELLOW},

            //ORANGE
            {634, SM_COLOR.ORANGE},
            {427, SM_COLOR.ORANGE},
            {432, SM_COLOR.ORANGE},
            {518, SM_COLOR.ORANGE},

            //RED
            {639, SM_COLOR.RED},
            {297, SM_COLOR.RED},
            {313, SM_COLOR.RED},
            {513, SM_COLOR.RED},

            //BROWN
            {644, SM_COLOR.BROWN},
            {295, SM_COLOR.BROWN},
            {317, SM_COLOR.BROWN},
            {691, SM_COLOR.BROWN},


            //DARK GREY
            {829, SM_COLOR.DARK_GREY},
            {819, SM_COLOR.DARK_GREY},
            {824, SM_COLOR.DARK_GREY},

            //TEAL
            {879, SM_COLOR.TEAL},
            {869, SM_COLOR.TEAL},
            {874, SM_COLOR.TEAL},
            {884, SM_COLOR.TEAL},

            //PINK
            {913, SM_COLOR.PINK},
            {903, SM_COLOR.PINK},
            {908, SM_COLOR.PINK},
            {918, SM_COLOR.PINK}
        };
        private static Dictionary<short, Vector3i> BC = new Dictionary<short, Vector3i>()
        {
               //GREYS
            {598, SM_COLOR.GREY},
            {5, SM_COLOR.GREY},
            {263, SM_COLOR.GREY},

            //BLACK
            {603, SM_COLOR.BLACK},
            {75, SM_COLOR.BLACK},
            {264, SM_COLOR.BLACK},
            {593, SM_COLOR.BLACK},

            //WHITE
            {608, SM_COLOR.WHITE},
            {81, SM_COLOR.WHITE},
            {271, SM_COLOR.WHITE},
            {507, SM_COLOR.WHITE},

            //PURPLE
            {613, SM_COLOR.PURPLE},
            {69, SM_COLOR.PURPLE},
            {266, SM_COLOR.PURPLE},
            {537, SM_COLOR.PURPLE},

            //BLUE
            {618, SM_COLOR.BLUE},
            {77, SM_COLOR.BLUE},
            {267, SM_COLOR.BLUE},
            {532, SM_COLOR.BLUE},

            // GREEN +HAZARRD
            {623, SM_COLOR.GREEN},
            {78, SM_COLOR.GREEN},
            {268, SM_COLOR.GREEN},
            {527, SM_COLOR.GREEN},
            {438, SM_COLOR.GREEN},//haz

            //YELLOW +HAZARD
            {628, SM_COLOR.YELLOW},
            {79, SM_COLOR.YELLOW},
            {270, SM_COLOR.YELLOW},
            {522, SM_COLOR.YELLOW},
            {436, SM_COLOR.YELLOW},

            //ORANGE
            {633, SM_COLOR.ORANGE},
            {426, SM_COLOR.ORANGE},
            {431, SM_COLOR.ORANGE},
            {517, SM_COLOR.ORANGE},

            //RED
            {638, SM_COLOR.RED},
            {76, SM_COLOR.RED},
            {265, SM_COLOR.RED},
            {512, SM_COLOR.RED},

            //BROWN
            {643, SM_COLOR.BROWN},
            {70, SM_COLOR.BROWN},
            {269, SM_COLOR.BROWN},
            {690, SM_COLOR.BROWN},
         

            //DARK GREY
            {828, SM_COLOR.DARK_GREY},
            {818, SM_COLOR.DARK_GREY},
            {823, SM_COLOR.DARK_GREY},

            //TEAL
            {878, SM_COLOR.TEAL},
            {868, SM_COLOR.TEAL},
            {873, SM_COLOR.TEAL},
            {883, SM_COLOR.TEAL},

            //PINK
            {912, SM_COLOR.PINK},
            {902, SM_COLOR.PINK},
            {907, SM_COLOR.PINK},
            {917, SM_COLOR.PINK}
        };
        private static Dictionary<short, Vector3i> HC = new Dictionary<short, Vector3i>()
        {
            //GREYS
            {601, SM_COLOR.GREY},
            {357, SM_COLOR.GREY},
            {401, SM_COLOR.GREY},

            //BLACK
            {606, SM_COLOR.BLACK},
            {385, SM_COLOR.BLACK},
            {369, SM_COLOR.BLACK},
            {596, SM_COLOR.BLACK},

            //WHITE
            {611, SM_COLOR.WHITE},
            {392, SM_COLOR.WHITE},
            {376, SM_COLOR.WHITE},
            {510, SM_COLOR.WHITE},

            //PURPLE
            {616, SM_COLOR.PURPLE},
            {387, SM_COLOR.PURPLE},
            {371, SM_COLOR.PURPLE},
            {540, SM_COLOR.PURPLE},

            //BLUE
            {621, SM_COLOR.BLUE},
            {388, SM_COLOR.BLUE},
            {372, SM_COLOR.BLUE},
            {535, SM_COLOR.BLUE},

            // GREEN +HAZARRD
            {626, SM_COLOR.GREEN},
            {389, SM_COLOR.GREEN},
            {373, SM_COLOR.GREEN},
            {530, SM_COLOR.GREEN},
            {652, SM_COLOR.GREEN},//haz

            //YELLOW +HAZARD
            {631, SM_COLOR.YELLOW},
            {391, SM_COLOR.YELLOW},
            {375, SM_COLOR.YELLOW},
            {525, SM_COLOR.YELLOW},
            {649, SM_COLOR.YELLOW},//HAZ

            //ORANGE
            {636, SM_COLOR.ORANGE},
            {429, SM_COLOR.ORANGE},
            {434, SM_COLOR.ORANGE},
            {520, SM_COLOR.ORANGE},

            //RED
            {641, SM_COLOR.RED},
            {386, SM_COLOR.RED},
            {370, SM_COLOR.RED},
            {515, SM_COLOR.RED},

            //BROWN
            {646, SM_COLOR.BROWN},
            {403, SM_COLOR.BROWN},
            {374, SM_COLOR.BROWN},
            {693, SM_COLOR.BROWN},


            //DARK GREY
            {831, SM_COLOR.DARK_GREY},
            {821, SM_COLOR.DARK_GREY},
            {826, SM_COLOR.DARK_GREY},

            //TEAL
            {881, SM_COLOR.TEAL},
            {871, SM_COLOR.TEAL},
            {876, SM_COLOR.TEAL},
            {886, SM_COLOR.TEAL},

            //PINK
            {915, SM_COLOR.PINK},
            {905, SM_COLOR.PINK},
            {910, SM_COLOR.PINK},
            {920, SM_COLOR.PINK}
        };
        private static Dictionary<short, Vector3i> TC = new Dictionary<short, Vector3i>()
        {
            //GREYS
            {602, SM_COLOR.GREY},
            {348, SM_COLOR.GREY},
            {402, SM_COLOR.GREY},

            //BLACK
            {607, SM_COLOR.BLACK},
            {393, SM_COLOR.BLACK},
            {377, SM_COLOR.BLACK},
            {597, SM_COLOR.BLACK},

            //WHITE
            {612, SM_COLOR.WHITE},
            {400, SM_COLOR.WHITE},
            {384, SM_COLOR.WHITE},
            {511, SM_COLOR.WHITE},

            //PURPLE
            {617, SM_COLOR.PURPLE},
            {395, SM_COLOR.PURPLE},
            {379, SM_COLOR.PURPLE},
            {541, SM_COLOR.PURPLE},

            //BLUE
            {622, SM_COLOR.BLUE},
            {396, SM_COLOR.BLUE},
            {380, SM_COLOR.BLUE},
            {536, SM_COLOR.BLUE},

            // GREEN +HAZARRD
            {627, SM_COLOR.GREEN},
            {397, SM_COLOR.GREEN},
            {381, SM_COLOR.GREEN},
            {531, SM_COLOR.GREEN},
            {653, SM_COLOR.GREEN},//haz

            //YELLOW +HAZARD
            {632, SM_COLOR.YELLOW},
            {398, SM_COLOR.YELLOW},
            {383, SM_COLOR.YELLOW},
            {526, SM_COLOR.YELLOW},
            {650, SM_COLOR.YELLOW},//HAZ

            //ORANGE
            {637, SM_COLOR.ORANGE},
            {430, SM_COLOR.ORANGE},
            {435, SM_COLOR.ORANGE},
            {521, SM_COLOR.ORANGE},

            //RED
            {642, SM_COLOR.RED},
            {394, SM_COLOR.RED},
            {378, SM_COLOR.RED},
            {516, SM_COLOR.RED},

            //BROWN
            {647, SM_COLOR.BROWN},
            {404, SM_COLOR.BROWN},
            {382, SM_COLOR.BROWN},
            {694, SM_COLOR.BROWN},


            //DARK GREY
            {832, SM_COLOR.DARK_GREY},
            {822, SM_COLOR.DARK_GREY},
            {827, SM_COLOR.DARK_GREY},

            //TEAL
            {882, SM_COLOR.TEAL},
            {872, SM_COLOR.TEAL},
            {877, SM_COLOR.TEAL},
            {887, SM_COLOR.TEAL},

            //PINK
            {916, SM_COLOR.PINK},
            {906, SM_COLOR.PINK},
            {911, SM_COLOR.PINK},
            {921, SM_COLOR.PINK},
        };
        private static Dictionary<short, Vector3i> CC = new Dictionary<short, Vector3i>()
        {
             //GREYS
            {600, SM_COLOR.GREY},
            {302, SM_COLOR.GREY},
            {320, SM_COLOR.GREY},

            //BLACK
            {605, SM_COLOR.BLACK},
            {305, SM_COLOR.BLACK},
            {321, SM_COLOR.BLACK},
            {595, SM_COLOR.BLACK},

            //WHITE
            {610, SM_COLOR.WHITE},
            {310, SM_COLOR.WHITE},
            {328, SM_COLOR.WHITE},
            {509, SM_COLOR.WHITE},

            //PURPLE
            {615, SM_COLOR.PURPLE},
            {303, SM_COLOR.PURPLE},
            {323, SM_COLOR.PURPLE},
            {539, SM_COLOR.PURPLE},

            //BLUE
            {620, SM_COLOR.BLUE},
            {307, SM_COLOR.BLUE},
            {324, SM_COLOR.BLUE},
            {534, SM_COLOR.BLUE},

            // GREEN +HAZARRD
            {625, SM_COLOR.GREEN},
            {308, SM_COLOR.GREEN},
            {325, SM_COLOR.GREEN},
            {529, SM_COLOR.GREEN},
            {651, SM_COLOR.GREEN},//haz

            //YELLOW +HAZARD
            {630, SM_COLOR.YELLOW},
            {309, SM_COLOR.YELLOW},
            {327, SM_COLOR.YELLOW},
            {524, SM_COLOR.YELLOW},
            {648, SM_COLOR.YELLOW},//HAZ

            //ORANGE
            {635, SM_COLOR.ORANGE},
            {428, SM_COLOR.ORANGE},
            {433, SM_COLOR.ORANGE},
            {519, SM_COLOR.ORANGE},

            //RED
            {640, SM_COLOR.RED},
            {306, SM_COLOR.RED},
            {322, SM_COLOR.RED},
            {514, SM_COLOR.RED},

            //BROWN
            {645, SM_COLOR.BROWN},
            {304, SM_COLOR.BROWN},
            {326, SM_COLOR.BROWN},
            {692, SM_COLOR.BROWN},


            //DARK GREY
            {830, SM_COLOR.DARK_GREY},
            {820, SM_COLOR.DARK_GREY},
            {825, SM_COLOR.DARK_GREY},

            //TEAL
            {880, SM_COLOR.TEAL},
            {870, SM_COLOR.TEAL},
            {875, SM_COLOR.TEAL},
            {885, SM_COLOR.TEAL},

            //PINK
            {914, SM_COLOR.PINK},
            {904, SM_COLOR.PINK},
            {909, SM_COLOR.PINK},
            {919, SM_COLOR.PINK},
        };

        private static int indexOf(short[] array, short val)
        {
            for (int i = 0; i < array.Length; i++)
                if (array[i] == val)
                    return i;
            return -1;
        }

        public static bool isHull(short blockID)
        {
            return BC.ContainsKey(blockID);
        }

        public static bool isWedge(short blockID)
        {
            return WC.ContainsKey(blockID);
        }

        public static bool isCorner(short blockID)
        {
            return CC.ContainsKey(blockID);
        }

        public static bool isHepta(short blockID)
        {
            return HC.ContainsKey(blockID);
        }

        public static bool isTetra(short blockID)
        {
            return TC.ContainsKey(blockID);
        }

        public static bool isAnyHull(short blockID)
        {
            return isHull(blockID) || isCorner(blockID) || isWedge(blockID) || isHepta(blockID) || isTetra(blockID);
        }

        public static int Sevo_ID(short blockID)
        {
            if (isHull(blockID))
                return 196;
            if (isCorner(blockID))
                return 199;
            if (isWedge(blockID))
                return 197;
            if (isHepta(blockID))
                return 200;
            if (isTetra(blockID))
                return 198;
            return 0;
        }

        public static Vector3i Sevo_Paint(short blockID)
        {
            if (isHull(blockID))
                return BC[blockID];
            if (isCorner(blockID))
                return CC[blockID];
            if (isWedge(blockID))
                return WC[blockID];
            if (isHepta(blockID))
                return HC[blockID];
            if (isTetra(blockID))
                return TC[blockID];
            return new Vector3i(0,0,0);
        }
        public struct SM_COLOR
        {
            public static Vector3i GREY = new Vector3i(100, 103, 105);
            public static Vector3i BLACK = new Vector3i(10, 10, 12);
            public static Vector3i WHITE = new Vector3i(220, 220, 220);
            public static Vector3i PURPLE = new Vector3i(148, 10, 196);
            public static Vector3i BLUE = new Vector3i(10, 84, 196);
            public static Vector3i GREEN = new Vector3i(69, 177, 42);
            public static Vector3i YELLOW = new Vector3i(196, 172, 10);
            public static Vector3i ORANGE = new Vector3i(196, 68, 10);
            public static Vector3i RED = new Vector3i(196, 10, 10);
            public static Vector3i BROWN = new Vector3i(142, 75, 49);
            public static Vector3i DARK_GREY = new Vector3i(80, 82, 84);
            public static Vector3i TEAL = new Vector3i(10, 196, 140);
            public static Vector3i PINK = new Vector3i(196, 10, 150);
        }
    }
}

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
using testapp1;
using SeekOrigin = System.IO.SeekOrigin;

namespace EvoEditApp
{
    class SmLoad
    {
        public string Name;
        public Smd3 Smd;
        public int V;
        public int Scale;

        public SmLoad(int s = 1)
        {
            Scale = s;
            V = 0;
            Smd = new Smd3(s:Scale);
        }
        public List<Vector3i> Dump()
        {
            return Smd.GetBlockList().Blist.Keys.ToList();
        }
        public Dictionary<Vector3i,BlockBit> DumpData()
        {
            return Smd.GetBlockList().Blist;
        }

        public void Read(DirectoryInfo directoryBp)
        {
            Name = directoryBp.Name;
            Smd.ReadFolder(directoryBp);
        }
    }

    public class Smd3
    {
        private int _segmentsInLineRegion;
        private int _blocksInLineSegment;
        private Vector3i _positionCore;
        private BlockList _blist;
        private readonly int _scale;
        public int V;
        public BlockList GetBlockList()
        {
            return _blist;
        }

        public Tuple<Vector3i,Vector3i> get_min_max_vector()
        {
            var minT = new Vector3i(16, 16, 16);
            var maxT = new Vector3i(16, 16, 16);

            foreach (Vector3i v in _blist.Blist.Keys)
            {
                minT = Vector3i.Min(minT, v);
                maxT = Vector3i.Max(maxT, v);
            }

            return new Tuple<Vector3i, Vector3i>(minT, maxT);
        }
        public Smd3(int a = 16,int b = 32,int s = 1)
        {
            V = 0;
            _segmentsInLineRegion = a;
            _blocksInLineSegment = b;
            _positionCore = new Vector3i(16, 16, 16);
            _blist = new BlockList();
            this._scale = s;
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
                    V = 3;
                    var smdRegion = new SmdRegion(s:_scale);
                    foreach (var fileName in fileList)
                    {
                        smdRegion.Read(fileName, _blist);
                    }

                    break;
                }
                case ".smd2":
                {
                    V = 2;
                    Console.WriteLine(@"'smd2' file format found");
                    var smd2 = new Smd2(s:_scale);
                    smd2.Read(directoryBp); Console.WriteLine(@"Finished Read");
                    this._blist = smd2.GetBlockList();
                    break;
                }
                default:
                    throw new Exception("Unknown smd format");
            }
        }



        public void Read(DirectoryInfo directoryBp)
        {
            var directoryData = new DirectoryInfo(System.IO.Path.Combine(directoryBp.FullName, "DATA"));
            //directory_data = os.path.join(directory_blueprint, "DATA");
            var fileList = directoryData.GetFiles().OrderBy( fi => fi.Name).ToList();
            if (fileList.Count() < 0)
            {
                Console.WriteLine("no smd files found");
                return;
            }
            //assert len(file_list) > 0, "No smd files found"
            var fName = fileList[0];
            if ((fName.Attributes & FileAttributes.Directory) != 0 && fName.Name.StartsWith("ATTACHED"))
            {
                var dp = new DirectoryInfo(fName.FullName);
                fileList = dp.GetFiles().OrderBy(fi => fi.Name).ToList();
                if (fileList.Count() < 0)
                {
                    Console.WriteLine("no smd files found");
                    return;
                }
                fName = fileList[0];
            }
            if (fName.Extension.Equals(".smd3"))
            {
               V = 3;
                var smdRegion =new SmdRegion();
                foreach(var fileName in fileList)
                {
                    //this._file_name_prefix, x, y, z = os.path.splitext(file_name)[0].rsplit('.', 3) TODO CHECK IF THIS COMES UP AGAIN
                    smdRegion.Read(fileName, _blist);
                }
            }
            else if (fName.Extension.Equals(".smd2"))
            {
                V = 2;
                Console.WriteLine("'smd2' file format found");
                var smd2 = new Smd2();
                smd2.Read(directoryBp); Console.WriteLine("done");
                //var offset = (8, 8, 8);
                this._blist = smd2.GetBlockList();
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
        private readonly int _int24;
        private readonly int _version;
        public bool Visited; 

        public BlockBit(int i, int v, bool u = false)
        {
            _int24 = i;
            _version = v;
            Visited = u;
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

        private int get_axis_rotation()
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

        private int get_rotations()
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
            if (BlockTypes.IsCorner(id))
                return get_sevo_corners2(rot, axis);
            //  Console.WriteLine("fucked");

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
                            return 23;//6//FUCKING HEPTA (NO 7)22 
                        case 2:
                            if (BlockTypes.IsWedge(id)) return 5;
                            return 8; //5
                        case 3:
                            if (BlockTypes.IsWedge(id)) return 4;
                            return 17;//4
                        default:
                            if (BlockTypes.IsWedge(id)) return 7;
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
        private Tuple<int, int, int, int> _version;
        private readonly int _segmentsInACube;
        private readonly int _segmentsInAnArea;
        private readonly int _segmentsInALine;
        private readonly int _blocksInALineInASegment;
        private readonly int _scale;
        public SmdRegion(int segmentsInALine= 16, int blocksInALine= 32,int s =1)
        {
            this._blocksInALineInASegment = blocksInALine;
            this._segmentsInALine = segmentsInALine;
            this._segmentsInAnArea = this._segmentsInALine * this._segmentsInALine;
            this._segmentsInACube = this._segmentsInAnArea * this._segmentsInALine;
            this._version = new Tuple<int,int,int,int>(3, 0, 0, 0);
            this._scale = s;
            //this.position_to_segment = { }
            //this.position_to_region = new Dictionary<int, SmdRegion>();
            // this._block_list = new BlockList();
        }

        private static bool _isEOF(SmBinary s)
        {
            if (s.BaseStream.Position < s.BaseStream.Length)
                return true;
            s.BaseStream.Seek(-1, SeekOrigin.Current);
            return false;
        }

        private Tuple<ushort, ushort> _read_segment_index(SmBinary s)
        {
            var indentifier = s.ReadUInt16(true);
            var size = s.ReadUInt16(true);
            return new Tuple<ushort, ushort>(indentifier, size);
        }

        private Dictionary<ushort, ushort> _read_region_header(SmBinary s)
        {

            //this._version = //
            this._version = s.read_vector_4_byte();
            var segmentIdToSize = new Dictionary<ushort, ushort>();
            for (var i = 0; i < this._segmentsInACube; i++)
            {
                this._read_segment_index(s).Deconstruct(out ushort identifier, out ushort size);
                if (identifier > 0)
                {
                    segmentIdToSize[identifier] = size;
                }
               
            }
            return segmentIdToSize;
        }
        private void _read_file(BlockList blockList, SmBinary s)
        {
            var segmentIdToSize = this._read_region_header(s);
            ushort segmentId = 0;
            var segement = new SmdSegment(this._blocksInALineInASegment,_scale);
            while (_isEOF(s))
            {
                segmentId += 1;
                if (!segmentIdToSize.ContainsKey(segmentId))
                {
                    s.BaseStream.Seek(49152, SeekOrigin.Current);
                    continue;
                }
                if (segmentIdToSize[segmentId] == 0)
                {
                    s.BaseStream.Seek(49152, SeekOrigin.Current);
                    continue;
                }
                segement.Read(blockList, s);
            }
        }

        public void Read(FileInfo file, BlockList blockList)
        {
            Console.WriteLine($"Reading file {file.Name}");
            using (FileStream f = new FileStream(file.FullName, FileMode.Open))
            {
                this._read_file(blockList, new SmBinary(f));
                Console.WriteLine("read1");
            }
        }

        internal class SmdSegment
        {
            private readonly int _blocksInALine;
            private readonly int _blocksInAnArea;
            private int _version;
            private readonly int _scale;
            private ulong _timestamp;
            private Vector3i _position;
            private bool _hasValidData;
            private uint _compressedSize;
          

            public SmdSegment(int blocksInALine = 32,int s = 1)
            {
                this._blocksInALine = blocksInALine;
                this._blocksInAnArea = this._blocksInALine * this._blocksInALine;
                this._version = 2;
                this._timestamp = 0;
                this._position = null;
                this._hasValidData = false;
                this._compressedSize = 0;
                this._scale = (int)(4 * Math.Pow(2, s));
                // this.block_index_to_block = { }
            }
         
            private void _read_header(SmBinary s)
            {
               
                this._version = Math.Abs((int)((int)s.ReadByte()));
                if (_version > 3 || _version < 2)
                {
                    Console.WriteLine($@"unsupported version: {this._version}");
                }
                this._timestamp = s.ReadUInt64();
                this._position = s.read_vector_3_int32();
                this._hasValidData = s.ReadBoolean();
                this._compressedSize = s.ReadUInt32();

            }
            private void _read_block_data(BlockList blockList, SmBinary s)
            {
                try
                {
                    var decompressedData = SmBinary.DecompressZlib(new MemoryStream(s.ReadBytes((int)this._compressedSize)));
                    for (var index = 0; index < (int)(decompressedData.Length / 3); index++)
                    {
                        var pos = index * 3;
                        byte[] padded = new byte[3];
                        var int24 = 0;
                        Array.Copy(decompressedData, pos, padded, 0, 3);
                        int24 = this._version < 3 ? SmBinary.ToUnsignedInt(padded) : SmBinary.ToUnsignedIntB(padded);
                        if (int24 == 0) continue;
                        var block = new BlockBit(int24, this._version);
                        if (!BlockTypes.IsAnyHull((short)block.get_id()))
                        {
                            continue;
                        }
                        blockList.Blist.Add(get_block_position_by_block_index(index), block);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(@"Decompression Failed, skipping Sector");
                }
                finally
                {
                    s.BaseStream.Seek(49126 - this._compressedSize, SeekOrigin.Current);
                }
                
             
            }
            public void Read(BlockList blockList, SmBinary s)
            {
                this._read_header(s);
                if (!_hasValidData)
                {
                    s.BaseStream.Seek(49126, SeekOrigin.Current);
                }
                else
                {
                    this._read_block_data(blockList, s);
                }
            }

            public Vector3i get_block_position_by_block_index(int blockIndex)
            {
                var z = (int)(blockIndex / this._blocksInAnArea);
                var rest = blockIndex % this._blocksInAnArea;
                var y = (int)(rest / this._blocksInALine);
                var x = rest % this._blocksInALine;
                return new Vector3i(-(x + _position.X)*_scale, (y + _position.Y)* _scale, (z + _position.Z)* _scale);
            }
    }
    }

    public class BlockList
    {
        public Dictionary<Vector3i, BlockBit> Blist;

        public BlockList()
        {
            Blist = new Dictionary<Vector3i, BlockBit>();
        }
    }

    internal class SmBinary : BinaryReader
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

        public static int ToUnsignedInt(byte[] bytes)
        {
            return ToUnsignedInt(bytes, 0, bytes.Length);
        }

        public static int ToUnsignedIntB(byte[] bytes)
        {
            return bytes[0] | bytes[1] << 8 | bytes[2] << 16;
        }

        public static int ToUnsignedInt(byte[] bytes, int o, int l)
        {
            long v = 0;
            for (var i = 0; i < l; i++)
                v = (v << 8) | (uint)(bytes[o + i] & 0xff);
            return (int)v;
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

        public SmBinary(Stream input) : base(input)
        {
        }
    }
   
}

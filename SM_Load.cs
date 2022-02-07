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

        public SmLoad(int s = 1,bool slabs = false)
        {
            Scale = s;
            V = 0;
            Smd = new Smd3(s:Scale,slabflag:slabs);
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
        private bool _slabflag;
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

        public Smd3(int a = 16, int b = 32, int s = 1, bool slabflag = false)
        {
            V = 0;
            _segmentsInLineRegion = a;
            _blocksInLineSegment = b;
            _positionCore = new Vector3i(16, 16, 16);
            _blist = new BlockList();
            this._scale = s;
            this._slabflag = slabflag;
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
                    var smdRegion = new SmdRegion(s:_scale,_slabflag:this._slabflag);
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
    }



internal class SmdRegion
    {
        private Tuple<int, int, int, int> _version;
        private readonly int _segmentsInACube;
        private readonly int _segmentsInAnArea;
        private readonly int _segmentsInALine;
        private readonly int _blocksInALineInASegment;
        private readonly int _scale;
        private readonly bool slabflag;
        public SmdRegion(int segmentsInALine= 16, int blocksInALine= 32,int s =1, bool _slabflag = false)
        {
            this._blocksInALineInASegment = blocksInALine;
            this._segmentsInALine = segmentsInALine;
            this._segmentsInAnArea = this._segmentsInALine * this._segmentsInALine;
            this._segmentsInACube = this._segmentsInAnArea * this._segmentsInALine;
            this._version = new Tuple<int,int,int,int>(3, 0, 0, 0);
            this._scale = s;
            this.slabflag = _slabflag;
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
            var segement = new SmdSegment(this._blocksInALineInASegment,_scale,slabflag);
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
            private bool _slabflag;
          

            public SmdSegment(int blocksInALine = 32,int s = 1, bool _slabflag = false)
            {
                this._blocksInALine = blocksInALine;
                this._blocksInAnArea = this._blocksInALine * this._blocksInALine;
                this._version = 2;
                this._timestamp = 0;
                this._position = null;
                this._hasValidData = false;
                this._compressedSize = 0;
                this._scale = (int)(4 * Math.Pow(2, s));
                this._slabflag = _slabflag;
                // this.block_index_to_block = { }
            }
         
            private void _read_header(SmBinary s)
            {
               
                this._version = Math.Abs((int)((int)s.ReadByte()));
                //if (_version > 3 || _version < 2)
              //  {
                //    Console.WriteLine($@"unsupported version: {this._version}");
               // }
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
                       // Console.WriteLine((short)block.get_id());

                       if (BlockTypes.IsSlab((short)block.get_id()) && _slabflag) continue;
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

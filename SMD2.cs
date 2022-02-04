using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using testapp1;

namespace EvoEditApp
{

    public class Smd2
    {
        private readonly int _blocksInALineInASegment;
        private readonly int _segmentsInALineOfARegion;
        private BlockList _blockList;
        private int _scale;
        private readonly Dictionary<int, SmdRegion> _positionToRegion;

        public Smd2(int segmentsInALineOfARegion = 16, int blocksInALineOfASegment = 16,int s = 1)
        {
            this._blocksInALineInASegment = blocksInALineOfASegment;
            this._segmentsInALineOfARegion = segmentsInALineOfARegion;
            this._positionToRegion = new Dictionary<int, SmdRegion>();
            this._blockList = new BlockList();
            this._scale = s;
        }
        public BlockList GetBlockList()
        {
            return _blockList;
        }


        public void Read(DirectoryInfo directoryBp)
        {
            if (!directoryBp.Name.StartsWith("ATTACHED") && !directoryBp.Name.StartsWith("DATA")) return;

            var fileList = directoryBp.GetFiles().OrderBy(fi => fi.Name).ToList();
            if (fileList.Count() < 0)
            {
                Console.WriteLine(@"No smd files found");
                return;
            }
            var fName = fileList[0];
            // var directory_data = new DirectoryInfo(System.IO.Path.Combine(directoryBp.FullName, "DATA"));
            //directory_data = os.path.join(directory_blueprint, "DATA");
            //var fileList = directory_data.GetFiles().OrderBy(fi => fi.Name).ToList();

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
            if (fileList.Count() < 0)
            {
                Console.WriteLine("no smd files found");
                return;
            }
            //assert len(file_list) > 0, "No smd files found"
            var smdRegion = new SmdRegion(s:_scale);
            foreach (var fileName in fileList)
            {
                if (fileName.Extension != ".smd2")
                {
                    throw new Exception($"Unexpected file extension format {fileName.Extension}");
                }
                smdRegion.Read(fileName, this._blockList);
            }

        }

        private class SmdRegion
        {
            private readonly int _blocksInALineInASegment;
            private readonly int _segmentsInALine;
            private readonly int _segmentsInAnArea;
            private readonly int _segmentsInACube;
            private readonly int _segmentHeaderSize;
            private readonly int _scale;
            private Tuple<int, int, int, int> _version;
            private Dictionary<Tuple<int>, SmdSegment> _positionToSegment;
            public SmdRegion(int segmentsInALine = 16, int blocksInALine = 16,int s = 1)
            {
                this._blocksInALineInASegment = blocksInALine;
                this._segmentsInALine = segmentsInALine;
                this._segmentsInAnArea = this._segmentsInALine * this._segmentsInALine;
                this._segmentsInACube = this._segmentsInAnArea * this._segmentsInALine;
                this._version = new Tuple<int, int, int, int>(0, 0, 0, 0);
                this._positionToSegment = new Dictionary<Tuple<int>, SmdSegment>();
                this._segmentHeaderSize = 26;
                this._scale = s;
                if (this._version.Item4 == 0)
                {
                    this._segmentHeaderSize = 25;
                }
            }

            private static bool _isEOF(SmBinary s)
            {
                if (s.BaseStream.Position < s.BaseStream.Length)
                    return true;
                s.BaseStream.Seek(-1, SeekOrigin.Current);
                return false;
            }

            private Tuple<int, int> _read_segment_index(SmBinary s)
            {
                var indentifier = s.ReadInt32(true);
                var size = s.ReadInt32(true);
                return new Tuple<int, int>(indentifier, size);
            }

            private Dictionary<int, int> _read_region_header(SmBinary s)
            {

                //this._version = //
                this._version = s.read_vector_4_byte();
                var segmentIdToSize = new Dictionary<int, int>();
                for (var i = 0; i < this._segmentsInACube; i++)
                {
                    this._read_segment_index(s).Deconstruct(out int identifier, out int size);
                    if (identifier == -1) continue;
                    segmentIdToSize[identifier] = size;
                }

                for (var i = 0; i < this._segmentsInACube; i++)
                {
                    s.ReadUInt64();
                }

                return segmentIdToSize;
            }

            private void _read_file(BlockList blockList, SmBinary s)
            {
                var segmentIdToSize = this._read_region_header(s);
                var segmentId = -1;
                var segement = new SmdSegment(this._version.Item4, this._blocksInALineInASegment,_scale);
                while (_isEOF(s))
                {
                    segmentId += 1;
                    if (!segmentIdToSize.ContainsKey(segmentId))
                    {
                        s.BaseStream.Seek(5120, SeekOrigin.Current);
                        continue;
                    }
                    if (segmentIdToSize[segmentId] == 0)
                    {
                        s.BaseStream.Seek(5120, SeekOrigin.Current);
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

            private class SmdSegment
            {
                private int _blocksInALine;
                private int _blocksInAnArea;
                private int _blocksInACube;
                private int _regionVersion;
                private int _version;
                private ulong _timestamp;
                private Vector3i _position;
                private bool _hasValidData;
                private uint _compressedSize;
                private int _headerSize;
                private readonly int _dataSize;
                private readonly int _scale;

                public SmdSegment(int version, int blocksInALine = 16,int s = 1)
                {
                    this._blocksInALine = blocksInALine;
                    this._blocksInAnArea = this._blocksInALine * this._blocksInALine;
                    this._blocksInACube = this._blocksInAnArea * this._blocksInALine;
                    this._regionVersion = version;
                    this._version = 0;
                    this._timestamp = 0;
                    this._position = null;
                    this._hasValidData = false;
                    this._compressedSize = 0;
                    this._scale = (int)(4*Math.Pow(2,s));
                    // this.block_index_to_block = { }
                    this._headerSize = 26;
                    if (this._regionVersion == 0)
                    {
                        this._headerSize = 25;
                    }
                    this._dataSize = 5120 - this._headerSize;
                }
                
                private void _read_header(SmBinary s)
                {
                    if (this._regionVersion != 0)
                        this._version = Math.Abs((int)(((int)s.ReadByte()) - Math.Pow(2, 8)));
                    //this._version = Math.Abs((int)s.ReadByte());
                    this._timestamp = s.ReadUInt64();
                    this._position = s.read_vector_3_int32();
                    this._hasValidData = s.ReadBoolean();
                    this._compressedSize = s.ReadUInt32();
                }

                private void _read_block_data(BlockList blockList, SmBinary s)
                {
                    try
                    {
                        var decompressedData =
                            SmBinary.DecompressZlib(new MemoryStream(s.ReadBytes((int)this._compressedSize)));
                        for (int index = 0; index < (int)(decompressedData.Length / 3); index++)
                        {
                            var pos = index * 3;
                            byte[] padded = new byte[3];
                            Array.Copy(decompressedData, pos, padded, 0, 3);
                            var int24 = SmBinary.ToUnsignedInt(padded);
                            if (int24 == 0) continue;
                            BlockBit block = new BlockBit(int24, this._version);
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
                        s.BaseStream.Seek(this._dataSize - this._compressedSize, SeekOrigin.Current);
                    }
                }

                public void Read(BlockList blockList, SmBinary s)
                {
                    this._read_header(s);
                    if (!_hasValidData)
                    {
                        s.BaseStream.Seek(this._dataSize, SeekOrigin.Current);
                    }
                    else
                    {
                        this._read_block_data(blockList, s);
                    }
                    if (this._hasValidData && blockList.Blist.Count == 0)
                        this._hasValidData = false;
                }

                private Vector3i get_block_position_by_block_index(int blockIndex)
                {
                    var z = (int)(blockIndex / this._blocksInAnArea);
                    var rest = blockIndex % this._blocksInAnArea;
                    var y = (int)(rest / this._blocksInALine);
                    var x = rest % this._blocksInALine;
                    return new Vector3i(-(x + _position.X)*_scale, (y + _position.Y) * _scale, (z + _position.Z) * _scale);
                }
            }
        }
    }

}
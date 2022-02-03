using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace EvoEditApp
{

    public class Smd2
    {
        private readonly int _blocks_in_a_line_in_a_segment;
        private readonly int _segments_in_a_line_of_a_region;
        private BlockList _block_list;
        private int scale;
        private readonly Dictionary<int, SmdRegion> position_to_region;

        public Smd2(int segments_in_a_line_of_a_region = 16, int blocks_in_a_line_of_a_segment = 16,int s = 1)
        {
            this._blocks_in_a_line_in_a_segment = blocks_in_a_line_of_a_segment;
            this._segments_in_a_line_of_a_region = segments_in_a_line_of_a_region;
            this.position_to_region = new Dictionary<int, SmdRegion>();
            this._block_list = new BlockList();
            this.scale = s;
        }
        public BlockList GetBlockList()
        {
            return _block_list;
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
            var smd_region = new SmdRegion(s:scale);
            foreach (var file_name in fileList)
            {
                if (file_name.Extension != ".smd2")
                {
                    throw new Exception($"Unexpected file extension format {file_name.Extension}");
                }
                smd_region.read(file_name, this._block_list);
            }

        }

        private class SmdRegion
        {
            private readonly int _blocks_in_a_line_in_a_segment;
            private readonly int _segments_in_a_line;
            private readonly int _segments_in_an_area;
            private readonly int _segments_in_a_cube;
            private readonly int _segment_header_size;
            private readonly int scale;
            private Tuple<int, int, int, int> _version;
            private Dictionary<Tuple<int>, SmdSegment> _position_to_segment;
            public SmdRegion(int segments_in_a_line = 16, int blocks_in_a_line = 16,int s = 1)
            {
                this._blocks_in_a_line_in_a_segment = blocks_in_a_line;
                this._segments_in_a_line = segments_in_a_line;
                this._segments_in_an_area = this._segments_in_a_line * this._segments_in_a_line;
                this._segments_in_a_cube = this._segments_in_an_area * this._segments_in_a_line;
                this._version = new Tuple<int, int, int, int>(0, 0, 0, 0);
                this._position_to_segment = new Dictionary<Tuple<int>, SmdSegment>();
                this._segment_header_size = 26;
                this.scale = s;
                if (this._version.Item4 == 0)
                {
                    this._segment_header_size = 25;
                }
            }

            private static bool _isEOF(SMBinary s)
            {
                if (s.BaseStream.Position < s.BaseStream.Length)
                    return true;
                s.BaseStream.Seek(-1, SeekOrigin.Current);
                return false;
            }

            private Tuple<int, int> _read_segment_index(SMBinary s)
            {
                var indentifier = s.ReadInt32(true);
                var size = s.ReadInt32(true);
                return new Tuple<int, int>(indentifier, size);
            }

            private Dictionary<int, int> _read_region_header(SMBinary s)
            {

                //this._version = //
                this._version = s.read_vector_4_byte();
                var segment_id_to_size = new Dictionary<int, int>();
                for (var i = 0; i < this._segments_in_a_cube; i++)
                {
                    this._read_segment_index(s).Deconstruct(out int identifier, out int size);
                    if (identifier == -1) continue;
                    segment_id_to_size[identifier] = size;
                }

                for (var i = 0; i < this._segments_in_a_cube; i++)
                {
                    s.ReadUInt64();
                }

                return segment_id_to_size;
            }

            private void _read_file(BlockList block_list, SMBinary s)
            {
                var segment_id_to_size = this._read_region_header(s);
                var segment_id = -1;
                var segement = new SmdSegment(this._version.Item4, this._blocks_in_a_line_in_a_segment,scale);
                while (_isEOF(s))
                {
                    segment_id += 1;
                    if (!segment_id_to_size.ContainsKey(segment_id))
                    {
                        s.BaseStream.Seek(5120, SeekOrigin.Current);
                        continue;
                    }
                    if (segment_id_to_size[segment_id] == 0)
                    {
                        s.BaseStream.Seek(5120, SeekOrigin.Current);
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

            private class SmdSegment
            {
                private int _blocks_in_a_line;
                private int _blocks_in_an_area;
                private int _blocks_in_a_cube;
                private int _region_version;
                private int _version;
                private ulong _timestamp;
                private Vector3i _position;
                private bool has_valid_data;
                private uint _compressed_size;
                private int _header_size;
                private readonly int _data_size;
                private readonly int scale;

                public SmdSegment(int version, int blocks_in_a_line = 16,int s = 1)
                {
                    this._blocks_in_a_line = blocks_in_a_line;
                    this._blocks_in_an_area = this._blocks_in_a_line * this._blocks_in_a_line;
                    this._blocks_in_a_cube = this._blocks_in_an_area * this._blocks_in_a_line;
                    this._region_version = version;
                    this._version = 0;
                    this._timestamp = 0;
                    this._position = null;
                    this.has_valid_data = false;
                    this._compressed_size = 0;
                    this.scale = s;
                    // this.block_index_to_block = { }
                    this._header_size = 26;
                    if (this._region_version == 0)
                    {
                        this._header_size = 25;
                    }
                    this._data_size = 5120 - this._header_size;
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
                    if (this._region_version != 0)
                        this._version = Math.Abs((int)(((int)s.ReadByte()) - Math.Pow(2, 8)));
                    //this._version = Math.Abs((int)s.ReadByte());
                    this._timestamp = s.ReadUInt64();
                    this._position = s.read_vector_3_int32();
                    this.has_valid_data = s.ReadBoolean();
                    this._compressed_size = s.ReadUInt32();

                }
                private void _read_block_data(BlockList block_list, SMBinary s)
                {
                 
                    var decompressed_data = DecompressZlib(new MemoryStream(s.ReadBytes((int)this._compressed_size)));//TODO MIGHT BREAK
                    int i = 0;
                    for (int index = 0; index < (int)(decompressed_data.Length / 3); index++)
                    {
                        var pos = index * 3;
                        byte[] padded = new byte[3];
                        Array.Copy(decompressed_data, pos, padded, 0, 3);
                        var int_24 = this.toUnsignedInt(padded);
                        if (int_24 == 0) continue;
                        BlockBit block = new BlockBit(int_24, this._version);
                        if (!BlockTypes.isAnyHull((short)block.get_id()))
                        {
                            continue;
                        }
                        i++;
                        block_list.blist.Add(get_block_position_by_block_index(index,32), block);
                    }
                    Console.WriteLine($"found{i} blocks in segment");
                    s.BaseStream.Seek(this._data_size - this._compressed_size, SeekOrigin.Current);
                }
                public void read(BlockList block_list, SMBinary s)
                {
                    this._read_header(s);
                    if (!has_valid_data)
                    {
                        s.BaseStream.Seek(this._data_size, SeekOrigin.Current);
                    }
                    else
                    {
                        this._read_block_data(block_list, s);
                    }
                    if (this.has_valid_data && block_list.blist.Count == 0)
                        this.has_valid_data = false;
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

                public Vector3i get_block_position_by_block_index(int block_index,int m = 1)
                {
                    var z = (int)(block_index / this._blocks_in_an_area);
                    var rest = block_index % this._blocks_in_an_area;
                    var y = (int)(rest / this._blocks_in_a_line);
                    var x = rest % this._blocks_in_a_line;
                    return new Vector3i(-(x + _position.x) * m*scale, (y + _position.y) * m* scale, (z + _position.z) * m* scale);
                }
            }
        }
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Hawser {
    class WAD {
        public string Path;
        public long Length;
        public BinaryReader Reader;
        public Dictionary<string, Chunk> Chunks = new Dictionary<string, Chunk>();

        public WAD(string _path) {
            this.Path = _path;
            if (File.Exists(_path) == true) {
                this.Reader = new BinaryReader(new MemoryStream(File.ReadAllBytes(_path)));
                using (Chunk _headerChunk = new Chunk(this.Reader)) {
                    if (_headerChunk.Name == "FORM") {
                        this.Length = _headerChunk.Length;
                        while (this.Reader.BaseStream.Position < _headerChunk.Offset + _headerChunk.Length) {
                            Chunk _chunkGet = new Chunk(this.Reader, true);
                            this.Chunks.Add(_chunkGet.Name, _chunkGet);
                        }
                        Console.WriteLine("found {0} chunks", this.Chunks.Count);
                    } else throw new Exception("Invalid GameMaker WAD provided");
                }
            } else throw new FileNotFoundException("Could not find file: " + _path);
        }

        public void Import(string _path) {
            if (File.Exists(_path) == true) {
                using (StreamReader _reader = new StreamReader(File.OpenRead(_path))) {
                    // Gather replacements
                    List<Artifact> StringList = JsonConvert.DeserializeObject<List<Artifact>>(_reader.ReadToEnd());
                    for(int i = 0; i < StringList.Count; i++) {
                        if (StringList[i].Replacement == "") {
                            StringList.RemoveAt(i--);
                        }
                    }
                    Console.WriteLine("found {0} replacement strings", StringList.Count);

                    // Create backup
                    if (File.Exists(this.Path + ".bak") == false) File.Copy(this.Path, this.Path + ".bak");

                    // Update artifacts
                    int _stringBase = 0;
                    for(int i = 0; i < StringList.Count; i++) {
                        StringList[i].Link = this.Length + (_stringBase + 12);
                        _stringBase += 4 + (StringList[i].Replacement.Length + 1);
                    }

                    using (BinaryWriter _writer = new BinaryWriter(File.OpenWrite(this.Path))) {
                        // Write fake chunk
                        _writer.BaseStream.Seek(_writer.BaseStream.Length, SeekOrigin.Begin);
                        _writer.Write((Int32)0x4E454D47);
                        _writer.Write((Int32)_stringBase + 4);
                        _writer.Write((Int32)0);

                        // Write links
                        this.Reader.BaseStream.Seek(this.Chunks["STRG"].Offset, SeekOrigin.Begin);
                        for (int i = 0, _i = this.Reader.ReadInt32(); i < _i; i++) {
                            Int32 _stringOffset = this.Reader.ReadInt32();
                            for(int j = 0; j < StringList.Count; j++) {
                                if (StringList[j].Offset == _stringOffset) {
                                    _writer.BaseStream.Seek(this.Reader.BaseStream.Position - 4, SeekOrigin.Begin);
                                    _writer.Write((Int32)StringList[j].Link + 8);
                                }
                            }
                        }

                        // Write artifacts
                        _writer.BaseStream.Seek(_writer.BaseStream.Length, SeekOrigin.Begin);
                        for(int i = 0; i < StringList.Count; i++) {
                            _writer.Write((Int32)StringList[i].Replacement.Length);
                            _writer.Write((byte[])ASCIIEncoding.ASCII.GetBytes(StringList[i].Replacement));
                            _writer.Write((byte)0x00);
                        }

                        // Update FORM
                        _writer.BaseStream.Seek(4, SeekOrigin.Begin);
                        _writer.Write((Int32)(this.Length + 12) + _stringBase);
                    }

                    // Write artifacts

                    /*for(int i = 0; i < Include.Length; i++) {
                        if (this.Chunks.ContainsKey(Include[i]) == true) {

                        }
                    }*/
                }
            } else throw new FileNotFoundException("Could not find file: " + _path);
        }

        public void Export(string _path) {
            List<Artifact> StringList = new List<Artifact>();
            if (this.Chunks.ContainsKey("STRG") == true) {
                Chunk _chunkString = this.Chunks["STRG"];
                this.Reader.BaseStream.Seek(_chunkString.Offset, SeekOrigin.Begin);
                for (int i = 0, _i = this.Reader.ReadInt32(); i < _i; i++) {
                    StringList.Add(new Artifact(this.Reader));
                }
                Console.WriteLine("exported {0} strings", StringList.Count);
            }
            
            using (StreamWriter _writer = new StreamWriter(File.OpenWrite(_path))) {
                _writer.Write(JsonConvert.SerializeObject(StringList, Formatting.Indented));
            }
        }
    }

    class Chunk : IDisposable {
        public string Name;
        public Int32 Length;
        public long Offset;
        public MemoryStream Data;

        public Chunk(BinaryReader _reader, bool _load=false) {
            this.Name = ASCIIEncoding.ASCII.GetString(_reader.ReadBytes(4));
            this.Length = _reader.ReadInt32();
            this.Offset = _reader.BaseStream.Position;
            if (_load == true) {
                this.Data = new MemoryStream(_reader.ReadBytes(this.Length));
            }
        }

        public void Load(BinaryReader _reader) {
            _reader.BaseStream.Seek(this.Offset, SeekOrigin.Begin);
            this.Data = new MemoryStream(_reader.ReadBytes(this.Length));
            _reader.BaseStream.Seek(this.Offset + this.Length, SeekOrigin.Begin);
        }

        public void Dispose() { return;  }
    }
}

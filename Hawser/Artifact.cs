using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Hawser {
    class Artifact {
        public long Offset;
        public string Target;
        public string Replacement = "";
        [JsonIgnore]
        public long Link = -1;

        public Artifact() {
            // stub
        }

        public Artifact(BinaryReader _reader) {
            this.Offset = _reader.ReadInt32();
            long _baseOffset = _reader.BaseStream.Position;
            _reader.BaseStream.Seek(this.Offset, SeekOrigin.Begin);
            this.Target = ASCIIEncoding.ASCII.GetString(_reader.ReadBytes(_reader.ReadInt32()));
            _reader.BaseStream.Seek(_baseOffset, SeekOrigin.Begin);
        }

        public override string ToString() {
            return this.Target + " => " + this.Replacement;
        }
    }
}

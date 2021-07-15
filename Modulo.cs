using System;
using System.Linq;
using System.Xml.Linq;
using Neo.VM;

namespace LazyCompilerNeo
{
    partial class Modulo
    {
        ScriptBuilder sb = new();
        public byte[] Script => sb.ToArray();
        public Modulo()
        {
        }
        public Modulo(XElement node)
        {
            node.CompileChildren().ForEach(v => sb.EmitRaw(v));
        }
    }
}

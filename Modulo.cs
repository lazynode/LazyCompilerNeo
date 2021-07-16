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
        public Modulo(XElement node)
        {
            node.CompileChildren();
            this.GetType().GetMethod(node.Name.LocalName)?.Invoke(this, new object[] { node });
        }
    }
}

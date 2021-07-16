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
            this.GetType().GetMethod(node.Name.LocalName)?.Invoke(this, new object[] { node });
        }

      

        // if (node.Name.NamespaceName.Length > 0)
        // {
        //     sb.Emit(Enum.Parse<OpCode>(node.Name.LocalName), node.Attribute("oprand")?.Value.HexToBytes());
        // }
        // node.CompileChildren().ForEach(v => sb.EmitRaw(v));
    }
}

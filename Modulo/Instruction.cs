using System.Xml.Linq;
using Neo.VM;
using Neo;
using System;

namespace LazyCompilerNeo
{
    partial class Modulo
    {
        public class Instruction : Modulo
        {
            public Instruction(XElement node) : base(node)
            {
                sb.Emit(Enum.Parse<OpCode>(node.Name.LocalName), node.Attribute("oprand")?.Value.HexToBytes());
            }
        };
    }
}

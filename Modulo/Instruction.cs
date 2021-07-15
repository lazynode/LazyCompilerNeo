using System.Xml.Linq;
using Neo.VM;
using Neo;

namespace LazyCompilerNeo
{
    partial class Modulo
    {
        public class Instruction : Modulo
        {
            public Instruction(XElement node) : base(node)
            {
                sb.Emit((OpCode)System.Enum.Parse(typeof(OpCode), node.Name.LocalName), node.Attribute("oprand")?.Value.HexToBytes());
            }
        };
    }
}

using System.Xml.Linq;
using System.Linq;
using Neo.VM;

namespace LazyCompilerNeo
{
    partial class Modulo
    {
        public class Assembly : Modulo
        {
            public Assembly(XElement node) : base(node)
            {
            }
            public void WHILE(XElement node)
            {
                byte[] bytecode = node.CompileChildren().SelectMany(v => v).ToArray();
                OpCode jmp = (OpCode)System.Enum.Parse(typeof(OpCode), "JMP" + (node.Attribute("instruction")?.Value) ?? "");
                if (node.Attribute("false") is null)
                {
                    sb.EmitRaw(bytecode);
                    sb.EmitJump(jmp, -bytecode.Length);
                    return;
                }
                int offset = bytecode.Length + 0x02;
                if (offset < sbyte.MinValue || offset > sbyte.MaxValue)
                {
                    sb.EmitJump(jmp, offset);
                }
                else
                {
                    sb.EmitJump(jmp, offset + 0x02);
                }
                sb.EmitRaw(bytecode);
            }
        };
    }
}

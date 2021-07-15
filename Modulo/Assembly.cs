using System.Xml.Linq;
using System.Linq;
using Neo;
using Neo.VM;
using System;
using System.Numerics;

namespace LazyCompilerNeo
{
    partial class Modulo
    {
        public class Assembly : Modulo
        {
            static bool static_field_inited = false;
            public Assembly(XElement node) : base(node)
            {
            }
            public void WHILE(XElement node)
            {
                byte[] bytecode = node.CompileChildren().SelectMany(v => v).ToArray();
                OpCode jmp = Enum.Parse<OpCode>("JMP" + (node.Attribute("instruction")?.Value) ?? "");
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
            public void PUSH(XElement node)
            {
                XAttribute attr = node.Attributes().First();
                switch (attr.Name.LocalName)
                {
                    case "int":
                        sb.EmitPush(BigInteger.Parse(attr.Value));
                        break;
                    case "string":
                        sb.EmitPush(attr.Value);
                        break;
                    case "bytes":
                        sb.EmitPush(attr.Value.HexToBytes());
                        break;
                    default:
                        throw new ArgumentException();
                }
            }

            public void VAR(XElement node)
            {
                byte slot = byte.Parse(node.Attribute("slot").Value);
                byte[] bytecode = node.CompileChildren().SelectMany(v => v).ToArray();
                switch (node.Attribute("instruction").Value)
                {
                    case "create":
                        sb.EmitRaw(bytecode);
                        sb.Emit(OpCode.STSFLD, new byte[] { slot });
                        break;
                    case "read":
                        sb.Emit(OpCode.LDSFLD, new byte[] { slot });
                        break;
                    case "update":
                        sb.Emit(OpCode.LDSFLD, new byte[] { slot });
                        sb.EmitRaw(bytecode);
                        sb.Emit(OpCode.STSFLD, new byte[] { slot });
                        break;
                    case "delete":
                        sb.Emit(OpCode.PUSHNULL);
                        sb.Emit(OpCode.STSFLD, new byte[] { slot });
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
        };
    }
}

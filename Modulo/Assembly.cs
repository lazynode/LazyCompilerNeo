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
            public Assembly(XElement node) : base(node)
            {
            }
            public void DOWHILE(XElement node)
            {
                byte[] bytecode = node.CompileChildren().SelectMany(v => v).ToArray();
                OpCode jmp = Enum.Parse<OpCode>("JMP" + (node.Attribute("cond")?.Value) ?? "");
                sb.EmitRaw(bytecode);
                sb.EmitJump(jmp, -bytecode.Length);
                return;
            }
            public void SKIP(XElement node)
            {
                byte[] bytecode = node.CompileChildren().SelectMany(v => v).ToArray();
                OpCode jmp = Enum.Parse<OpCode>("JMP" + (node.Attribute("cond")?.Value) ?? "");
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
            public void MALLOC(XElement node)
            {
                sb.Emit(OpCode.INITSSLOT, new byte[] { byte.Parse(node.Attribute("n").Value) });
            }
            public void VAR(XElement node)
            {
                byte slot = byte.Parse(node.Attribute("slot").Value);
                byte[] bytecode = node.CompileChildren().SelectMany(v => v).ToArray();
                switch (node.Attribute("act").Value)
                {
                    case "set":
                        sb.EmitRaw(bytecode);
                        sb.Emit(OpCode.STSFLD, new byte[] { slot });
                        break;
                    case "get":
                        sb.Emit(OpCode.LDSFLD, new byte[] { slot });
                        break;
                    case "update":
                        sb.Emit(OpCode.LDSFLD, new byte[] { slot });
                        sb.EmitRaw(bytecode);
                        sb.Emit(OpCode.STSFLD, new byte[] { slot });
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
            public void TRANSFORM(XElement node)
            {
                node.Attributes().ToList().ForEach(v =>
                {
                    switch (v.Name.LocalName)
                    {
                        case "sign":
                            sb.Emit(OpCode.SIGN);
                            break;
                        case "abs":
                            sb.Emit(OpCode.ABS);
                            break;
                        case "neg":
                            sb.Emit(OpCode.NEGATE);
                            break;
                        case "inc":
                            sb.Emit(OpCode.INC);
                            break;
                        case "dec":
                            sb.Emit(OpCode.DEC);
                            break;
                        case "sqrt":
                            sb.Emit(OpCode.SQRT);
                            break;
                        case "add":
                            if (v.Value is not null)
                            {
                                sb.EmitPush(BigInteger.Parse(v.Value));
                            }
                            sb.Emit(OpCode.ADD);
                            break;
                        case "sub":
                            if (v.Value is not null)
                            {
                                sb.EmitPush(BigInteger.Parse(v.Value));
                            }
                            sb.Emit(OpCode.SUB);
                            break;
                        case "mul":
                            if (v.Value is not null)
                            {
                                sb.EmitPush(BigInteger.Parse(v.Value));
                            }
                            sb.Emit(OpCode.MUL);
                            break;
                        case "div":
                            if (v.Value is not null)
                            {
                                sb.EmitPush(BigInteger.Parse(v.Value));
                            }
                            sb.Emit(OpCode.DIV);
                            break;
                        case "mod":
                            if (v.Value is not null)
                            {
                                sb.EmitPush(BigInteger.Parse(v.Value));
                            }
                            sb.Emit(OpCode.MOD);
                            break;
                        case "pow":
                            if (v.Value is not null)
                            {
                                sb.EmitPush(BigInteger.Parse(v.Value));
                            }
                            sb.Emit(OpCode.POW);
                            break;
                        default:
                            throw new ArgumentException();
                    }
                });
            }
        };
    }
}

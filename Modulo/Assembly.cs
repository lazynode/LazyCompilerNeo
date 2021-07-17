using System.Xml.Linq;
using System.Linq;
using Neo;
using Neo.VM;
using System;
using System.Numerics;
using System.Xml.XPath;

namespace LazyCompilerNeo
{
    partial class Modulo
    {
        public class Assembly : Modulo
        {
            public static XNamespace ns = nameof(Assembly);
            public Assembly(XElement node) : base(node)
            {
            }
            public void DOWHILE(XElement node)
            {
                node.lazylize();
                node.Add(new XElement(Location.ns + "goto").attr("target", "..").attr("cond", node.attr("cond") ?? "if"));
            }
            public void WHILE(XElement node)
            {
                node.lazylize();
                Guid start = Guid.NewGuid();
                Guid stop = Guid.NewGuid();
                node.AddFirst(new XElement("lazy").attr("id", start));
                node.Add(new XElement("lazy").attr("id", stop));
                node.AddFirst(new XElement(Location.ns + "goto").attr("target", $"../lazy[@id='{stop}']"));
                node.Add(new XElement(Location.ns + "goto").attr("target", $"../lazy[@id='{start}']").attr("cond", node.attr("cond") ?? "if"));
            }
            public void IF(XElement node)
            {
                node.lazylize();
                Guid end = Guid.NewGuid();
                node.Add(new XElement("lazy").attr("id", end));
                node.AddFirst(new XElement(Location.ns + "goto").attr("target", $"../lazy[@id='{end}']").attr("cond", "ifnot"));
            }
            public void INT(XElement node)
            {
                new ScriptBuilder().EmitPush(BigInteger.Parse(node.attr("val"))).construct(node);
            }
            public void STRING(XElement node)
            {
                new ScriptBuilder().EmitPush(node.attr("val")).construct(node);
            }
            public void BYTES(XElement node)
            {
                new ScriptBuilder().EmitPush(node.attr("val").HexToBytes()).construct(node);
            }
            public void BOOL(XElement node)
            {
                new ScriptBuilder().EmitPush(bool.Parse(node.attr("val"))).construct(node);
            }
            // public void MALLOC(XElement node)
            // {
            //     sb.Emit(OpCode.INITSSLOT, new byte[] { byte.Parse(node.Attribute("n").Value) });
            // }
            // public void VAR(XElement node)
            // {
            //     byte slot = byte.Parse(node.Attribute("slot").Value);
            //     byte[] bytecode = node.CompileChildren().SelectMany(v => v).ToArray();
            //     switch (node.Attribute("act").Value)
            //     {
            //         case "set":
            //             sb.EmitRaw(bytecode);
            //             sb.Emit(OpCode.STSFLD, new byte[] { slot });
            //             break;
            //         case "get":
            //             sb.Emit(OpCode.LDSFLD, new byte[] { slot });
            //             break;
            //         case "update":
            //             sb.Emit(OpCode.LDSFLD, new byte[] { slot });
            //             sb.EmitRaw(bytecode);
            //             sb.Emit(OpCode.STSFLD, new byte[] { slot });
            //             break;
            //         default:
            //             throw new ArgumentException();
            //     }
            // }
            // public void TRANSFORM(XElement node)
            // {
            //     node.Attributes().ToList().ForEach(v =>
            //     {
            //         switch (v.Name.LocalName)
            //         {
            //             case "sign":
            //             case "abs":
            //             case "neg":
            //             case "inc":
            //             case "dec":
            //             case "sqrt":
            //             case "add":
            //             case "sub":
            //             case "mul":
            //             case "div":
            //             case "mod":
            //             case "pow":
            //             case "shl":
            //             case "shr":
            //             case "min":
            //             case "max":
            //                 sb.Emit(Enum.Parse<OpCode>(v.Name.LocalName.ToUpper()));
            //                 break;
            //             case "ladd":
            //             case "lsub":
            //             case "lmul":
            //             case "ldiv":
            //             case "lmod":
            //             case "lpow":
            //             case "lshl":
            //             case "lshr":
            //             case "lmin":
            //             case "lmax":
            //                 sb.EmitPush(BigInteger.Parse(v.Value));
            //                 sb.Emit(Enum.Parse<OpCode>(v.Name.LocalName.Substring(1).ToUpper()));
            //                 break;
            //             case "radd":
            //             case "rsub":
            //             case "rmul":
            //             case "rdiv":
            //             case "rmod":
            //             case "rpow":
            //             case "rshl":
            //             case "rshr":
            //             case "rmin":
            //             case "rmax":
            //                 sb.EmitPush(BigInteger.Parse(v.Value));
            //                 sb.Emit(OpCode.SWAP);
            //                 sb.Emit(Enum.Parse<OpCode>(v.Name.LocalName.Substring(1).ToUpper()));
            //                 break;
            //             default:
            //                 throw new ArgumentException();
            //         }
            //     });
            // }
        };
    }
}

using System.Xml.Linq;
using System.Linq;
using Neo;
using Neo.VM;
using System;
using System.Numerics;
using System.Xml.XPath;
using Neo.SmartContract;

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
                node.Add(new XElement(ns + "goto").attr("target", "..").attr("cond", node.attr("cond") ?? "if"));
            }
            public void WHILE(XElement node)
            {
                Guid start = Guid.NewGuid();
                Guid stop = Guid.NewGuid();
                node.AddFirst(new XElement(Compiler.lazy).attr("id", start));
                node.Add(new XElement(Compiler.lazy).attr("id", stop));
                node.AddFirst(new XElement(ns + "goto").attr("target", $"../lazy[@id='{stop}']"));
                node.Add(new XElement(ns + "goto").attr("target", $"../lazy[@id='{start}']").attr("cond", node.attr("cond") ?? "if"));
            }
            public void IF(XElement node)
            {
                Guid end = Guid.NewGuid();
                node.Add(new XElement(Compiler.lazy).attr("id", end));
                node.AddFirst(new XElement(ns + "goto").attr("target", $"../lazy[@id='{end}']").attr("cond", "ifnot"));
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
            public void SYSCALL(XElement node)
            {
                new ScriptBuilder().EmitSysCall(new InteropDescriptor() { Name = node.attr("name") }.Hash).construct(node);
            }
            public void CONTRACTCALL(XElement node)
            {
                node.Add(new ScriptBuilder().EmitPush(Enum.Parse<CallFlags>(node.Attribute("flag")?.Value ?? "All")).construct(new XElement(Compiler.lazy)));
                node.Add(new ScriptBuilder().EmitPush(node.attr("method")).construct(new XElement(Compiler.lazy)));
                node.Add(new ScriptBuilder().EmitPush(UInt160.Parse(node.attr("hash"))).construct(new XElement(Compiler.lazy)));
                node.Add(new XElement(Assembly.ns + "syscall").attr("name", "System.Contract.Call"));
            }
            public void GOTO(XElement node)
            {
                OpCode jmp = $"JMP{(node.attr("cond")?.ToUpper()) ?? ""}_L".opcode();
                string target = node.attr("target") ?? ".";
                new ScriptBuilder().EmitJump(jmp, 0).construct(node).attr("target", target);
            }
            public void INVOKE(XElement node)
            {
                string target = node.attr("target") ?? ".";
                new ScriptBuilder().Emit(OpCode.CALL_L, BitConverter.GetBytes(0)).construct(node).attr("target", target);
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
        };
    }
}

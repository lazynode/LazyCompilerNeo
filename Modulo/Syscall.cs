using System;
using System.Xml.Linq;
using System.Linq;
using Neo;
using Neo.VM;
using Neo.SmartContract;

namespace LazyCompilerNeo
{
    partial class Modulo
    {
        public class Syscall : Modulo
        {
            public static XNamespace NameSpace = nameof(Assembly);
            public Syscall(XElement node) : base(node)
            {
            }
            public void CALL(XElement node)
            {
                var sb = new ScriptBuilder();
                sb.EmitPush(Enum.Parse<CallFlags>(node.Attribute("flag")?.Value ?? "All"));
                sb.UpdateInstruction(node);
                sb = new ScriptBuilder();
                sb.EmitPush(node.Attribute("method").Value);
                sb.UpdateInstruction(node);
                sb = new ScriptBuilder();
                sb.EmitPush(UInt160.Parse(node.Attribute("hash").Value));
                sb.UpdateInstruction(node);
                sb = new ScriptBuilder();
                sb.EmitSysCall(ApplicationEngine.System_Contract_Call);
                sb.UpdateInstruction(node);
            }
            public void GetCallFlags(XElement node)
            {
                var sb = new ScriptBuilder();
                sb.EmitSysCall(ApplicationEngine.System_Contract_GetCallFlags);
                sb.UpdateInstruction(node);
            }
            public void CreateStandardAccount(XElement node)
            {
                var sb = new ScriptBuilder();
                sb.EmitSysCall(ApplicationEngine.System_Contract_CreateStandardAccount);
                sb.UpdateInstruction(node);
            }
            public void CreateMultisigAccount(XElement node)
            {
                var sb = new ScriptBuilder();
                sb.EmitSysCall(ApplicationEngine.System_Contract_CreateMultisigAccount);
                sb.UpdateInstruction(node);
            }
        };
    }
}

using System.Linq;
using System.Xml.Linq;
using Neo.VM;
using Neo;
using System;
using System.Collections.Generic;
using System.Xml.XPath;

namespace LazyCompilerNeo
{
    partial class Modulo
    {
        public class Location : Modulo
        {
            public static readonly XNamespace ns = nameof(Location);
            static Dictionary<XElement, int> length = new();
            public Location(XElement node) : base(node)
            {
                XElement root = node.root();
                if (root.DescendantsAndSelf().Where(v => v.Name.NamespaceName.Length > 0).Any())
                {
                    return;
                }
                root.code(root.DescendantsAndSelf().Select(v => KeyValuePair.Create<XElement, Action>(v, () =>
                {
                    length[v] = v.code().Length;
                })).ToDictionary(kv => kv.Key, kv => kv.Value));
                root.code(root.DescendantsAndSelf().Select(v => KeyValuePair.Create<XElement, Action>(v, () =>
                {
                    if (v.Name.LocalName.StartsWith("jmp") == false)
                    {
                        return;
                    }
                    if (v.attr("target") is null)
                    {
                        return;
                    }
                    new ScriptBuilder().EmitJump(v.Name.LocalName.opcode(), position(v.XPathSelectElement(v.attr("target"))) - position(v)).construct(v);
                })).ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
            }
            public void GOTO(XElement node)
            {
                OpCode jmp = $"JMP{(node.attr("cond")?.ToUpper()) ?? ""}_L".opcode();
                string target = node.attr("target") ?? ".";
                new ScriptBuilder().EmitJump(jmp, 0).construct(node).attr("target", target);
            }
            static int position(XElement node)
            {
                if (node.Parent is null)
                {
                    return node.ElementsBeforeSelf().Select(v => length[v]).Sum();
                }
                return position(node.Parent) + node.ElementsBeforeSelf().Select(v => length[v]).Sum();
            }
        };
    }
}

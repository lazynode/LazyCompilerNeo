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
                XElement root = node.Root();
                if (root.DescendantsAndSelf().Where(v => v.Name.NamespaceName.Length > 0).Count() > 0)
                {
                    return;
                }
                root.Code(root.DescendantsAndSelf().Select(v => KeyValuePair.Create<XElement, Action>(v, () =>
                {
                    length[v] = v.CodeLength();
                })).ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
                root.Code(root.DescendantsAndSelf().Select(v => KeyValuePair.Create<XElement, Action>(v, () =>
                {
                    if (v.Name.LocalName.StartsWith("JMP") == false)
                    {
                        return;
                    }
                    if (v.Attribute("target") is null)
                    {
                        return;
                    }
                    XElement target = v.XPathSelectElement(v.Attribute("target").Value);
                    ScriptBuilder sb = new();
                    sb.EmitJump(Enum.Parse<OpCode>(v.Name.LocalName.ToUpper()), position(target) - position(v));
                    sb.UpdateInstruction(v);
                })).ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
            }
            public void GOTO(XElement node)
            {
                OpCode jmp = Enum.Parse<OpCode>($"JMP{(node.Attribute("cond")?.Value.ToUpper()) ?? ""}_L");
                string target = node.Attribute("target").Value;
                ScriptBuilder sb = new();
                sb.EmitJump(jmp, 0);
                sb.UpdateInstruction(node);
                node.SetAttributeValue("target", target);
            }
            static int position(XElement node)
            {
                XElement brother = node.ElementsBeforeSelf().Count() > 0 ? node.ElementsBeforeSelf().Last() : null;
                return brother != null ? position(brother) + length[brother] : node.Parent != null ? position(node.Parent) : 0;
            }
        };
    }
}

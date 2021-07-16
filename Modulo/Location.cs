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
            static Dictionary<XElement, int> length = new();
            public Location(XElement node) : base(node)
            {
                if (node.Root().DescendantsAndSelf().Where(v => v.Name.NamespaceName.Length > 0).Count() > 0)
                {
                    return;
                }
                node.Code(node.Root().DescendantsAndSelf().Select(v => KeyValuePair.Create<XElement, Action>(v, () =>
                {
                    length[v] = v.CodeLength();
                })).ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
                node.Code(node.Root().DescendantsAndSelf().Select(v => KeyValuePair.Create<XElement, Action>(v, () =>
                {
                    if (v.Name.LocalName != "JMP")
                    {
                        return;
                    }
                    if (v.Attribute("target") is null)
                    {
                        return;
                    }
                    XElement target = node.Root().XPathSelectElement(v.Attribute("target").Value);
                    ScriptBuilder sb = new();
                    sb.EmitJump(OpCode.JMP_L, position(target) - position(node));
                    sb.UpdateInstruction(node);
                })).ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
            }
            public void GOTO(XElement node)
            {
                ScriptBuilder sb = new();
                sb.EmitJump(OpCode.JMP_L, 0);
                sb.UpdateInstruction(node);
                node.SetAttributeValue("target", node.Attribute("target").Value);
            }

            static int position(XElement node)
            {
                XElement brother = node.ElementsBeforeSelf().Count() > 0 ? node.ElementsBeforeSelf().Last() : null;
                return brother != null ? position(brother) + length[brother] : node.Parent != null ? position(node.Parent) : 0;
            }
        };
    }
}

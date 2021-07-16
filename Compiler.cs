using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Neo;
using Neo.VM;

namespace LazyCompilerNeo
{
    static partial class Compiler
    {
        public static byte[] Compiled(this XElement node)
        {
            while (node.DescendantsAndSelf().Where(v => v.Name.NamespaceName.Length > 0).Count() > 0)
            {
                Compile(node);
            }
            return node.Code();
        }
        public static void Compile(this XElement node)
        {
            Activator.CreateInstance(typeof(Modulo).GetNestedType(node.Name.NamespaceName) ?? typeof(Modulo), node);
        }

        public static void CompileChildren(this XElement node)
        {
            node.Elements().ToList().ForEach(v => v.Compile());
        }
        public static XElement Root(this XElement node)
        {
            XElement root = node;
            while (root.Parent is not null)
            {
                root = root.Parent;
            }
            return root;
        }
        public static int CodeLength(this XElement node)
        {
            return Code(node, new()).Length;
        }
        public static byte[] Code(this XElement node)
        {
            return Code(node, new());
        }
        public static byte[] Code(this XElement node, Dictionary<XElement, Action> callback)
        {
            ScriptBuilder sb = new();
            node.Descendants().ToList().ForEach(v =>
            {
                if (v.Name.NamespaceName != "")
                {
                    throw new Exception();
                }
                if (callback.ContainsKey(v))
                {
                    callback[v]();
                }
                if (v.Name.LocalName == "lazy")
                {
                    return;
                }
                sb.Emit(Enum.Parse<OpCode>(v.Name.LocalName), v.Attribute("oprand")?.Value.HexToBytes());
            });
            return sb.ToArray();
        }
        public static void UpdateInstruction(this ScriptBuilder sb, XElement node)
        {
            byte[] bytecode = sb.ToArray();
            node.Name = ((OpCode)bytecode[0]).ToString();
            node.RemoveAttributes();
            node.SetAttributeValue("oprand", bytecode.Skip(1).ToArray().ToHexString());
        }
    }
}

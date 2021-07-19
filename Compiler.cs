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
        public static readonly string lazy = "lazy";
        public static byte[] compiled(this XElement node, int left = int.MaxValue)
        {
            if (left == 0)
            {
                return null;
            }
            if (node.DescendantsAndSelf().Where(v => v.Name.NamespaceName.Length > 0).Any() == false)
            {
                return node.code();
            }
            node.compile();
            return node.compiled(left--);
        }
        public static void compile(this XElement node)
        {
            Activator.CreateInstance(typeof(Modulo).GetNestedType(node.Name.NamespaceName) ?? typeof(Modulo), node);
        }
        public static void compile_children(this XElement node)
        {
            node.Elements().ToList().ForEach(compile);
        }
        public static XElement root(this XElement node)
        {
            if (node.Parent is null)
            {
                return node;
            }
            return node.Parent.root();
        }
        public static byte[] code(this XElement node)
        {
            return code(node, new());
        }
        public static byte[] code(this XElement node, Dictionary<XElement, Action> callback)
        {
            return node.DescendantsAndSelf().Aggregate(new ScriptBuilder(), (sb, v) => sb.emit(v, callback)).ToArray();
        }
        public static void lazylize(this XElement node)
        {
            node.Name = lazy;
            node.RemoveAttributes();
        }
        public static string attr(this XElement node, XName name)
        {
            return node.Attribute(name)?.Value;
        }
        public static XElement attr(this XElement node, XName name, object value)
        {
            node.SetAttributeValue(name, value);
            return node;
        }
        public static ScriptBuilder emit(this ScriptBuilder sb, XElement node, Dictionary<XElement, Action> callback)
        {
            if (node.Name.NamespaceName != "")
            {
                throw new Exception(); // TODO
            }
            if (callback.ContainsKey(node))
            {
                callback[node]();
            }
            if (node.Name.LocalName == lazy)
            {
                return sb;
            }
            return sb.Emit(node.Name.LocalName.opcode(), node.Attribute("oprand")?.Value.HexToBytes());
        }
        public static XElement construct(this ScriptBuilder sb, XElement node)
        {
            byte[] bytecode = sb.ToArray();
            node.Name = bytecode[0].cmd();
            node.RemoveAll();
            node.SetAttributeValue("oprand", bytecode.Skip(1).ToArray().ToHexString());
            return node;
        }
        public static OpCode opcode(this string str)
        {
            return Enum.Parse<OpCode>(str.ToUpper());
        }
        public static XElement loadxml(this string str)
        {
            return XElement.Load(str);
        }
        public static string cmd(this OpCode op)
        {
            return op.ToString().ToLower();
        }
        public static string cmd(this byte op)
        {
            return ((OpCode)op).cmd();
        }
    }
}

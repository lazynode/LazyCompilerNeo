using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LazyCompilerNeo
{
    static partial class Compiler
    {
        public static byte[] Compile(this XElement node)
        {
            return ((Modulo)Activator.CreateInstance(typeof(Modulo).GetNestedType(node.Name.NamespaceName) ?? typeof(Modulo.Plain), node)).Script;
        }

        public static List<byte[]> CompileChildren(this XElement node)
        {
            return node.Elements().Select(v => v.Compile()).ToList();
        }
    }
}

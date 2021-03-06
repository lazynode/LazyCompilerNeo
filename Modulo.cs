using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Neo.VM;

namespace LazyCompilerNeo
{
    partial class Modulo
    {
        public Modulo(XElement node)
        {
            node.compile_children();
            GetType().GetMethod(node.Name.LocalName.ToUpper())?.Invoke(this, new object[] { node });
            if (node.Name.NamespaceName.Length > 0)
            {
                node.lazylize();
            }
        }
    }
}

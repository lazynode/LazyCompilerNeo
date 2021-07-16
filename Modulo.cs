﻿using System;
using System.Linq;
using System.Xml.Linq;
using Neo.VM;

namespace LazyCompilerNeo
{
    partial class Modulo
    {
        public Modulo(XElement node)
        {
            node.CompileChildren();
            this.GetType().GetMethod(node.Name.LocalName)?.Invoke(this, new object[] { node });
        }
    }
}

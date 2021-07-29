using System.Xml.Linq;
using System.Xml.XPath;
using System.Linq;
using Neo;
using Neo.VM;
using System;

namespace LazyCompilerNeo
{
    partial class Modulo
    {
        public class Basic : Modulo
        {
            public static XNamespace ns = nameof(Basic);
            public Basic(XElement node) : base(node)
            {
            }
            public void FUNC(XElement node)
            {
                if (node.attr("inline") is null)
                {
                    node.AddFirst(new ScriptBuilder().Emit(OpCode.INITSLOT, new byte[] { byte.Parse(node.attr("vars") ?? "0"), byte.Parse(node.attr("args") ?? "0") }).construct(new XElement(Compiler.lazy)));
                }
                node.AddFirst(new XElement(Compiler.lazy).attr("function", node.attr("name")).attr("inline", node.attr("inline")));
            }
            public void ARG(XElement node)
            {
                if (node.Parent.Name.NamespaceName != ns)
                {
                    throw new Exception();
                }
                if (node.Parent.Name.LocalName != nameof(FUNC).ToLower())
                {
                    throw new Exception();
                }
                int index = int.Parse(node.Parent.attr("args") ?? "0");
                node.Parent.attr("args", $"{index + 1}");
                node.Add(new XElement(Compiler.lazy).attr("type", "arg").attr("name", node.attr("name")).attr("index", $"{index}"));
            }
            public void VAR(XElement node)
            {
                if (node.Parent.Name.NamespaceName != ns)
                {
                    throw new Exception();
                }
                if (node.Parent.Name.LocalName != nameof(FUNC).ToLower())
                {
                    throw new Exception();
                }
                int index = int.Parse(node.Parent.attr("vars") ?? "0");
                node.Parent.attr("vars", $"{index + 1}");
                node.Add(new XElement(Compiler.lazy).attr("type", "var").attr("name", node.attr("name")).attr("index", $"{index}"));
            }
            public void GET(XElement node)
            {
                switch (node.attr("type"))
                {
                    case "var":
                        node.Add(new ScriptBuilder().Emit(OpCode.LDLOC, new byte[] { byte.Parse(node.attr("index")) }).construct(new XElement(Compiler.lazy)));
                        break;
                    case "arg":
                        node.Add(new ScriptBuilder().Emit(OpCode.LDARG, new byte[] { byte.Parse(node.attr("index")) }).construct(new XElement(Compiler.lazy)));
                        break;
                    case "literal":
                        switch (node.attr("datatype"))
                        {
                            case "int":
                            case "bool":
                            case "string":
                            case "bytes":
                                node.Add(new XElement(Assembly.ns + node.attr("datatype")).attr("val", node.attr("val")));
                                break;
                            default:
                                throw new Exception();
                        }
                        break;
                    default:
                        throw new Exception();
                }
            }
            public void SET(XElement node)
            {
                switch (node.attr("type"))
                {
                    case "var":
                        node.Add(new ScriptBuilder().Emit(OpCode.STLOC, new byte[] { byte.Parse(node.attr("index")) }).construct(new XElement(Compiler.lazy)));
                        break;
                    case "arg":
                        node.Add(new ScriptBuilder().Emit(OpCode.STARG, new byte[] { byte.Parse(node.attr("index")) }).construct(new XElement(Compiler.lazy)));
                        break;
                    default:
                        throw new Exception();
                }
            }
            public void LITERAL(XElement node)
            {
                node.Add(new XElement(ns + nameof(GET).ToLower()).attr("datatype", node.attr("type")).attr("val", node.attr("val")).attr("type", "literal"));
            }
            public void LOAD(XElement node)
            {
                XElement func = node;
                while (func.Name.NamespaceName != ns || func.Name.LocalName != nameof(FUNC).ToLower())
                {
                    func = func.Parent;
                }
                XElement ret = func.XPathSelectElement($"./lazy/lazy[@name='{node.attr("name")}'{(node.attr("type") is null ? "" : $" and @type='{node.attr("type")}'")}]");
                node.Add(new XElement(ns + nameof(GET).ToLower()).attr("index", ret.attr("index")).attr("type", ret.attr("type")));
            }
            public void SAVE(XElement node)
            {
                XElement func = node;
                while (func.Name.NamespaceName != ns || func.Name.LocalName != nameof(FUNC).ToLower())
                {
                    func = func.Parent;
                }
                XElement ret = func.XPathSelectElement($"./lazy/lazy[@name='{node.attr("name")}'{(node.attr("type") is null ? "" : $" and @type='{node.attr("type")}'")}]");
                node.Add(new XElement(ns + nameof(SET).ToLower()).attr("index", ret.attr("index")).attr("type", ret.attr("type")));
            }
            public void IF(XElement node)
            {
                XElement element = new XElement(Assembly.ns + "if", node.Elements().ToList());
                node.Elements().Remove();
                node.Add(new XElement(ns + nameof(LOAD).ToLower()).attr("type", node.attr("type")).attr("name", node.attr("name")));
                node.Add(element);
            }
            public void ELSE(XElement node)
            {
                XElement element = new XElement(Assembly.ns + "else", node.Elements().ToList());
                node.Elements().Remove();
                node.Add(new XElement(ns + nameof(LOAD).ToLower()).attr("type", node.attr("type")).attr("name", node.attr("name")));
                node.Add(element);
            }
            public void DOWHILE(XElement node)
            {
                node.Add(new XElement(ns + nameof(LOAD).ToLower()).attr("type", node.attr("type")).attr("name", node.attr("name")));
                XElement element = new XElement(Assembly.ns + "dowhile", node.Elements().ToList());
                node.Elements().Remove();
                node.Add(element);
            }
            public void WHILE(XElement node)
            {
                node.Add(new XElement(ns + nameof(LOAD).ToLower()).attr("type", node.attr("type")).attr("name", node.attr("name")));
                XElement element = new XElement(Assembly.ns + "while", node.Elements().ToList());
                node.Elements().Remove();
                node.Add(new XElement(ns + nameof(LOAD).ToLower()).attr("type", node.attr("type")).attr("name", node.attr("name")));
                node.Add(element);
            }
            public void RETURN(XElement node)
            {
                XElement get = new XElement(Compiler.lazy, node.Descendants(ns + nameof(GET).ToLower()).Reverse().ToList());
                node.Elements().Remove();
                node.Add(get);
                node.Add(new ScriptBuilder().Emit(OpCode.RET).construct(new XElement(Compiler.lazy)));
            }
            public void EXEC(XElement node)
            {
                XElement get = new XElement(Compiler.lazy, node.Descendants(ns + nameof(GET).ToLower()).Reverse().ToList());
                XElement set = new XElement(Compiler.lazy, node.Descendants(ns + nameof(SET).ToLower()).ToList());
                node.root().Descendants().Where(v => v.Name == ns + nameof(FUNC).ToLower()).Where(v => v.attr("inline") is not null).Where(v => v.attr("name") == node.attr("name")).ToList().ForEach(v => v.compile());
                XElement target = node.XPathSelectElement($"//lazy[@function='{node.attr("name")}']");
                node.Elements().Remove();
                node.Add(get);
                if (target?.attr("inline") is null)
                {
                    node.Add(new XElement(Assembly.ns + "invoke").attr("target", $"//lazy[@function='{node.attr("name")}']"));
                }
                else
                {
                    XElement func = new(Compiler.lazy);
                    target.ElementsAfterSelf().ToList().ForEach(v => func.Add(new XElement(v)));
                    node.Add(func);
                }
                node.Add(set);
            }
        };
    }
}

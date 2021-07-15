using System.Xml.Linq;

namespace LazyCompilerNeo
{
    partial class Modulo
    {
        public class Plain : Modulo
        {
            public Plain(XElement node) : base(node)
            {
                node.CompileChildren().ForEach(v => sb.EmitRaw(v));
            }
        };
    }
}

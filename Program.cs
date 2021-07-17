using System;
using System.IO;
using System.Xml.Linq;

namespace LazyCompilerNeo
{
    class Program
    {
        static void Main(string[] args)
        {
            XElement root = Environment.GetEnvironmentVariable("I")?.loadxml() ?? XElement.Load(Console.OpenStandardInput());
            Console.OpenStandardOutput().Write(root.compiled(int.Parse(Environment.GetEnvironmentVariable("COMPILETIME") ?? int.MaxValue.ToString())));
            if (Environment.GetEnvironmentVariable("DEBUG") is not null)
            {
                Console.Error.WriteLine(root);
            }
        }
    }
}

using System;
using System.Xml.Linq;

namespace LazyCompilerNeo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OpenStandardOutput().Write(XElement.Load(Console.OpenStandardInput()).Compiled());
        }
    }
}

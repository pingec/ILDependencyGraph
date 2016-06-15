using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ILDependencyGraph
{
    public class AssemblyReference
    {
        public AssemblyDefinition Assembly;
        public AssemblyDefinition ReferencedAssembly;

        public override bool Equals(object obj)
        {
            if (obj is AssemblyReference)
            {
                var other = obj as AssemblyReference;
                return Assembly.Name.Name == other.Assembly.Name.Name && ReferencedAssembly.Name.Name == other.ReferencedAssembly.Name.Name;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (this.Assembly.Name.Name + this.ReferencedAssembly.Name.Name).GetHashCode();
        }
    }

    internal class Program
    {
        /*
         * This console app produces a dot graph of dependencies of a provided .NET assembly file (exe/dll)
         * It only shows dependencies that are dristributed together with the assembly in the same directory (or optionally you can specify a separate directory)
         * You can use the text output to render the diagram Graphviz
         * One way of producing a rendered graph would be:
         * ILDependencyGraph.exe "path/to/my/assembly.dll" > assembly.dot
         * Then open GVEdit that comes with Graphviz for Windows and load the assembly.dot
         *
         */

        private static void Main(string[] args)
        {
            if (args.Length < 1 || args.Length > 2)
            {
                Console.WriteLine("Example usage: ILDependencyGraph.exe assembly [refsDir]");
                Console.WriteLine("assembly - the path to IL assembly file to analyze (eg. dll or exe)");
                Console.WriteLine("refsDir - optional path where referenced assemblies can be loaded from (if empty, same path as assembly is implied)");
                Console.WriteLine("WARNING: Any dll that is not available as a file in refsDir will not be shown in the reulting diagram (Eg. mscorlib.dll)");
                return;
            }

            var filePath = new FileInfo(args[0]).FullName;
            var refsDir = args.Length > 1 ? new FileInfo(args[1]).Directory.FullName : new FileInfo(filePath).Directory.FullName;

            var assembly = AssemblyDefinition.ReadAssembly(filePath);
            var dependencies = RecurseAssemblyReferences(assembly, refsDir).Distinct();

            Console.WriteLine("digraph AssemblyDependencies {");
            foreach (var dependency in dependencies)
            {
                Console.WriteLine(@"""{0}"" -> ""{1}"";", dependency.ReferencedAssembly.Name.Name, dependency.Assembly.Name.Name);
            }
            Console.WriteLine("}");

            Console.Read();
        }

        private static AssemblyDefinition ReadAssembly(string fileName, string refsDir)
        {
            try
            {
                return AssemblyDefinition.ReadAssembly(Path.Combine(refsDir, fileName));
            }
            catch
            {
                return null;
            }
        }

        private static List<AssemblyReference> RecurseAssemblyReferences(AssemblyDefinition assembly, string refsDir)
        {
            var currentResult = new List<AssemblyReference>();
            foreach (AssemblyNameReference anr in assembly.MainModule.AssemblyReferences) //we assume one module per assembly
            {
                var dllName = anr.Name + ".dll";

                var referencedAssembly = ReadAssembly(dllName, refsDir);

                if (referencedAssembly == null)
                    continue;

                currentResult.Add(new AssemblyReference { Assembly = assembly, ReferencedAssembly = referencedAssembly });

                var bla = RecurseAssemblyReferences(referencedAssembly, refsDir);
                currentResult.AddRange(bla);
            }

            return currentResult;
        }
    }
}
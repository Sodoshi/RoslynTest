using System.Diagnostics;
using RoslynCompiler;
using RoslynDomain;
namespace TestHarness
{
    internal static class Program
    {
        private static void Main()
        {
            const string code = @"using System;
                using RoslynDomain;
                namespace Custom
                {
                    public class Code : ICode
                    {
                        public void Execute(ref ICommand command)
                        {
                            Console.WriteLine(command.Data);
                        }
                    }
                }";
            using (var compiler = new Compiler())
            {
                compiler.Compile(code);
                ICommand command = new Command
                {
                    Data = "Hello world"
                };
                var consoleOutput = compiler.Execute("Custom.Code", ref command);
                Debug.Write(consoleOutput);
            }
        }
    }
}
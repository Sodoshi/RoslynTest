using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RoslynDomain;
namespace RoslynCompiler
{
    public class AppDomainProxy : MarshalByRefObject, IDisposable
    {
        private readonly Dictionary<string, ICode> _code = new Dictionary<string, ICode>();
        private readonly ConsoleOutput _consoleOutput;
        public AppDomainProxy()
        {
            _consoleOutput = new ConsoleOutput();
        }
        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }
        public string ExecuteCode(string nameSpace, ref ICommand command)
        {
            if (!_code.TryGetValue(nameSpace, out var code))
            {
                return string.Empty;
            }
            _consoleOutput.Clear();
            code.Execute(ref command);
            return _consoleOutput.GetOutput();
        }
        ~AppDomainProxy()
        {
            ReleaseUnmanagedResources();
        }
        public void LoadAssembly(byte[] assemblyFile)
        {
            var assembly = Assembly.Load(assemblyFile);
            assembly.GetTypes()
                .Where(x => x.GetInterface("ICode", true) != null)
                .ToList().ForEach(
                    assemblyClass =>
                    {
                        var nameSpace = assemblyClass.FullName ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(nameSpace))
                        {
                            return;
                        }
                        var command = assembly.CreateInstance(nameSpace) as ICode;
                        if (_code.ContainsKey(nameSpace))
                        {
                            _code[nameSpace] = command;
                        }
                        else
                        {
                            _code.Add(nameSpace, command);
                        }
                    });
        }
        private void ReleaseUnmanagedResources()
        {
            _consoleOutput.Dispose();
        }
    }
}
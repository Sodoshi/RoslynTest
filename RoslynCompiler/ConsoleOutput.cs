using System;
using System.IO;
namespace RoslynCompiler
{
    public class ConsoleOutput : IDisposable
    {
        private readonly TextWriter _originalOutput;
        private readonly StringWriter _stringWriter;
        public ConsoleOutput()
        {
            _stringWriter = new StringWriter();
            _originalOutput = Console.Out;
            Console.SetOut(_stringWriter);
        }
        public void Dispose()
        {
            Console.SetOut(_originalOutput);
            _stringWriter.Dispose();
        }
        public void Clear()
        {
            var stringBuilder = _stringWriter.GetStringBuilder();
            stringBuilder.Clear();
        }
        public string GetOutput()
        {
            return _stringWriter.ToString();
        }
    }
}
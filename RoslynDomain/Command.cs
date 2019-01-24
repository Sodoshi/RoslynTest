using System;
namespace RoslynDomain
{
    [Serializable]
    public class Command : ICommand
    {
        public string Data { get; set; }
    }
}

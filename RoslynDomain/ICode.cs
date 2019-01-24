namespace RoslynDomain
{
    public interface ICode
    {
        void Execute(ref ICommand command);
    }
}
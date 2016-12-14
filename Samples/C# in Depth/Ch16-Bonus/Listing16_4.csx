using System.ComponentModel;
using System.Runtime.CompilerServices;

dynamic x = new TypeUsedDynamically();
x.ShowCaller();

class TypeUsedDynamically
{
    internal void ShowCaller([CallerMemberName] string caller = "Unknown")
    {
        Console.WriteLine("Called by: {0}", caller);
    }
}
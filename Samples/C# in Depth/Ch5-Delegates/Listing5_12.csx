#r "System.Windows.Forms"

using System.Windows.Forms;

MethodInvoker x = CreateDelegateInstance();
x();
x();

static MethodInvoker CreateDelegateInstance()
{
    int counter = 5;

    MethodInvoker ret = delegate
    {
         Console.WriteLine(counter);
         counter++;
    };
    ret();
    return ret;
}
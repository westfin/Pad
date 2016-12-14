Execute(() => 1);

static void Execute(Func<int> action)
{
    Console.WriteLine("action returns an int: " + action());
}

static void Execute(Func<double> action)
{
    Console.WriteLine("action returns a double: " + action());
}

object list = new List<string> { "x", "y" };
object item = "z";
Console.WriteLine(AddConditionally(list, item));

private static bool AddConditionallyImpl<T>(IList<T> list, T item)
{
    if (list.Count > 10)
    {
        list.Add(item);
        return true;
    }
    return false;
}

public static bool AddConditionally(dynamic list, dynamic item)
{
    return AddConditionallyImpl(list, item);
}
Type type = typeof(ObjectExtensions); // Was typeof(Snippet) for Snippy
MethodInfo definition = type.GetMethod("Dump");
MethodInfo constructed;
constructed = definition.MakeGenericMethod(typeof(string));
constructed.Invoke(null, new object[] { "Hello" });

public static void PrintTypeParameter<T>()
{
    Console.WriteLine (typeof(T));
}


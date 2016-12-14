PrintConvertedValue("I'm a string", x => x.Length);

static void PrintConvertedValue<TInput, TOutput>
    (TInput input, Converter<TInput, TOutput> converter)
{
    Console.WriteLine(converter(input));
}


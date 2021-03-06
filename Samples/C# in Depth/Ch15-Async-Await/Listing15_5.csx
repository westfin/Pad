Task<int> task = ComputeLengthAsync(null);
Console.WriteLine("Fetched the task");
int length = await task;
Console.WriteLine("Length: {0}", length);

static async Task<int> ComputeLengthAsync(string text)
{
    if (text == null)
    {
        throw new ArgumentNullException("text");
    }
    await Task.Delay(500); // Simulate real asynchronous work
    return text.Length;
}

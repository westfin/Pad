StreamFactory factory = GenerateSampleData;

using (Stream stream = factory())
{
    int data;
    while ((data = stream.ReadByte()) != -1)
    {
        Console.WriteLine(data);
    }
}

delegate Stream StreamFactory();

static MemoryStream GenerateSampleData()
{
    byte[] buffer = new byte[16];
    for (int i = 0; i < buffer.Length; i++)
    {
        buffer[i] = (byte)i;
    }
    return new MemoryStream(buffer);
}

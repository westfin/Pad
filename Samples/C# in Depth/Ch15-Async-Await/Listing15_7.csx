Task task = ThrowCancellationException();
Console.WriteLine(task.Status);

static async Task ThrowCancellationException()
{
    throw new OperationCanceledException();
}

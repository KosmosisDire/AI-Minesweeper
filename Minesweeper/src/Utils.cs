

public static class Utils
{
    public static async Task WaitUntil(Func<bool> condition, int frequency = 25, int timeout = -1, CancellationToken token = default)
    {
        var waitTask = Task.Run(async () =>
        {
            while (!condition() && !token.IsCancellationRequested) await Task.Delay(frequency);
        });

        if (waitTask != await Task.WhenAny(waitTask, 
                Task.Delay(timeout))) 
            throw new TimeoutException();
    }
}
namespace PasswordManager.UI.Helpers;

public static class AsyncOperationHelper
{
    public static async Task RunAsync(
        Func<Task> operation,
        Func<bool> isBusy,
        Action<bool> setBusy)
    {
        if (isBusy())
            return;

        try
        {
            setBusy(true);

            await operation();
        }
        finally
        {
            setBusy(false);
        }
    }

    public static async Task<T?> RunAsync<T>(
        Func<Task<T>> operation,
        Func<bool> isBusy,
        Action<bool> setBusy)
    {
        if (isBusy())
            return default;

        try
        {
            setBusy(true);

            return await operation();
        }
        finally
        {
            setBusy(false);
        }
    }
}
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NSchedule
{
    public static class Extensions
    {
        // NOTE: Async void is intentional here. This provides a way
        // to call an async method from the constructor while
        // communicating intent to fire and forget, and allow
        // handling of exceptions
        public static async void SafeFireAndForget(this Task task,
            bool returnToCallingContext,
            Action<Exception> onException = null)
        {
            try
            {
                await task.ConfigureAwait(returnToCallingContext);
            }

            // if the provided action is not null, catch and
            // pass the thrown exception
            catch (Exception ex) when (onException != null)
            {
                onException(ex);
            }
        }

        public static Page GetCurrentPage(this Shell s)
        {
            return (s?.CurrentItem?.CurrentItem as IShellSectionController)?.PresentedPage;
        }
    }
}

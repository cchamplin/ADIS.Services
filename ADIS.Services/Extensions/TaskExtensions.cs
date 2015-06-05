using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIS.Services
{
    public static class TaskExceptions
    {
        public static Task<T> AsTaskResult<T>(this T result)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(result);
            return tcs.Task;
        }
        public static Task<object> AsTaskException(this Exception ex)
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetException(ex);
            return tcs.Task;
        }
    }
}

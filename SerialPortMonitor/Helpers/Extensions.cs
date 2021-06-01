using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SerialPortMonitor.Helpers
{

    public static class Extensions
    {
        /* ---------------------------------------------------------------------------------------------------------- */
        /* Author: Bruno Zell */
        /* Link: https://stackoverflow.com/questions/5497064/how-to-get-the-full-path-of-running-process              */
        /* ---------------------------------------------------------------------------------------------------------- */

        [DllImport("Kernel32.dll")]
        private static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);

        public static string GetPathToApp(this Process process, int buffer = 1024)
        {
            var fileNameBuilder = new StringBuilder(buffer);
            uint bufferLength = (uint)fileNameBuilder.Capacity + 1;
            return QueryFullProcessImageName(process.Handle, 0, fileNameBuilder, ref bufferLength) ? fileNameBuilder.ToString() : null;
        }

        /* ---------------------------------------------------------------------------------------------------------- */
        /* Author: Arend */
        /* Link: https://stackoverflow.com/questions/470256/process-waitforexit-asynchronously                        */
        /* ---------------------------------------------------------------------------------------------------------- */

        public static Task WaitForExitAsync(this Process process, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (process.HasExited) return Task.CompletedTask;

            var tcs = new TaskCompletionSource<object>();
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(null);
            if (cancellationToken != default(CancellationToken))
            {
                cancellationToken.Register(() => tcs.SetCanceled());
            }
                
            return process.HasExited ? Task.CompletedTask : tcs.Task;
        }
    }
}

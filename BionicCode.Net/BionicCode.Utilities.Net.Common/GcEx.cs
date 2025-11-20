namespace BionicCode.Utilities.Net
{
    using System;

    /// <summary>
    /// Helper methods to extend the <see cref="GC"/> class.
    /// </summary>
    public static class GcEx
    {
        /// <summary>
        /// Forces garbage collection and waits for finalizers to complete. 
        /// </summary>
        /// <param name="isAsync"><see langword="true"/> to perform a blocking garbage collection; <see langword="false"/> to perform a background garbage collection where possible.</param>
        /// <remarks>Executes multiple calls to <see cref="GC.Collect(int, GCCollectionMode, bool, bool)"/> and <see cref="GC.WaitForPendingFinalizers"/>, where all generations are collected and the GC runs in blocking mode by default and heap compacting is disabled.</remarks>
        public static void ForceFullGC(bool isAsync = false)
        {
            for (int i = 0; i < 10; i++)
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: !isAsync, compacting: false);
                GC.WaitForPendingFinalizers();
            }
        }
    }
}

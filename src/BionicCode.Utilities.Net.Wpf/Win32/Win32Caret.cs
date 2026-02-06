namespace BionicCode.Utilities.Net
{
#if !NETSTANDARD
    using System;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Interop;

    /// <summary>
    ///  Wrapper of Win32 caret functions.
    /// </summary>
    /// <remarks>To create multiple carets, create the required number of <see cref="Win32Caret"/> instances.
    /// <para>This type implements <see cref="IDisposable"/>. Calling <see cref="IDisposable.Dispose"/> will destroy the caret.</para></remarks>
    /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/menurc/carets"/>
    /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/menurc/about-carets"/>  
    public class Win32Caret : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool CreateCaret(IntPtr hWnd, IntPtr hBitmap, int nWidth, int nHeight);

        [DllImport("user32.dll")]
        private static extern bool ShowCaret(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool HideCaret(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetCaretPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern bool GetCaretPos(out CartesianPoint lpPoint);

        [DllImport("user32.dll")]
        private static extern bool DestroyCaret();

        [DllImport("user32.dll")]
        private static extern uint GetCaretBlinkTime();

        [DllImport("user32.dll")]
        private static extern bool SetCaretBlinkTime(uint milliseconds);

        public CaretInfo CurrentCaretInfo { get; private set; }
        public Window HostingWindow { get; }
        public IntPtr HostingWindowHandle { get; }

        public CaretVisibility CaretVisibility { get; private set; }

        private bool disposedValue;

        public Win32Caret(Window hostingWindow)
        {
            HostingWindow = hostingWindow;
            HostingWindowHandle = new WindowInteropHelper(hostingWindow).Handle;
        }

        /// <summary>
        /// Create and show a caret for the current hosting Window.
        /// </summary>
        /// <param name="caretInfo">Defines the properties of the caret.</param>
        /// <returns><c>true</c> if the operation was successful. Returns <c>false</c> if the creation has failed or the caret is already visible.</returns>
        /// <remarks>To create multiple carets, create the required number of <see cref="Win32Caret"/> instances.</remarks>
        /// <exception cref="InvalidOperationException">The caret was already destroyed.</exception>
        /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showcaret">ShowCaret function (winuser.h)</seealso>
        public bool Show(CaretInfo caretInfo)
        {
            bool isCaretVisible;
            switch (CaretVisibility)
            {
                case CaretVisibility.Destroyed:
                    throw new InvalidOperationException("The caret was already destroyed.");
                case CaretVisibility.Visible:
                    return false;
                case CaretVisibility.Hidden:
                    isCaretVisible = Win32Caret.ShowCaret(HostingWindowHandle);
                    break;
                default:
                    isCaretVisible = Win32Caret.CreateCaret(HostingWindowHandle, IntPtr.Zero, (int)CurrentCaretInfo.Width, (int)CurrentCaretInfo.Height)
                      && Win32Caret.SetCaretPos((int)CurrentCaretInfo.Position.X, (int)CurrentCaretInfo.Position.Y)
                      && Win32Caret.ShowCaret(HostingWindowHandle);
                    break;
            }

            CurrentCaretInfo = caretInfo;

            CaretVisibility = isCaretVisible ? CaretVisibility.Visible : CaretVisibility.Undefined;
            return isCaretVisible;
        }

        /// <summary>
        /// Destroys the Win32 caret and disposes the current instance.
        /// </summary>
        /// <returns><c>true</c> if successful.</returns>
        /// <remarks>To temporarily hide the caret call <see cref="Hide"/> instead.</remarks>
        /// <exception cref="InvalidOperationException">The caret was already destroyed.</exception>
        /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-destroycaret">DestroyCaret function (winuser.h)</seealso>
        public bool Destroy()
        {
            if (CaretVisibility == CaretVisibility.Destroyed)
            {
                throw new InvalidOperationException("The caret was already detroyed.");
            }

            CaretVisibility = CaretVisibility.Destroyed;
            return DisposeInternal();
        }

        /// <summary>
        /// Hides the caret, but does not destroy it.
        /// </summary>
        /// <returns><c>true</c> if successful.</returns>
        /// <remarks>To destroy the caret call <see cref="Destroy"/> or <see cref="IDisposable.Dispose"/>.</remarks>
        /// <exception cref="InvalidOperationException">The caret was already destroyed.</exception>
        /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-hidecaret">HideCaret function (winuser.h)</seealso>
        public bool Hide()
        {
            if (CaretVisibility == CaretVisibility.Destroyed)
            {
                throw new InvalidOperationException("The caret was already detroyed.");
            }

            CaretVisibility = CaretVisibility.Hidden;
            return Win32Caret.HideCaret(HostingWindowHandle);
        }

        /// <summary>
        /// Change the position at where the caret is rendered.
        /// </summary>
        /// <param name="caretInfo">Defines the properties of the caret.</param>
        /// <returns><c>true</c> if successfull.</returns>
        /// <exception cref="InvalidOperationException">The caret was already detroyed.</exception>
        /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setcaretpos">SetCaretPos function (winuser.h)</seealso>
        public bool ChangePosition(CaretInfo caretInfo)
        {
            if (CaretVisibility == CaretVisibility.Destroyed)
            {
                throw new InvalidOperationException("The caret was already detroyed.");
            }

            CurrentCaretInfo = caretInfo;
            return SetCaretPos((int)CurrentCaretInfo.Position.X, (int)CurrentCaretInfo.Position.Y);
        }

        /// <summary>
        /// Copies the caret's position to the specified <see cref="CartesianPoint"/> structure.
        /// </summary>
        /// <param name="caretInfo">Defines the properties of the caret.</param>
        /// <returns><c>true</c> if successfull.</returns>
        /// <remarks>The caret position is always given in the client coordinates of the window that contains the caret.
        /// <para>This API does not participate in DPI virtualization. The returned values are interpreted as logical sizes in terms of the window in question. The calling thread is not taken into consideration.</para></remarks>
        /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getcaretpos">GetCaretPos function (winuser.h)</seealso>
        public bool GetPosition(out CartesianPoint caretPsoition) => GetCaretPos(out caretPsoition);

        /// <summary>
        /// Retrieves the time required to invert the caret's pixels. The user can set this value using <see cref="SetBlinkTime(TimeSpan)"/>.
        /// </summary>
        /// <returns>A <see cref="TimeSpan"/> holding the blink time in milliseconds. A return value of <see cref="TimeSpan.Zero"/> indicates that the caret does not blink.</returns>
        /// <remarks>The flash time of a caret is twice as much as the blink time: "Blink" is the period of time between successive state changes in the caret animation. "Flash" is 2 * blink, the period of time to transition from visible, to hidden, to visible again.</remarks>
        /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getcaretblinktime">GetCaretBlinkTime function (winuser.h)</seealso>
        public static TimeSpan GetBlinkTime()
        {
            uint blinkTime = Win32Caret.GetCaretBlinkTime();
            return blinkTime == uint.MaxValue ? TimeSpan.Zero : TimeSpan.FromMilliseconds(blinkTime);
        }

        /// <summary>
        /// Returns the current flash time, which is twice the blink time value returned by <see cref="GetBlinkTime"/>.
        /// </summary>
        /// <returns>A <see cref="TimeSpan"/> holding the flash time in milliseconds.</returns>
        /// <remarks>The flash time of a caret is twice as much as the blink time: "Blink" is the period of time between successive state changes in the caret animation. "Flash" is 2 * blink, the period of time to transition from visible, to hidden, to visible again.</remarks>
#if NET
        public static TimeSpan GetFlashTime() => Win32Caret.GetBlinkTime().Multiply(2);
#else
    public static TimeSpan GetFlashTime() => TimeSpan.FromMilliseconds(2 * Win32Caret.GetBlinkTime().TotalMilliseconds);
#endif

        /// <summary>
        /// Sets the caret blink time to the specified number of milliseconds. The blink time is the elapsed time, in milliseconds, required to invert the caret's pixels.
        /// </summary>
        /// <param name="blinkTimeInMilliseconds">Caret blink rate in milliseconds.</param>
        /// <returns><c>true</c> if successfull.</returns>
        /// <remarks>The user can set the blink time using the Control Panel. Applications should respect the setting that the user has chosen. The SetCaretBlinkTime function should only be used by application that allow the user to set the blink time, such as a Control Panel applet.
        /// <para>The flash time is the elapsed time, in milliseconds, required to display, invert, and restore the caret's display. The flash time of a caret is twice as much as the blink time: "Blink" is the period of time between successive state changes in the caret animation. "Flash" is 2 * blink, the period of time to transition from visible, to hidden, to visible again.</para>
        /// <para>If you change the blink time, subsequently activated applications will use the modified blink time, even if you restore the previous blink time when you lose the keyboard focus or become inactive.This is due to the multithreaded environment, where deactivation of your application is not synchronized with the activation of another application.This feature allows the system to activate another application even if the current application is not responding.</para></remarks>
        /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setcaretblinktime">SetCaretBlinkTime function (winuser.h)</seealso>
        public static bool SetBlinkTime(TimeSpan blinkTimeInMilliseconds) => Win32Caret.SetCaretBlinkTime((uint)blinkTimeInMilliseconds.TotalMilliseconds);

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            _ = Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        protected virtual bool Dispose(bool disposing)
        {
            bool isCaretDestroyed = false;
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                isCaretDestroyed = DestroyCaret();
                disposedValue = true;
            }

            return isCaretDestroyed || disposedValue;
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~Win32Caret()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            _ = Dispose(disposing: false);
        }

        protected bool DisposeInternal()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            bool isDestroyed = Dispose(disposing: true);
            GC.SuppressFinalize(this);
            return isDestroyed;
        }
    }

    public enum CaretVisibility
    {
        Undefined = 0,
        Destroyed,
        Visible,
        Hidden
    }
#endif
}


#if ENABLE_WINMD_SUPPORT
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Runtime.InteropServices;

public static class SwapChainUtilities
{
#if ENABLE_IL2CPP
    [DllImport("__Internal")]
    private static extern int GetPageContent([MarshalAs(UnmanagedType.IInspectable)]object frame, [MarshalAs(UnmanagedType.IInspectable)]out object pageContent);

    public static SwapChainPanel GetMainPanel()
    {
        object pageContent;
        var result = GetPageContent(Windows.UI.Xaml.Window.Current.Content, out pageContent);
        if (result < 0)
            Marshal.ThrowExceptionForHR(result);

        return pageContent as Windows.UI.Xaml.Controls.SwapChainPanel;
    }
#else

    public static SwapChainPanel GetMainPanel()
    {
        if (Windows.UI.Xaml.Window.Current == null)
        {
            throw new System.InvalidOperationException("This method requires XAML build type! D3D is not supported!");
        }
        var frame = Windows.UI.Xaml.Window.Current.Content as Frame;
        var page = frame.Content as Page;
        return page.Content as SwapChainPanel;
    }

#endif
}
#endif
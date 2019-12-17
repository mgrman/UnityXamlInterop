using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

#if ENABLE_WINMD_SUPPORT

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#endif

public class NewBehaviourScript : MonoBehaviour
{
#if ENABLE_WINMD_SUPPORT
    private WebView webView;

    [DllImport("__Internal")]
    private static extern int GetPageContent([MarshalAs(UnmanagedType.IInspectable)]object frame, [MarshalAs(UnmanagedType.IInspectable)]out object pageContent);

    private static Rect RectTransformToScreenSpace(RectTransform transform)
    {
        Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
        return new Rect((Vector2)transform.position - (size * 0.5f), size);
    }

#if ENABLE_IL2CPP
    private SwapChainPanel GetMainPanel()
    {
        object pageContent;
        var result = GetPageContent(Windows.UI.Xaml.Window.Current.Content, out pageContent);
        if (result < 0)
            Marshal.ThrowExceptionForHR(result);

        return pageContent as Windows.UI.Xaml.Controls.SwapChainPanel;
    }
#else

    private SwapChainPanel GetMainPanel()
    {
        var frame = Windows.UI.Xaml.Window.Current.Content as Frame;
        var page = frame.Content as Page;
        Debug.LogError(page.Content.GetType().FullName);
        return page.Content as SwapChainPanel;
    }

#endif

    // Start is called before the first frame update
    private void Start()
    {
        var rectTransform = GetComponent<RectTransform>();
        var screenRect = RectTransformToScreenSpace(rectTransform);

        UnityEngine.WSA.Application.InvokeOnUIThread(() =>
        {
            var panel = GetMainPanel();

            webView = new WebView();
            webView.Source = new System.Uri("http://google.com");
            webView.Width = screenRect.width;
            webView.Height = screenRect.height;
            webView.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
            webView.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
            webView.Margin = new Windows.UI.Xaml.Thickness(screenRect.xMin, screenRect.yMin, 0, 0);

            panel.Children.Add(webView);

            var onResizeHandler = new WindowSizeChangedEventHandler((o, e) => OnResize());
            Window.Current.SizeChanged += onResizeHandler;
            //Debug.LogError(page.GetType().Name);
        }, false);
    }

    private void OnResize()
    {
        UnityEngine.WSA.Application.InvokeOnAppThread(() =>
        {
            var rectTransform = GetComponent<RectTransform>();
            var screenRect = RectTransformToScreenSpace(rectTransform);

            UnityEngine.WSA.Application.InvokeOnUIThread(() =>
            {
                webView.Width = screenRect.width;
                webView.Height = screenRect.height;
                webView.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
                webView.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
                webView.Margin = new Windows.UI.Xaml.Thickness(screenRect.xMin, screenRect.yMin, 0, 0);
            }, false);
        }, false);
    }

#endif
}
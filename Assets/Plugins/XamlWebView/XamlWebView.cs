using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

#if ENABLE_WINMD_SUPPORT
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#endif

[RequireComponent(typeof(RectTransform))]
public class XamlWebView : MonoBehaviour
{
    [SerializeField]
    private string _url = "https://google.com";

    private bool _resizeOnUpdate;
    private bool _resizeOnLateUpdate;
    private Rect _screenRect;

#if ENABLE_WINMD_SUPPORT
    private WebView webView;
    private SwapChainPanel panel;
#endif
    
    // Start is called before the first frame update
    protected virtual void Start()
    {
        UpdateScreenRect();
        CreateXaml();
    }

    protected virtual void LateUpdate()
    {
        if (UpdateScreenRect())
        {
            ResizeXaml();
        }
    }

    protected virtual void OnDestroy()
    {
        DestroyXaml();
    }

    private bool UpdateScreenRect()
    {
        var rectTransform = GetComponent<RectTransform>();
        var newScreenRect = RectTransformToScreenSpace(rectTransform);
        var oldScreenRect = _screenRect;
        _screenRect = newScreenRect;
        return oldScreenRect != newScreenRect;
    }

    private void CreateXaml()
    {
        UnityEngine.WSA.Application.InvokeOnUIThread(() =>
            {
                CreateXamlOnUIThread();
            },
            false);
    }

    private void CreateXamlOnUIThread()
    {
#if ENABLE_WINMD_SUPPORT
            panel = SwapChainUtilities.GetMainPanel();

            webView = new WebView();
            webView.Source = new System.Uri(_url);
            webView.Width = _screenRect.width;
            webView.Height = _screenRect.height;
            webView.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
            webView.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
            webView.Margin = new Windows.UI.Xaml.Thickness(_screenRect.xMin, _screenRect.yMin, 0, 0);

            panel.Children.Add(webView);
#endif
    }

    private void ResizeXaml()
    {
        UnityEngine.WSA.Application.InvokeOnUIThread(() =>
            {
#if ENABLE_WINMD_SUPPORT
               ResizeXamlOnUIThread();
#endif
            },
            false);
    }

    private void ResizeXamlOnUIThread()
    {
#if ENABLE_WINMD_SUPPORT
        webView.Width = _screenRect.width;
        webView.Height = _screenRect.height;
        webView.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
        webView.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
        webView.Margin = new Windows.UI.Xaml.Thickness(_screenRect.xMin, _screenRect.yMin, 0, 0);
#endif
    }

    private void DestroyXaml()
    {
        UnityEngine.WSA.Application.InvokeOnUIThread(() =>
            {
#if ENABLE_WINMD_SUPPORT
               DestroyXamlOnUIThread();
#endif
            },
            false);
    }

    private void DestroyXamlOnUIThread()
    {
#if ENABLE_WINMD_SUPPORT
            panel.Children.Remove(webView);
#endif
    }

    private static Rect RectTransformToScreenSpace(RectTransform transform)
    {
        Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
        return new Rect((Vector2) transform.position - (size * 0.5f), size);
    }
}
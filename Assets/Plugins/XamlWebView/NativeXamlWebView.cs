using System;

#if ENABLE_WINMD_SUPPORT
using XamlWebView.Utilities;
using Windows.UI.Xaml.Controls;
using Windows.Graphics.Display;
#endif

namespace XamlWebView
{
    /// <summary>
    /// All methods must be called on the UWP Apllication's UI thread, via <see cref="UnityEngine.WSA.Application.InvokeOnUIThread"/>
    /// </summary>
    public class NativeXamlWebView : IXamlWebView
    {
#if ENABLE_WINMD_SUPPORT
        private readonly WebView _webView;
        private readonly SwapChainPanel _panel;
#endif

        public event Action<NavigationEndedResponse> NavigationEnded;

        public NativeXamlWebView()
        {
#if ENABLE_WINMD_SUPPORT
            _panel = SwapChainUtilities.GetMainPanel();

            _webView = new WebView();
            _webView.NavigationCompleted += WebView_NavigationCompleted;
            _webView.NavigationFailed += WebView_NavigationFailed;
            _webView.DefaultBackgroundColor = Windows.UI.Colors.Transparent;

            _panel.Children.Add(_webView);
#endif
        }

        public void NavigateToString(string html, Uri baseUri)
        {
#if ENABLE_WINMD_SUPPORT
            var streamUri = _webView.BuildLocalStreamUri(nameof(XamlWebView), UnityStreamingAssetsResolver.HtmlVirtualPath);
            _webView.NavigateToLocalStreamUri(streamUri, new UnityStreamingAssetsResolver(html, baseUri));
#endif
        }

        public void Navigate(Uri uri)
        {
#if ENABLE_WINMD_SUPPORT
            _webView.Navigate(uri);
#endif
        }

#if ENABLE_WINMD_SUPPORT
        private void WebView_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            var response = new NavigationEndedResponse(e.Uri, e.WebErrorStatus.ToString());
            NavigationEnded?.Invoke(response);
        }

        private void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs e)
        {
            NavigationEndedResponse response;
            if (e.IsSuccess)
                response = new NavigationEndedResponse(e.Uri);
            else
                response = new NavigationEndedResponse(e.Uri, e.WebErrorStatus.ToString());

            NavigationEnded?.Invoke(response);
        }
#endif

        public void Resize(UnityEngine.Rect screenRect)
        {
#if ENABLE_WINMD_SUPPORT
            DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();
            var scale = displayInformation.RawPixelsPerViewPixel;
            _webView.Width = screenRect.width / scale;
            _webView.Height = screenRect.height / scale;
            _webView.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
            _webView.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
            _webView.Margin = new Windows.UI.Xaml.Thickness(screenRect.xMin / scale, screenRect.yMin / scale, 0, 0);
#endif
        }

        public void Dispose()
        {
#if ENABLE_WINMD_SUPPORT
            if (_webView != null)
            {
                _webView.NavigationCompleted -= WebView_NavigationCompleted;
                _webView.NavigationFailed -= WebView_NavigationFailed;

                _panel.Children.Remove(_webView);
            }
#endif
        }
    }
}
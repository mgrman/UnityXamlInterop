using System;

namespace XamlWebView
{
    /// <summary>
    /// All methods can be called from any thread
    /// </summary>
    public class ThreadSafeNativeXamlWebView : IXamlWebView
    {
        private NativeXamlWebView _nativeXamlWebView;

        public event Action<NavigationEndedResponse> NavigationEnded;

        public ThreadSafeNativeXamlWebView()
        {
            UnityEngine.WSA.Application.InvokeOnUIThread(() =>
                {
                    _nativeXamlWebView = new NativeXamlWebView();
                    _nativeXamlWebView.NavigationEnded += OnNavigationEnded;
                }, true);
        }

        private void OnNavigationEnded(NavigationEndedResponse obj)
        {
            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                {
                    NavigationEnded?.Invoke(obj);
                }, false);
        }

        public void NavigateToString(string html, Uri baseUri)
        {
            UnityEngine.WSA.Application.InvokeOnUIThread(() =>
                {
                    _nativeXamlWebView.NavigateToString(html, baseUri);
                }, false);
        }

        public void Navigate(Uri uri)
        {
            UnityEngine.WSA.Application.InvokeOnUIThread(() =>
                {
                    _nativeXamlWebView.Navigate(uri);
                }, false);
        }

        public void Resize(UnityEngine.Rect screenRect)
        {
            UnityEngine.WSA.Application.InvokeOnUIThread(() =>
                {
                    _nativeXamlWebView.Resize(screenRect);
                }, false);
        }

        public void Dispose()
        {
            UnityEngine.WSA.Application.InvokeOnUIThread(() =>
                {
                    _nativeXamlWebView.Dispose();
                }, false);
        }
    }
}
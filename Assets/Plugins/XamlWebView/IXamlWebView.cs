using System;

namespace XamlWebView
{
    public interface IXamlWebView : IDisposable
    {
        event Action<NavigationEndedResponse> NavigationEnded;
        void NavigateToString(string html, Uri baseUri);
        void Navigate(Uri uri);
        void Resize(UnityEngine.Rect screenRect);
    }
}
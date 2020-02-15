using System;

namespace XamlWebView
{
    public class NavigationEndedResponse
    {
        public NavigationEndedResponse(Uri uri)
        {
            IsSuccess = true;
            Uri = uri;
            Error = null;
        }

        public NavigationEndedResponse(Uri uri, string error)
        {
            IsSuccess = false;
            Uri = uri;
            Error = error;
        }

        public bool IsSuccess { get; }

        public Uri Uri { get; }

        public string Error { get; }
    }
}
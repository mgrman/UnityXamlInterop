using System;
using UnityEngine;
using XamlWebView.Utilities;

namespace XamlWebView
{
    [RequireComponent(typeof(RectTransform))]
    public class XamlWebViewComponent : MonoBehaviour
    {
        [SerializeField]
        private bool _verbose = true;

        [SerializeField]
        [Tooltip("URL of website to load, if set then HTML content is ignored!")]
        private string _url = "https://google.com";

        [SerializeField]
        [Tooltip("Set this path to where the HTML would be located in streamingAssets, for loading relative resources.")]
        private string _htmlBaseStreamingAssetsUri = "Base/aaa.html";

        [SerializeField]
        [Multiline]
        [Tooltip("HTML string to load. Url must be empty!")]
        private string _html = "<link rel=\"stylesheet\" type=\"text/css\" href=\"aaa.css\"><p>bbbb</p>";

        private static Uri _streamingAssetsUri;

        private Rect _screenRect;
        private IXamlWebView _nativeXamlWebView;

        private string _previousUrl;
        private string _previousBaseUri;
        private string _previousHtml;

        protected virtual void Awake()
        {
            _streamingAssetsUri = new Uri(Application.streamingAssetsPath + "\\", UriKind.Absolute); // must end in slash, so it can be later combined with relative uris, without overriding the last folder
        }

        protected virtual void OnEnable()
        {
            UpdateScreenRect();

            _nativeXamlWebView = new ThreadSafeNativeXamlWebView();
            _nativeXamlWebView.NavigationEnded += OnNavigationEnded;
        }

        protected virtual void LateUpdate()
        {
            if (_nativeXamlWebView == null)
            {
                return;
            }

            if (UpdateScreenRect())
            {
                ResizeXaml();
            }

            if (DidNavigationInfoChanged())
            {
                Navigate();
            }
        }

        protected virtual void OnDisable()
        {
            _nativeXamlWebView?.Dispose();
        }

        private bool UpdateScreenRect()
        {
            var rectTransform = GetComponent<RectTransform>();
            var newScreenRect = rectTransform.RectTransformToScreenSpace();
            var oldScreenRect = _screenRect;
            _screenRect = newScreenRect;
            return oldScreenRect != newScreenRect;
        }

        private bool DidNavigationInfoChanged()
        {
            if (_previousUrl == _url && _previousHtml == _html && _previousBaseUri == _htmlBaseStreamingAssetsUri)
            {
                return false;
            }

            _previousUrl = _url;
            _previousHtml = _html;
            _previousBaseUri = _htmlBaseStreamingAssetsUri;
            return true;
        }

        private void Navigate()
        {
            if (!string.IsNullOrEmpty(_url))
            {
                _nativeXamlWebView.Navigate(new System.Uri(_url));
            }
            else
            {
                var baseRelativeUri = _htmlBaseStreamingAssetsUri.StartsWith("/") ? _htmlBaseStreamingAssetsUri.Substring(1) : _htmlBaseStreamingAssetsUri;
                var baseUri = new Uri(_streamingAssetsUri, baseRelativeUri);

                _nativeXamlWebView.NavigateToString(_html, baseUri);
            }
            _nativeXamlWebView.Resize(_screenRect);
        }

        private void OnNavigationEnded(NavigationEndedResponse obj)
        {
            if (_verbose)
                Debug.Log($"Uri:{obj.Uri} IsSuccess:{obj.IsSuccess} Error:{obj.Error}");
        }

        private void ResizeXaml()
        {
            _nativeXamlWebView.Resize(_screenRect);
        }
    }
}
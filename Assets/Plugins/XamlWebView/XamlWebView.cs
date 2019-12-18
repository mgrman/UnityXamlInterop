using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;

#if ENABLE_WINMD_SUPPORT
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Graphics.Display;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.Web;
using Windows.Storage;

#endif

[RequireComponent(typeof(RectTransform))]
public class XamlWebView : MonoBehaviour
{
    [SerializeField]
    private string _url = "https://google.com";


    [SerializeField]
    private string _html = "https://google.com";

    private static string _streamingAssetsPath;
    private static Uri _streamingAssetsUri;

    private UnityEngine.Rect _screenRect;

#if ENABLE_WINMD_SUPPORT
    private WebView webView;
    private SwapChainPanel panel;
#endif
    
    // Start is called before the first frame update
    protected virtual void Start()
    {
        _html =  "<link rel=\"stylesheet\" type=\"text/css\" href=\"aaa.css\">"+_html;
        _streamingAssetsPath = UnityEngine.Application.streamingAssetsPath+"\\"; // must end in slash, so it can be later combined with relative uris, without overriding the last folder
        _streamingAssetsUri = new Uri(_streamingAssetsPath, UriKind.Absolute);
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
        webView.NavigationCompleted += WebView_NavigationCompleted;
        webView.NavigationFailed += WebView_NavigationFailed;
        if (string.IsNullOrEmpty(_html))
        {
            webView.Source = new System.Uri(_url);
        }
        else
        {
            var normalizedUrl = _url.StartsWith("/") ? _url : $"/{_url}";
            var streamUri = webView.BuildLocalStreamUri("123", normalizedUrl);
            webView.NavigateToLocalStreamUri(streamUri, new Resolver(_html, normalizedUrl));
        }
        ResizeXamlOnUIThread();

        panel.Children.Add(webView);
#endif
    }

#if ENABLE_WINMD_SUPPORT
    private void WebView_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
    {
        Debug.LogError($"{e.Uri} {e.WebErrorStatus}");
    }

    private void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs e)
    {
        Debug.LogError($"{e.Uri} {e.IsSuccess} {e.WebErrorStatus}");
    }

    private class Resolver: IUriToStreamResolver
    {
        private string _html;
        private string _htmlFileName;

        public Resolver(string html, string htmlFileName)
        {
            _html = html;
            _htmlFileName = htmlFileName;
        }

        public IAsyncOperation<IInputStream> UriToStreamAsync(Uri uri)
        {
            if (uri.AbsolutePath == _htmlFileName)
            {
                return ToInputStreamAsync(_html).AsAsyncOperation();
                //return ToInputStreamAsync(_html).AsAsyncOperation();
            }
            var relativePath = uri.AbsolutePath.Substring(1);// no starting slash, so it can be used as relative URI

            var absolutePath = new Uri(_streamingAssetsUri, relativePath);

            return OpenFileAsync(absolutePath.LocalPath).AsAsyncOperation();
        }
        private async Task<IInputStream> OpenFileAsync(string path)
        {
            StorageFile f = await StorageFile.GetFileFromPathAsync(path);
            IRandomAccessStream stream = await f.OpenAsync(FileAccessMode.Read);
            return stream;
        }

        public static async Task<IInputStream> ToInputStreamAsync(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);

            var memoryStream = new InMemoryRandomAccessStream();
            await memoryStream.WriteAsync(bytes.AsBuffer());
            await memoryStream.FlushAsync();
            memoryStream.Seek(0);

            return memoryStream;
        }

    }
#endif

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
        DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();
        var scale = displayInformation.RawPixelsPerViewPixel;
        webView.Width = _screenRect.width/ scale;
        webView.Height = _screenRect.height / scale;
        webView.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
        webView.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
        webView.Margin = new Windows.UI.Xaml.Thickness(_screenRect.xMin / scale, _screenRect.yMin / scale, 0,0 );
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

    public static UnityEngine.Rect RectTransformToScreenSpace(RectTransform transform)
    {
        Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
        UnityEngine.Rect rect = new UnityEngine.Rect(transform.position.x, Screen.height - transform.position.y, size.x, size.y);
        rect.x -= (transform.pivot.x * size.x);
        rect.y -= ((1.0f - transform.pivot.y) * size.y);
        return rect;
    }
}
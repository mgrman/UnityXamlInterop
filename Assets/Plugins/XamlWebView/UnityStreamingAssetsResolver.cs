#if ENABLE_WINMD_SUPPORT
using System;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.Web;
using Windows.Storage;

namespace XamlWebView
{
    internal class UnityStreamingAssetsResolver : IUriToStreamResolver
    {
        public static readonly string HtmlVirtualPath = $"/{Guid.NewGuid()}.html";
        private string _html;
        private Uri _baseUri;

        public UnityStreamingAssetsResolver(string html, Uri baseUri)
        {
            _html = html;
            _baseUri = baseUri;
        }

        public IAsyncOperation<IInputStream> UriToStreamAsync(Uri uri)
        {
            if (uri.AbsolutePath == HtmlVirtualPath)
            {
                return ToInputStreamAsync(_html)
                    .AsAsyncOperation();
            }

            var relativePath = uri.AbsolutePath.Substring(1); // no starting slash, so it can be used as relative URI

            var absolutePath = new Uri(_baseUri, relativePath);

            return OpenFileAsync(absolutePath.LocalPath)
                .AsAsyncOperation();
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
}
#endif
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using SkiaSharp;

namespace ThumbnailGenerator;

public class BingVisualSearch
{
    const string CRLF = "\r\n";
    static string BoundaryTemplate = "batch_{0}";
    static string StartBoundaryTemplate = "--{0}";
    static string EndBoundaryTemplate = "--{0}--";
    private static string? accessKey = Environment.GetEnvironmentVariable("AZURE_APIKEY");
    private static string uriBase = "https://api.bing.microsoft.com/v7.0/images/visualsearch";

    private static HashSet<string> BackupSources = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
    {
        "pintrest", "artstation"
    };
    public static async Task<List<string>> DoSearch(byte[] imageBinary)
    {
        
        
        try
        {
            
            var boundary = string.Format(BoundaryTemplate, Guid.NewGuid());
            var json = await BingImageSearch(imageBinary, boundary, uriBase, accessKey);
            var parsed = JsonSerializer.Deserialize<BingJSONResponse.Root>(json);
            var relatedQuery = from tag in parsed.Tags
                where tag.DisplayName == ""
                from action in tag.Actions
                where action.ActionType == "RelatedSearches"
                from value in action.Data.Value
                select value.Text;

            var tags = from tag in parsed.Tags
                where tag.DisplayName != ""
                select tag.DisplayName;
            
            var visualSearchQuery = from tag in parsed.Tags
                where tag.DisplayName == ""
                from action in tag.Actions
                where action.ActionType == "VisualSearch"
                from value in action.Data.Value
                where BackupSources.Any(bu => value.Name.Contains(bu, StringComparison.CurrentCultureIgnoreCase))
                select value.Name;

            var related = relatedQuery.Take(3).Concat(tags).Concat(visualSearchQuery.Take(3)).ToList();


            return related;
        }
        catch (Exception e)
        {
            throw;
        }
    }

    static async Task<string> BingImageSearch(byte[] image, string boundary, string uri, string subscriptionKey)
    {
        var rencoded = SKBitmap.Decode(image);
        image = rencoded.Encode(SKEncodedImageFormat.Webp, 80).ToArray();
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
        requestMessage.Headers.Add("Ocp-Apim-Subscription-Key", accessKey);

        var content = new MultipartFormDataContent(boundary);
        content.Add(new ByteArrayContent(image), "image", "myimage");
        requestMessage.Content = content;

        var httpClient = new HttpClient();

        var httpResponse = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        HttpStatusCode statusCode = httpResponse.StatusCode;
        HttpContent responseContent = httpResponse.Content;

        string json = null;

        if (responseContent != null)
        {
            Task<String> stringContentsTask = responseContent.ReadAsStringAsync();
            json = stringContentsTask.Result;
        }

        return json;
    }
    

}
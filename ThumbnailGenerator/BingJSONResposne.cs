using System.Text.Json.Serialization;

namespace ThumbnailGenerator.BingJSONResponse;
// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class Instrumentation
    {
        [JsonPropertyName("_type")]
        public string Type { get; set; }
    }

    public class Thumbnail
    {
        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

    public class InsightsMetadata
    {
        [JsonPropertyName("pagesIncludingCount")]
        public int PagesIncludingCount { get; set; }

        [JsonPropertyName("availableSizesCount")]
        public int AvailableSizesCount { get; set; }

        [JsonPropertyName("recipeSourcesCount")]
        public int? RecipeSourcesCount { get; set; }
    }

    public class Value
    {
        [JsonPropertyName("webSearchUrl")]
        public string WebSearchUrl { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("thumbnailUrl")]
        public string ThumbnailUrl { get; set; }

        [JsonPropertyName("datePublished")]
        public DateTime DatePublished { get; set; }

        [JsonPropertyName("isFamilyFriendly")]
        public bool IsFamilyFriendly { get; set; }

        [JsonPropertyName("contentUrl")]
        public string ContentUrl { get; set; }

        [JsonPropertyName("hostPageUrl")]
        public string HostPageUrl { get; set; }

        [JsonPropertyName("contentSize")]
        public string ContentSize { get; set; }

        [JsonPropertyName("encodingFormat")]
        public string EncodingFormat { get; set; }

        [JsonPropertyName("hostPageDisplayUrl")]
        public string HostPageDisplayUrl { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("hostPageDiscoveredDate")]
        public DateTime HostPageDiscoveredDate { get; set; }

        [JsonPropertyName("thumbnail")]
        public Thumbnail Thumbnail { get; set; }

        [JsonPropertyName("imageInsightsToken")]
        public string ImageInsightsToken { get; set; }

        [JsonPropertyName("insightsMetadata")]
        public InsightsMetadata InsightsMetadata { get; set; }

        [JsonPropertyName("imageId")]
        public string ImageId { get; set; }

        [JsonPropertyName("accentColor")]
        public string AccentColor { get; set; }

        [JsonPropertyName("hostPageFavIconUrl")]
        public string HostPageFavIconUrl { get; set; }

        [JsonPropertyName("hostPageDomainFriendlyName")]
        public string HostPageDomainFriendlyName { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("displayText")]
        public string DisplayText { get; set; }
    }

    public class Data
    {
        [JsonPropertyName("value")]
        public List<Value> Value { get; set; }

        [JsonPropertyName("currentOffset")]
        public int? CurrentOffset { get; set; }

        [JsonPropertyName("nextOffset")]
        public int? NextOffset { get; set; }

        [JsonPropertyName("totalEstimatedMatches")]
        public int? TotalEstimatedMatches { get; set; }
    }

    public class Image
    {
        [JsonPropertyName("webSearchUrl")]
        public string WebSearchUrl { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("isFamilyFriendly")]
        public bool IsFamilyFriendly { get; set; }

        [JsonPropertyName("contentSize")]
        public string ContentSize { get; set; }

        [JsonPropertyName("encodingFormat")]
        public string EncodingFormat { get; set; }

        [JsonPropertyName("hostPageDisplayUrl")]
        public string HostPageDisplayUrl { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("thumbnail")]
        public Thumbnail Thumbnail { get; set; }

        [JsonPropertyName("visualWords")]
        public string VisualWords { get; set; }

        [JsonPropertyName("thumbnailUrl")]
        public string ThumbnailUrl { get; set; }

        [JsonPropertyName("imageInsightsToken")]
        public string ImageInsightsToken { get; set; }
    }

    public class Action
    {
        [JsonPropertyName("_type")]
        public string Type { get; set; }

        [JsonPropertyName("actionType")]
        public string ActionType { get; set; }

        [JsonPropertyName("data")]
        public Data Data { get; set; }

        [JsonPropertyName("image")]
        public Image Image { get; set; }

        [JsonPropertyName("webSearchUrl")]
        public string WebSearchUrl { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("serviceUrl")]
        public string ServiceUrl { get; set; }
    }

    public class TopLeft
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }
    }

    public class TopRight
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }
    }

    public class BottomRight
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }
    }

    public class BottomLeft
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }
    }

    public class QueryRectangle
    {
        [JsonPropertyName("topLeft")]
        public TopLeft TopLeft { get; set; }

        [JsonPropertyName("topRight")]
        public TopRight TopRight { get; set; }

        [JsonPropertyName("bottomRight")]
        public BottomRight BottomRight { get; set; }

        [JsonPropertyName("bottomLeft")]
        public BottomLeft BottomLeft { get; set; }
    }

    public class DisplayRectangle
    {
        [JsonPropertyName("topLeft")]
        public TopLeft TopLeft { get; set; }

        [JsonPropertyName("topRight")]
        public TopRight TopRight { get; set; }

        [JsonPropertyName("bottomRight")]
        public BottomRight BottomRight { get; set; }

        [JsonPropertyName("bottomLeft")]
        public BottomLeft BottomLeft { get; set; }
    }

    public class Tag
    {
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("actions")]
        public List<Action> Actions { get; set; }

        [JsonPropertyName("image")]
        public Image Image { get; set; }

    }

    public class DebugInfo
    {
    }

    public class Root
    {
        [JsonPropertyName("_type")]
        public string Type { get; set; }

        [JsonPropertyName("instrumentation")]
        public Instrumentation Instrumentation { get; set; }

        [JsonPropertyName("tags")]
        public List<Tag> Tags { get; set; }

        [JsonPropertyName("image")]
        public Image Image { get; set; }

        [JsonPropertyName("debugInfo")]
        public DebugInfo DebugInfo { get; set; }
    }


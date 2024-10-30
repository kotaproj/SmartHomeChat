using Markdig;

namespace SmartHomeChat.Helper;

public static class MarkdownHelper
{
    public static string RenderMarkdown(string markdown)
    {
        var pipeline = new Markdig.MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        return Markdig.Markdown.ToHtml(markdown, pipeline);
    }
}

namespace BlazorAI.Services
{
    public interface IMarkdownRepository
    {
        Task<string?> GetContentByFilename(string fileName);
    }
}

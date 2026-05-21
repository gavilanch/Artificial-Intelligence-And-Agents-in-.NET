namespace BlazorAI.Services
{
    public class MarkdownRepository(IWebHostEnvironment env) : IMarkdownRepository
    {
        public async Task<string?> GetContentByFilename(string fileName)
        {
            var filesDirectory = Path.Combine(env.ContentRootPath, "markdown-files");
            var completeRoute = Path.Combine(filesDirectory, fileName);

            if (!File.Exists(completeRoute))
            {
                return null;
            }

            return await File.ReadAllTextAsync(completeRoute);
        }
    }
}

using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MCPServer.MCPResources
{
    [McpServerResourceType]
    public class DocumentsResources(IWebHostEnvironment env)
    {
        [McpServerResource(
        UriTemplate = "documents://code-of-conduct",
        MimeType = "text/markdown"),
     Description("Code of conduct of the company in markdown format")]
        public string CodeOfConduct()
        {
            var route = Path.Combine(
                        env.ContentRootPath,
                        "documents",
                        "code-of-conduct.md");

            if (!File.Exists(route))
            {
                return "Document not found";
            }

            return File.ReadAllText(route);

        }

    }
}

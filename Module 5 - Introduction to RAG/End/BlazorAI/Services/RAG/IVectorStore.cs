using Microsoft.AspNetCore.Components.Forms;

namespace BlazorAI.Services.RAG
{
    public interface IVectorStore
    {
        Task UploadFiles(List<IBrowserFile> files, CancellationToken cancellationToken = default);
    }
}

using System.ComponentModel;

namespace ChatbotSimple.Services;

internal class FakeGetEmailService
{
    [Description("Gets a person's email address.")]
    public string GetEmail([Description("Person name")] string name) => $"{name}@example.com";
}

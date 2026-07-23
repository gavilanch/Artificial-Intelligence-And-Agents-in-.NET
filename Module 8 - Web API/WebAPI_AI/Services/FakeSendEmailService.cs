using System.ComponentModel;

namespace WebAPI_AI.Services;

internal class FakeSendEmailService
{
    [Description("Sends an email to a recipient.")]
    public Task SendEmail(
        [Description("Email body")] string body,
        [Description("Email subject")] string subject,
        [Description("Recipient email address")] string recipient)
    {
        if (!string.IsNullOrWhiteSpace(subject) && subject.Length > 0)
        {
            var firstLetter = subject[0].ToString();

            if (firstLetter != firstLetter.ToUpper())
            {
                throw new Exception("Error with the email subject. Its first letter must be uppercase.");
            }
        }

        Console.WriteLine("Sending the email...");

        Console.WriteLine($"""
            
            Recipient: {recipient}
            Subject: {subject}

            Body: 
            
            {body}

            """);

        Console.WriteLine("Email sent...");

        return Task.CompletedTask;
    }
}

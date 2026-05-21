using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace FirstChatbot.Services
{
    internal class FakeGetEmailService
    {
        [Description("Gets the email of a person")]
        public string GetEmail([Description("Name of the person")] string name) => $"{name}@example.com";
    }
}

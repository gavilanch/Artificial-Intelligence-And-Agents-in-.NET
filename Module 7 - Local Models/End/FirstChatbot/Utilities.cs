using System;
using System.Collections.Generic;
using System.Text;

namespace FirstChatbot
{
internal class Utilities
{
    internal static void SetEnvironmentVariables()
    {
        foreach (var line in File.ReadAllLines(".env"))
        {
            // OPENAIKEY=THE_KEY
            var parts = line.Split('=', 2);
            if (parts.Length == 2)
            {
                Environment.SetEnvironmentVariable(parts[0], parts[1]);
            }
        }
    }
}
}

namespace DesktopGremlin.Services
{
    public static class InputParser
    {
        public static ParsedInput Parse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return new ParsedInput { Type = InputType.Empty };

            var trimmed = raw.Trim();

            if (trimmed.StartsWith("/"))
            {
                var parts = trimmed.Split(' ', 2);
                var cmd = parts[0].ToLower();
                var arg = parts.Length > 1 ? parts[1] : string.Empty;

                return cmd switch
                {
                    "/help" => new ParsedInput { Type = InputType.Command, Command = "help" },
                    "/clear" => new ParsedInput { Type = InputType.Command, Command = "clear" },
                    "/quit" => new ParsedInput { Type = InputType.Command, Command = "quit" },
                    _ => new ParsedInput { Type = InputType.Command, Command = "unknown", Argument = trimmed }
                };
            }

            return new ParsedInput { Type = InputType.Message, Message = trimmed };
        }
    }

    public class ParsedInput
    {
        public InputType Type { get; set; }
        public string Command { get; set; } = string.Empty;
        public string Argument { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public enum InputType { Empty, Command, Message }
}

namespace RenzoAgostini.Server.Emailing.Models
{
    public sealed class EmailResult
    {
        public bool Success { get; init; }
        public string? Error { get; init; }

        public static EmailResult Ok() => 
            new() 
            { 
                Success = true 
            };

        public static EmailResult Fail(string error) => 
            new() 
            {
                Success = false, 
                Error = error 
            };
    }
}

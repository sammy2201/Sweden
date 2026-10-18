namespace SwedenStart;

public class ApiException : Exception
{
     public ApiException(
         int statusCode,
         string title,
         string message,
         string? code = null,
         Exception? innerException = null)
         : base(message, innerException)
     {
          StatusCode = statusCode;
          Title = title;
          Code = code;
     }

     public int StatusCode { get; }
     public string Title { get; }
     public string? Code { get; }
}
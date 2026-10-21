namespace SwedenStart;

public class ApiException : Exception
{
     public ApiException(
         int statusCode,
         string title,
         string message)
         : base(message)
     {
          StatusCode = statusCode;
          Title = title;
     }

     public int StatusCode { get; }
     public string Title { get; }

}
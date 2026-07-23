using System.Text.Json.Serialization;

namespace Dsw2026Tpi.CrossCutting.Models;

public record ErrorResponse
{
    public string ErrorCode { get; init; }
    public string Message { get; init; }

   
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ErrorDetail>? Details { get; set; }

    public ErrorResponse(string errorCode, string message)
    {
        ErrorCode = errorCode;
        Message = message;
    }

    public void AddDetail(string field, string issue)
    {
        Details ??= new List<ErrorDetail>();
        Details.Add(new ErrorDetail(field, issue));
    }

    public void AddDetail(IEnumerable<(string, string)> details)
    {
        Details ??= new List<ErrorDetail>();
        foreach (var detail in details)
        {
            Details.Add(new ErrorDetail(detail.Item1, detail.Item2));
        }
    }
}

public record ErrorDetail(string Field, string Issue);
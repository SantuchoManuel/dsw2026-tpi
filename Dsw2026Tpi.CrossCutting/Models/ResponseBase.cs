using System.Text.Json.Serialization;

namespace Dsw2026Tpi.CrossCutting.Models;

public class ErrorResponse
{
    public string ErrorCode { get; set; }
    public string Message { get; set; }

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

public class ErrorDetail
{
    public string Field { get; set; }
    public string Issue { get; set; }

    public ErrorDetail(string field, string issue)
    {
        Field = field;
        Issue = issue;
    }
}
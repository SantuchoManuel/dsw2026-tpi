namespace Dsw2026Tpi.CrossCutting.Exceptions;

/// <summary>
/// Excepción que se lanza cuando ocurre un conflicto, por ejemplo al intentar crear un recurso duplicado.
/// </summary>
public class ConflictException : AppException
{
    public ConflictException(string errorCode, string message)
        : base(message, errorCode)
    {
    }
}

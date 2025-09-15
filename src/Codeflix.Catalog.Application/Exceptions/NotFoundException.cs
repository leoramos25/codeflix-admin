namespace Codeflix.Catalog.Application.Exceptions;

public class NotFoundException(string? message) : Exception(message)
{
    public static void ThrowIfNull(object? @object, string exceptionMessage)
    {
        if (@object is null)
            throw new NotFoundException(exceptionMessage);
    }
}

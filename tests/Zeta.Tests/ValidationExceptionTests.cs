namespace Zeta.Tests;

public class ValidationExceptionTests
{
    [Fact]
    public void Constructor_SetsMessageAndErrors()
    {
        var error1 = new ValidationError("name", "required", "Name is required");
        var error2 = new ValidationError("email", "email", "Invalid email");
        var errors = new List<ValidationError> { error1, error2 };
        var message = "Validation failed";

        var exception = new ValidationException(message, errors);

        Assert.Equal(message, exception.Message);
        Assert.Equal(2, exception.Errors.Count);
        Assert.Same(errors, exception.Errors);
    }

    [Fact]
    public void Errors_Property_ReturnsProvidedErrors()
    {
        var error = new ValidationError("field", "code", "message");
        var errors = new List<ValidationError> { error };

        var exception = new ValidationException("test", errors);

        Assert.Single(exception.Errors);
        Assert.Equal("$.field", exception.Errors[0].Path);
        Assert.Equal("code", exception.Errors[0].Code);
        Assert.Equal("message", exception.Errors[0].Message);
    }

    [Fact]
    public void ValidationException_IsException()
    {
        var exception = new ValidationException("test", new List<ValidationError>());

        Assert.IsAssignableFrom<Exception>(exception);
    }

    [Fact]
    public void ValidationException_CanBeCaught()
    {
        var error = new ValidationError("test", "code", "message");
        var errors = new List<ValidationError> { error };

        try
        {
            throw new ValidationException("Validation failed", errors);
        }
        catch (ValidationException ex)
        {
            Assert.Equal("Validation failed", ex.Message);
            Assert.Single(ex.Errors);
        }
    }

    [Fact]
    public void ValidationException_WithEmptyErrors_Works()
    {
        var exception = new ValidationException("No errors", new List<ValidationError>());

        Assert.Equal("No errors", exception.Message);
        Assert.Empty(exception.Errors);
    }
}

namespace Zeta.Tests;

public class ResultTests
{
    // ==================== Error Deduplication ====================

    [Fact]
    public void Failure_DuplicateErrors_KeepDuplicates()
    {
        var error = new ValidationError("field", "required", "Field is required");
        var result = Result<string>.Failure(error, error, error);

        Assert.Equal(3, result.Errors.Count);
        Assert.Equal("field", result.Errors[0].Path);
    }

    [Fact]
    public void Failure_DuplicateErrorsInList_Deduplicates()
    {
        var error = new ValidationError("email", "email", "Invalid email");
        var errors = new List<ValidationError> { error, error, error };
        var result = Result<string>.Failure(errors);

        Assert.Equal(3, result.Errors.Count);
    }

    [Fact]
    public void Failure_DifferentErrors_KeepsAll()
    {
        var error1 = new ValidationError("name", "required", "Name is required");
        var error2 = new ValidationError("email", "email", "Invalid email");
        var result = Result<string>.Failure(error1, error2);

        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void Failure_SamePathDifferentCode_KeepsBoth()
    {
        var error1 = new ValidationError("password", "min_length", "Too short");
        var error2 = new ValidationError("password", "complexity", "Must have special char");
        var result = Result<string>.Failure(error1, error2);

        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void Failure_SameCodeDifferentPath_KeepsBoth()
    {
        var error1 = new ValidationError("name", "required", "Field is required");
        var error2 = new ValidationError("email", "required", "Field is required");
        var result = Result<string>.Failure(error1, error2);

        Assert.Equal(2, result.Errors.Count);
    }

    // ==================== Basic Result Operations ====================

    [Fact]
    public void Success_IsSuccess_ReturnsTrue()
    {
        var result = Result<int>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Failure_IsFailure_ReturnsTrue()
    {
        var result = Result<int>.Failure(new ValidationError("", "error", "Failed"));

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Map_OnSuccess_TransformsValue()
    {
        var result = Result<int>.Success(10);
        var mapped = result.Map(x => x * 2);

        Assert.True(mapped.IsSuccess);
        Assert.Equal(20, mapped.Value);
    }

    [Fact]
    public void Map_OnFailure_PreservesErrors()
    {
        var error = new ValidationError("field", "code", "message");
        var result = Result<int>.Failure(error);
        var mapped = result.Map(x => x * 2);

        Assert.True(mapped.IsFailure);
        Assert.Single(mapped.Errors);
    }

    [Fact]
    public void GetOrDefault_OnSuccess_ReturnsValue()
    {
        var result = Result<string>.Success("hello");

        Assert.Equal("hello", result.GetOrDefault("fallback"));
    }

    [Fact]
    public void GetOrDefault_OnFailure_ReturnsFallback()
    {
        var result = Result<string>.Failure(new ValidationError("", "e", "m"));

        Assert.Equal("fallback", result.GetOrDefault("fallback"));
    }

    [Fact]
    public void Match_OnSuccess_CallsSuccessHandler()
    {
        var result = Result<int>.Success(5);
        var output = result.Match(
            success: v => $"Value: {v}",
            failure: e => "Error");

        Assert.Equal("Value: 5", output);
    }

    [Fact]
    public void Match_OnFailure_CallsFailureHandler()
    {
        var result = Result<int>.Failure(new ValidationError("", "e", "m"));
        var output = result.Match(
            success: v => $"Value: {v}",
            failure: e => "Error");

        Assert.Equal("Error", output);
    }

    // ==================== Cached Success Tests ====================

    [Fact]
    public void Success_ReferenceType_SameInstance_ReturnsCachedResult()
    {
        var user = new TestUser { Name = "John" };
        var result1 = Result<TestUser>.Success(user);
        var result2 = Result<TestUser>.Success(user);

        Assert.Same(result1, result2); // Same Result instance (cached)
        Assert.Same(user, result1.Value);
        Assert.Same(user, result2.Value);
    }

    [Fact]
    public void Success_ReferenceType_DifferentInstances_ReturnsDifferentResults()
    {
        var user1 = new TestUser { Name = "John" };
        var user2 = new TestUser { Name = "Jane" };
        var result1 = Result<TestUser>.Success(user1);
        var result2 = Result<TestUser>.Success(user2);

        Assert.NotSame(result1, result2); // Different Result instances
        Assert.Same(user1, result1.Value);
        Assert.Same(user2, result2.Value);
    }

    [Fact]
    public void Success_ValueType_AlwaysCreatesNewResult()
    {
        var result1 = Result<int>.Success(42);
        var result2 = Result<int>.Success(42);

        // Value types don't get cached (not reference-equal)
        Assert.NotSame(result1, result2);
        Assert.Equal(42, result1.Value);
        Assert.Equal(42, result2.Value);
    }

    private class TestUser
    {
        public string Name { get; set; } = "";
    }
}

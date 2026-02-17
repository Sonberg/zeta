namespace Zeta.Tests;

public class ResultExtensionsTests
{
    // ==================== Combine Tests ====================

    [Fact]
    public void Combine_AllSuccess_ReturnsSuccessWithArray()
    {
        var results = new[]
        {
            Result<int>.Success(1),
            Result<int>.Success(2),
            Result<int>.Success(3)
        };

        var combined = results.Combine();

        Assert.True(combined.IsSuccess);
        Assert.Equal(3, combined.Value.Length);
        Assert.Equal(1, combined.Value[0]);
        Assert.Equal(2, combined.Value[1]);
        Assert.Equal(3, combined.Value[2]);
    }

    [Fact]
    public void Combine_SomeFailures_ReturnsFailureWithAllErrors()
    {
        var error1 = new ValidationError("field1", "code1", "Error 1");
        var error2 = new ValidationError("field2", "code2", "Error 2");
        
        var results = new[]
        {
            Result<int>.Success(1),
            Result<int>.Failure(error1),
            Result<int>.Success(3),
            Result<int>.Failure(error2)
        };

        var combined = results.Combine();

        Assert.True(combined.IsFailure);
        Assert.Equal(2, combined.Errors.Count);
        Assert.Contains(combined.Errors, e => e.Path == "$.field1");
        Assert.Contains(combined.Errors, e => e.Path == "$.field2");
    }

    [Fact]
    public void Combine_AllFailures_ReturnsFailureWithAllErrors()
    {
        var error1 = new ValidationError("field1", "code1", "Error 1");
        var error2 = new ValidationError("field2", "code2", "Error 2");
        var error3 = new ValidationError("field3", "code3", "Error 3");
        
        var results = new[]
        {
            Result<int>.Failure(error1),
            Result<int>.Failure(error2),
            Result<int>.Failure(error3)
        };

        var combined = results.Combine();

        Assert.True(combined.IsFailure);
        Assert.Equal(3, combined.Errors.Count);
    }

    [Fact]
    public void Combine_EmptyCollection_ReturnsSuccessWithEmptyArray()
    {
        var results = Array.Empty<Result<int>>();

        var combined = results.Combine();

        Assert.True(combined.IsSuccess);
        Assert.Empty(combined.Value);
    }

    [Fact]
    public void Combine_SingleSuccess_ReturnsSuccessWithSingleElementArray()
    {
        var results = new[] { Result<string>.Success("test") };

        var combined = results.Combine();

        Assert.True(combined.IsSuccess);
        Assert.Single(combined.Value);
        Assert.Equal("test", combined.Value[0]);
    }

    // ==================== Async Then Extension Tests ====================

    [Fact]
    public async Task Then_AsyncExtension_OnSuccess_ChainsValidation()
    {
        var task = Task.FromResult(Result<int>.Success(10));
        
        var result = await task.Then(async x =>
        {
            await Task.Delay(1);
            return Result<string>.Success($"Value: {x}");
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("Value: 10", result.Value);
    }

    [Fact]
    public async Task Then_AsyncExtension_OnFailure_PreservesErrors()
    {
        var error = new ValidationError("field", "code", "message");
        var task = Task.FromResult(Result<int>.Failure(error));
        
        var result = await task.Then(async x =>
        {
            await Task.Delay(1);
            return Result<string>.Success($"Value: {x}");
        });

        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Equal("$.field", result.Errors[0].Path);
    }

    // ==================== Async Map Extension Tests ====================

    [Fact]
    public async Task Map_AsyncExtension_OnSuccess_TransformsValue()
    {
        var task = Task.FromResult(Result<int>.Success(10));
        
        var result = await task.Map(x => x * 2);

        Assert.True(result.IsSuccess);
        Assert.Equal(20, result.Value);
    }

    [Fact]
    public async Task Map_AsyncExtension_OnFailure_PreservesErrors()
    {
        var error = new ValidationError("field", "code", "message");
        var task = Task.FromResult(Result<int>.Failure(error));
        
        var result = await task.Map(x => x * 2);

        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Equal("$.field", result.Errors[0].Path);
    }

    [Fact]
    public async Task Map_AsyncExtension_WithComplexTransformation_Works()
    {
        var task = Task.FromResult(Result<string>.Success("hello"));
        
        var result = await task.Map(s => new { Value = s, Length = s.Length });

        Assert.True(result.IsSuccess);
        Assert.Equal("hello", result.Value.Value);
        Assert.Equal(5, result.Value.Length);
    }
}

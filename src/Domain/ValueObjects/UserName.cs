using FluentResults;

namespace Domain.ValueObjects;

public sealed class UserName : ValueObject
{
    public Guid UserID  { get; private set; } // has to be here or else EF Core will cry
    public string Value { get; }
  
    private UserName(string value)
    {
        Value = value;
    }
    
    public static Result<UserName> Create(string value)
    {
        if (value.Length < 3 || value.Length > 50)
        {
            return Result.Fail<UserName>("Username must be between 3 and 50 characters long.");
        }
        return new UserName(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value; // returning the value for equality comparison
    }
}
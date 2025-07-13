using System.Text.RegularExpressions;
using FluentResults;

namespace Domain.ValueObjects;

public sealed class Email : ValueObject
{
    public Guid UserID  { get; private set; } // has to be here or else EF Core will cry
    public string Value { get; }
    
    private Email(string value)
    {
        Value = value;
    }
    
    public static Result<Email> Create(string value)
    {
        if (!Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            return Result.Fail<Email>("Invalid email format.");
        }
        return new Email(value);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value; // returning the value for equality comparison
    }
}
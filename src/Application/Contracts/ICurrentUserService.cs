using Domain.ValueObjects;

namespace Application.Contracts;

public interface ICurrentUserService
{
    Guid UserID { get; }
    UserName UserName { get; }
}

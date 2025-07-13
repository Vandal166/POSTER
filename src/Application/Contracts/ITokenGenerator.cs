using Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace Application.Contracts;

public interface ITokenGenerator
{
    string GenerateToken(User user, IConfiguration configuration);
}
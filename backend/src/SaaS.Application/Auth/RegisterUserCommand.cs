using MediatR;

namespace SaaS.Application.Auth;

public record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    bool AcceptTerms) : IRequest<RegisterUserResult>;

public record RegisterUserResult;

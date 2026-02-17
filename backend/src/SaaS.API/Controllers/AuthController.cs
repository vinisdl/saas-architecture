using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SaaS.Application.Auth;

namespace SaaS.API.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [EnableRateLimiting("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (request.Password != request.ConfirmPassword)
            return BadRequest(new { error = "Password and confirmation do not match." });
        if (!request.AcceptTerms)
            return BadRequest(new { error = "You must accept the terms and privacy policy." });
        if (string.IsNullOrWhiteSpace(request.Email) || !IsValidEmail(request.Email))
            return BadRequest(new { error = "Invalid email address." });
        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { error = "Password is required." });

        try
        {
            await _mediator.Send(new RegisterUserCommand(
                request.FirstName ?? "",
                request.LastName ?? "",
                request.Email.Trim(),
                request.Password,
                request.AcceptTerms), cancellationToken);
            return StatusCode(StatusCodes.Status201Created, new { message = "Account created. Please sign in." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

public record RegisterRequest(
    [Required] [MinLength(1)] string FirstName,
    [Required] [MinLength(1)] string LastName,
    [Required] [EmailAddress] string Email,
    [Required] [MinLength(8)] string Password,
    [Required] string ConfirmPassword,
    bool AcceptTerms);

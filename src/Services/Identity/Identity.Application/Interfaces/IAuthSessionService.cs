using System;
using Identity.Application.DTOs;
using Identity.Domain.Entities;

namespace Identity.Application.Interfaces;

public interface IAuthSessionService
{
    Task<TokenResponse> CreateSessionAsync(User user, CancellationToken ct = default);
}

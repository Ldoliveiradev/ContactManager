using ContactManager.Application.Auth.Models;
using ContactManager.Application.Auth.Models.Dto;
using ContactManager.Application.Auth.Models.Requests;
using ContactManager.Application.Auth.Models.Responses;

namespace ContactManager.Application.Auth.Interfaces;

public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<ChangePasswordResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default);
}

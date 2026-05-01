using Application.Core;
using Application.DTOs.ViolationDTOs;

namespace Application.Interfaces.Services;

public interface IAdminService
{
    Task<Result<bool>> UnblockUserAsync(string userId, CancellationToken ct);
    Task<Result<bool>> DeletePublicationAsync(CreateViolationDto createDto, string adminId, CancellationToken ct);
    Task<Result<bool>> DeleteCommentAsync(CreateViolationDto createDto, CancellationToken ct);
    Task<Result<bool>> BlockUserAsync(string userId, string blockedById, CancellationToken ct);
}
using Application.DTOs.ViolationDTOs;

namespace Application.Interfaces.Services;

public interface IViolationService
{
    Task<List<ViolationDto>> GetViolationsByUserId(string userId, CancellationToken ct);
    Task<bool> CreateViolation(Violation violation, CancellationToken ct);
}
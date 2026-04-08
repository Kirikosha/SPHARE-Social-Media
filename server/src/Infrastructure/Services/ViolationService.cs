using Application.DTOs.ViolationDTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;

namespace Infrastructure.Services;

public class ViolationService(IViolationRepository violationRepository, IMapper mapper) : IViolationService
{
    public async Task<List<ViolationDto>> GetViolationsByUserId(string userId, CancellationToken ct)
    {
        var violations = await violationRepository.GetViolationsByUserId(userId, ct);
        return mapper.Map<List<ViolationDto>>(violations);
    }

    public async Task<bool> CreateViolation(Violation violation, CancellationToken ct)
    {
        try
        {
            await violationRepository.CreateViolation(violation, ct);
            return true;
        }
        catch(Exception)
        {
            return false;
        }
    }
}
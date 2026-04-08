using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ViolationRepository(ApplicationDbContext context) : IViolationRepository
{
    //TODO: maybe some limit or batching required
    public async Task<List<Violation>> GetViolationsByUserId(string userId, CancellationToken ct)
    {
        List<Violation> violations = await context.Violations.Where(a => a.ViolatedById == userId)
            .ToListAsync(ct);
        return violations;
    }

    public async Task CreateViolation(Violation violation, CancellationToken ct)
    {
        await context.Violations.AddAsync(violation, ct);
    }
}
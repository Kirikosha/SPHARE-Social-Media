namespace Application.Interfaces.Repositories;

public interface IViolationRepository
{
    Task<List<Violation>> GetViolationsByUserId(string userId, CancellationToken ct);
    Task CreateViolation(Violation violation, CancellationToken ct);
}
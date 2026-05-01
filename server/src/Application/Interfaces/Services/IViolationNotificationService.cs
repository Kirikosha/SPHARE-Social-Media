using Application.Core;

namespace Application.Interfaces.Services;

using Domain.Entities;
using System.Threading.Tasks;

public interface IViolationNotificationService
{
    Task<Result<bool>> RegisterViolationAsync(Violation violation, int scoreIncrease, bool isPublication,
        CancellationToken ct);
}

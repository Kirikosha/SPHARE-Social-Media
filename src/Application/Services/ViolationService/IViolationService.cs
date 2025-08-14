namespace Application.Services.ViolationService;

using Domain.Entities;
using System.Threading.Tasks;

public interface IViolationService
{
    Task<bool> RegisterViolationAsync(User user, Violation violation, int scoreIncrease, bool isPublication);
}

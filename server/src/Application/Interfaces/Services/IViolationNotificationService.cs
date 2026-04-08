namespace Application.Interfaces.Services;

using Domain.Entities;
using System.Threading.Tasks;

public interface IViolationNotificationService
{
    Task<bool> RegisterViolationAsync(User user, Violation violation, int scoreIncrease, bool isPublication);
}

using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Additions
{
    public interface IAuthorizationService
    {
        bool ResetPasswordToGenerated(Employee employee, IUnitOfWork uow);
        bool ResetPassword(Employee employee, string password, IUnitOfWork uow);
        bool TryToSaveUser(Employee employee, IUnitOfWork uow);
    }
}
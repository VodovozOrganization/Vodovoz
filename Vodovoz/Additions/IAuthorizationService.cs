using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Additions
{
    public interface IAuthorizationService
    {
        bool ResetPasswordToGenerated(string userLogin, string email);
        bool ResetPassword(string userLogin, string password, string email);
        bool TryToSaveUser(Employee employee, IUnitOfWork uow);
    }
}
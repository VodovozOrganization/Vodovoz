using InstantSmsService;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Additions
{
    public interface IAuthorizationService
    {
        ResultMessage ResetPasswordToGenerated(Employee employee, int passwordLength);
        ResultMessage ResetPassword(Employee employee, string password);
        bool TryToSaveUser(Employee employee, IUnitOfWork uow);
        
    }
}
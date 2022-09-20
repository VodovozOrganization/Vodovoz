using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.Infrastructure.Services
{
    public interface IAuthorizationService
    {
        bool TryToSaveUser(Employee employee, IUnitOfWork uow);
		bool ResetPasswordToGenerated(string userLogin, string email, string fullName);
		bool ResetPassword(string userLogin, string password, string email, string fullname);
	}
}
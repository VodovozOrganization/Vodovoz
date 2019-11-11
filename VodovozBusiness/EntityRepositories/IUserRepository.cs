using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;

namespace Vodovoz.EntityRepositories
{
	public interface IUserRepository
	{
		User GetCurrentUser(IUnitOfWork uow);
		UserSettings GetCurrentUserSettings(IUnitOfWork uow);
		string GetTempDirForCurrentUser(IUnitOfWork uow);
		UserSettings GetUserSettings(IUnitOfWork uow, int userId);
	}
}
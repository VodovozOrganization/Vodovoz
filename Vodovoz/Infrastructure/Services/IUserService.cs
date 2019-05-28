using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
namespace Vodovoz.Infrastructure.Services
{
	public interface IUserService
	{
		int CurrentUserId { get; }
		UserBase GetCurrentUser(IUnitOfWork uow);
	}
}

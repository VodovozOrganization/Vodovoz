using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Repositories;

namespace Vodovoz.Infrastructure.Services
{
	public class UserService : IUserService
	{
		public UserService()
		{
			 CurrentUserId = UserRepository.GetCurrentUserId();
		}

		public int CurrentUserId { get; }

		public UserBase GetCurrentUser(IUnitOfWork uow)
		{
			return uow.GetById<UserBase>(CurrentUserId);
		}
	}
}

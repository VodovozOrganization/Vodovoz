using System;
using System.IO;
using QS.DomainModel.UoW;
using QS.Project.Services;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Repositories.HumanResources
{
	[Obsolete("Используйте одноимённый класс из Vodovoz.EntityRepositories.UserSingletonRepository")]
	public static class UserRepository
	{
		public static User GetCurrentUser (IUnitOfWork uow)
		{
			return uow.Session.QueryOver<User> ()
				.Where (u => u.Id == ServicesConfig.UserService.CurrentUserId)
				.SingleOrDefault ();
		}

		public static string GetTempDirForCurrentUser(IUnitOfWork uow)
		{
			var userId = GetCurrentUser(uow)?.Id;

			if(userId == null)
				return string.Empty;

			return Path.Combine(Path.GetTempPath(), "Vodovoz", userId.ToString());
		}

		/// <summary>
		/// По возможности не используйте напрямую этот метод, для получения настроек используйте класс CurrentUserSettings
		/// </summary>
		public static UserSettings GetCurrentUserSettings (IUnitOfWork uow)
		{
			return uow.Session.QueryOver<UserSettings> ()
				.Where (s => s.User.Id == ServicesConfig.UserService.CurrentUserId)
				.SingleOrDefault ();
		}

	}
}


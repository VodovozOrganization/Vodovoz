using System;
using System.IO;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;

namespace Vodovoz.EntityRepositories
{
	public class UserSingletonRepository : IUserRepository
	{
		private static UserSingletonRepository instance;

		public static UserSingletonRepository GetInstance()
		{
			if(instance == null)
				instance = new UserSingletonRepository();
			return instance;
		}

		protected UserSingletonRepository() { }

		public User GetCurrentUser(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<User>()
				.Where(u => u.Id == QSProjectsLib.QSMain.User.Id)
				.SingleOrDefault();
		}

		public string GetTempDirForCurrentUser(IUnitOfWork uow)
		{
			var userId = GetCurrentUser(uow)?.Id;

			if(userId == null)
				return string.Empty;

			return Path.Combine(Path.GetTempPath(), "Vodovoz", userId.ToString());
		}

		/// <summary>
		/// По возможности не используйте напрямую этот метод, для получения настроек используйте класс CurrentUserSettings
		/// </summary>
		public UserSettings GetCurrentUserSettings(IUnitOfWork uow)
		{
			return GetUserSettings(uow, QSProjectsLib.QSMain.User.Id);
		}

		public UserSettings GetUserSettings(IUnitOfWork uow, int userId)
		{
			return uow.Session.QueryOver<UserSettings>()
				.Where(s => s.User.Id == userId)
				.SingleOrDefault();
		}
	}
}

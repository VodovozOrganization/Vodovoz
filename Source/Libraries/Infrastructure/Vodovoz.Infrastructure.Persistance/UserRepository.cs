using System.IO;
using System.Linq;
using NHibernate;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Services;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Core.Domain.Users.Settings;
using Vodovoz.Domain.HistoryChanges;
using Vodovoz.EntityRepositories;

namespace Vodovoz.Infrastructure.Persistance
{
	internal sealed class UserRepository : IUserRepository
	{
		public User GetCurrentUser(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<User>()
				.Where(u => u.Id == ServicesConfig.UserService.CurrentUserId)
				.SingleOrDefault();
		}

		public User GetUserById(IUnitOfWork uow, int id)
		{
			return uow.GetById<User>(id);
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
			return GetUserSettings(uow, ServicesConfig.UserService.CurrentUserId);
		}

		public UserSettings GetUserSettings(IUnitOfWork uow, int userId)
		{
			return uow.Session.QueryOver<UserSettings>()
				.Where(s => s.User.Id == userId)
				.SingleOrDefault();
		}

		public User GetUserByLogin(IUnitOfWork uow, string login)
		{
			return uow.Session.QueryOver<User>()
				.Where(e => e.Login == login)
				.SingleOrDefault();
		}

		public bool MySQLUserWithLoginExists(IUnitOfWork uow, string login)
		{
			var query = $"SELECT COUNT(*) AS c from mysql.user WHERE USER = '{login}'";
			int count = uow.Session.CreateSQLQuery(query).AddScalar("c", NHibernateUtil.Int32).List<int>().FirstOrDefault();
			return count > 0;
		}

		public void GiveSelectPrivilegesToArchiveDataBase(IUnitOfWork uow, string login)
		{
			var archivedChangedEntitySchema = OrmConfig.FindMappingByShortClassName(nameof(ArchivedChangedEntity)).Table.Schema;

			var sql = $"GRANT Select ON `{archivedChangedEntitySchema}`.* TO '{login}'";
			uow.Session.CreateSQLQuery(sql).ExecuteUpdate();
		}

		public void ChangePasswordForUser(IUnitOfWork uow, string login, string password)
		{
			var sql = $"SET PASSWORD FOR '{login}' = PASSWORD('{password}')";
			uow.Session.CreateSQLQuery(sql).ExecuteUpdate();
		}

		public void CreateUser(IUnitOfWork uow, string login, string password)
		{
			var query = $"CREATE USER '{login}' IDENTIFIED BY '{password}'";
			uow.Session.CreateSQLQuery(query).ExecuteUpdate();
		}

		public void DropUser(IUnitOfWork uow, string login)
		{
			var query = $"DROP USER '{login}'";
			uow.Session.CreateSQLQuery(query).ExecuteUpdate();
		}

		public void GrantPrivilegesToUser(IUnitOfWork uow, string privileges, string tableName, string login)
		{
			var query = $"GRANT {privileges} ON `{tableName}`.* TO '{login}'";
			uow.Session.CreateSQLQuery(query).ExecuteUpdate();
		}

		public void GrantPrivilegesToNewUser(IUnitOfWork uow, string tableName, string login)
		{
			var privileges = "SELECT, INSERT, UPDATE, DELETE, EXECUTE";
			GrantPrivilegesToUser(uow, privileges, tableName, login);
		}
	}
}

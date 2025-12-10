using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Users;
using Vodovoz.EntityRepositories.Permissions;

namespace Vodovoz.Infrastructure.Persistance.Permissions
{
	internal sealed class UserRoleRepository : IUserRoleRepository
	{
		public IList<AvailableDatabase> GetAllAvailableDatabases(IUnitOfWork uow) => uow.GetAll<AvailableDatabase>().ToList();
		public IList<UserRole> GetAllUserRoles(IUnitOfWork uow) => uow.GetAll<UserRole>().ToList();

		public bool IsUserRoleWithSameNameExists(IUnitOfWork uow, string name)
		{
			return uow.Session.QueryOver<UserRole>()
				.Where(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase))
				.SingleOrDefault() != null;
		}

		public AvailableDatabase GetAvailableDatabaseById(IUnitOfWork uow, int id) => uow.GetById<AvailableDatabase>(id);

		public UserRole GetUserRoleById(IUnitOfWork uow, int id)
		{
			return uow.GetById<UserRole>(id);
		}

		public void CreateUserRoleIfNotExists(IUnitOfWork uow, string role)
		{
			var sql = $"CREATE ROLE IF NOT EXISTS '{role}'";
			uow.Session.CreateSQLQuery(sql).ExecuteUpdate();
		}

		public void GrantPrivilegeToRole(IUnitOfWork uow, string privilege, string role)
		{
			var sql = $"GRANT {privilege} TO '{role}'";
			uow.Session.CreateSQLQuery(sql).ExecuteUpdate();
		}

		public void RevokePrivilegeFromRole(IUnitOfWork uow, string privilege, string role)
		{
			var sql = $"REVOKE {privilege} FROM '{role}'";
			uow.Session.CreateSQLQuery(sql).ExecuteUpdate();
		}

		public void RevokeAllPrivilegesFromRole(IUnitOfWork uow, string role)
		{
			var sql = $"REVOKE ALL PRIVILEGES, GRANT OPTION FROM '{role}'";
			uow.Session.CreateSQLQuery(sql).ExecuteUpdate();
		}

		public IEnumerable<string> ShowGrantsForUser(IUnitOfWork uow, string login)
		{
			return uow.Session.CreateSQLQuery($"Show Grants For '{login}'")
				.AddScalar($"Grants for {login}@%", NHibernateUtil.String)
				.List<string>();
		}

		public IEnumerable<string> ShowGrantsForRole(IUnitOfWork uow, string role)
		{
			return uow.Session.CreateSQLQuery($"Show Grants For {role}")
				.List<string>();
		}

		public void SetDefaultRoleToUser(IUnitOfWork uow, UserRole role, string login)
		{
			var roleName = role != null ? $"'{role.Name}'" : "NONE";
			SetDefaultRoleToUser(uow, roleName, login);
		}

		public void SetDefaultRoleToUser(IUnitOfWork uow, string role, string login)
		{
			var sql = $"SET DEFAULT ROLE {role} FOR '{login}'";
			uow.Session.CreateSQLQuery(sql).ExecuteUpdate();
		}

		public void GrantRoleToUser(IUnitOfWork uow, string role, string login, bool withAdminOption = false)
		{
			var adminOption = withAdminOption ? "WITH ADMIN OPTION" : "";
			var sql = $"GRANT '{role}' TO '{login}' {adminOption}";
			uow.Session.CreateSQLQuery(sql).ExecuteUpdate();
		}

		public void RevokeRoleFromUser(IUnitOfWork uow, string role, string login)
		{
			var sql = $"REVOKE '{role}' FROM '{login}'";
			uow.Session.CreateSQLQuery(sql).ExecuteUpdate();
		}
	}
}

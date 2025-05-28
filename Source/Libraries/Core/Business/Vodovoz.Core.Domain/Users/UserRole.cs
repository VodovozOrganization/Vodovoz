using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Permissions;

namespace Vodovoz.Domain.Permissions
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "роли пользователей при работе с БД",
		Nominative = "роль пользователя при работе с БД")]
	public class UserRole : PropertyChangedBase, IDomainObject
	{
		private string _name;
		private string _description;
		
		private IList<User> _users = new List<User>();
		private IList<AvailableDatabase> _availableDatabases = new List<AvailableDatabase>();
		private IList<PrivilegeBase> _privileges = new List<PrivilegeBase>();
		private GenericObservableList<PrivilegeBase> _observablePrivileges;
		private GenericObservableList<AvailableDatabase> _observableAvailableDatabases;

		public static string UserRoleName = "USER";
		public static string UserFinancierRoleName = "USER_FINANCIER";
		
		public virtual int Id { get; set; }

		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}
		
		[Display(Name = "Описание")]
		public virtual string Description
		{
			get => _description;
			set => SetField(ref _description, value);
		}

		[Display(Name = "Пользователи")]
		public virtual IList<User> Users
		{
			get => _users;
			set => SetField(ref _users, value);
		}

		[Display(Name = "Привилегии")]
		public virtual IList<PrivilegeBase> Privileges
		{
			get => _privileges;
			set => SetField(ref _privileges, value);
		}
		
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<PrivilegeBase> ObservablePrivileges =>
			_observablePrivileges ?? (_observablePrivileges = new GenericObservableList<PrivilegeBase>(Privileges));

		[Display(Name = "Доступные базы данных")]
		public virtual IList<AvailableDatabase> AvailableDatabases
		{
			get => _availableDatabases;
			set => SetField(ref _availableDatabases, value);
		}
		
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<AvailableDatabase> ObservableAvailableDatabases =>
			_observableAvailableDatabases ??
			(_observableAvailableDatabases = new GenericObservableList<AvailableDatabase>(AvailableDatabases));

		public virtual void GrantPrivileges(IUnitOfWork uow, IUserRoleRepository userRoleRepository)
		{
			if(string.IsNullOrWhiteSpace(Name))
			{
				return;
			}
			
			foreach(var privilege in Privileges)
			{
				userRoleRepository.GrantPrivilegeToRole(uow, privilege.ToString(), Name);
			}
		}

		public virtual void AddAvailableDatabase(AvailableDatabase availableDatabase)
		{
			if(!ObservableAvailableDatabases.Contains(availableDatabase))
			{
				ObservableAvailableDatabases.Add(availableDatabase);
			}
		}

		public static string SearchingPatternFromUserGrants(string login) => $"GRANT [`|']?(\\w+)[`|']? TO [`|']?{login}[`|']?@[`|']?%[`|']?";
	}
}

using System.Collections.Generic;
using QS.DomainModel.Entity;
using QS.Utilities.Text;

namespace Vodovoz.Core.Data.Employees
{
	public class EmployeeWithLogin : IDomainObject
	{
		//Нужен для Nhibernate
		protected EmployeeWithLogin() { }
		
		private EmployeeWithLogin(int id, string name, string lastName, string patronymic)
		{
			Id = id;
			Name = name;
			LastName = lastName;
			Patronymic = patronymic;
		}
		
		public virtual int Id { get; }
		public virtual string LastName { get; }
		public virtual string Name { get; }
		public virtual string Patronymic { get; }
		public virtual IList<ExternalApplicationUserForApi> ExternalApplicationUsers { get; } = new List<ExternalApplicationUserForApi>();
		public virtual string ShortName => PersonHelper.PersonNameWithInitials(LastName, Name, Patronymic);
		public virtual string FullName => PersonHelper.PersonFullName(LastName, Name, Patronymic);

		public static EmployeeWithLogin Create(int id, string name, string lastName, string patronymic)
		{
			return new EmployeeWithLogin(id, name, lastName, patronymic);
		}
	}
}

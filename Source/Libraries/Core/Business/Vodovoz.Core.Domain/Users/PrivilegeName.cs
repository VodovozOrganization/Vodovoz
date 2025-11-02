using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.Users
{
	/// <summary>
	/// Имя привилегии, которое используется для определения прав доступа к базам данных.
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Feminine,
		Accusative = "имя привелегии",
		AccusativePlural = "имена привелегий",
		Genitive = "имени привелегии",
		GenitivePlural = "имен привелегий",
		Nominative = "имя привелегии",
		NominativePlural = "имена привелегий",
		Prepositional = "имени привелегии",
		PrepositionalPlural = "именах привелегий")]
	public class PrivilegeName : IDomainObject
	{
		private int _id;
		private string _name;
		private PrivilegeType _privilegeType;
		private IList<AvailableDatabase> _unavailableDatabases = new List<AvailableDatabase>();

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id
		{
			get => _id;
			set => _id = value;
		}

		/// <summary>
		/// Имя
		/// </summary>
		[Display(Name = "Имя")]
		public virtual string Name
		{
			get => _name;
			set => _name = value;
		}

		/// <summary>
		/// Тип
		/// </summary>
		[Display(Name = "Тип")]
		public virtual PrivilegeType PrivilegeType
		{
			get => _privilegeType;
			set => _privilegeType = value;
		}

		/// <summary>
		/// Список недоступных баз данных для данной привилегии
		/// </summary>
		[Display(Name = "Недоступные БД")]
		public virtual IList<AvailableDatabase> UnavailableDatabases
		{
			get => _unavailableDatabases;
			set => _unavailableDatabases = value;
		}
	}
}

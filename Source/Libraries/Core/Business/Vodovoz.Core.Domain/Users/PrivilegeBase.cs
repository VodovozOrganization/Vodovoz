using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Users;

namespace Vodovoz.Domain.Permissions
{
	/// <summary>
	/// Базовый класс для привилегий, которые определяют права доступа к базам данных.
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Feminine,
		Accusative = "привилегия работы с БД",
		AccusativePlural = "привилегии работы с БД",
		Genitive = "привилегии работы с БД",
		GenitivePlural = "привилегий работы с БД",
		Nominative = "привилегия работы с БД",
		NominativePlural = "привилегии работы с БД",
		Prepositional = "привилегии работы с БД",
		PrepositionalPlural = "привилегиях работы с БД")]
	public abstract class PrivilegeBase : PropertyChangedBase, IDomainObject
	{
		private UserRole _userRole;
		private PrivilegeName _privilegeName;
		private string _databaseName;
		private string _tableName;
		private int _id;

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Роль пользователя
		/// </summary>
		[Display(Name = "Роль пользователя")]
		public virtual UserRole UserRole
		{
			get => _userRole;
			set => SetField(ref _userRole, value);
		}

		/// <summary>
		/// Имя привилегии
		/// </summary>
		[Display(Name = "Имя привилегии")]
		public virtual PrivilegeName PrivilegeName
		{
			get => _privilegeName;
			set => SetField(ref _privilegeName, value);
		}

		/// <summary>
		/// Тип привилегии
		/// </summary>
		[Display(Name = "Тип")]
		public abstract PrivilegeType PrivilegeType { get; }

		/// <summary>
		/// База данных
		/// </summary>
		[Display(Name = "База данных")]
		public virtual string DatabaseName
		{
			get => _databaseName;
			set => SetField(ref _databaseName, value);
		}

		/// <summary>
		/// Имя таблицы в базе данных
		/// </summary>
		[Display(Name = "Имя таблицы БД")]
		public virtual string TableName
		{
			get => _tableName;
			set => SetField(ref _tableName, value);
		}

		/// <summary>
		/// Переопределение метода ToString для удобства отображения привилегии.
		/// </summary>
		/// <returns></returns>
		public override string ToString() =>
			PrivilegeType == PrivilegeType.GlobalPrivilege
				? $"{PrivilegeName.Name} ON {DatabaseName}.{TableName}"
				: $"{PrivilegeName.Name} ON `{DatabaseName}`.{TableName}";
	}
}

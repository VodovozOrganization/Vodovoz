using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Permissions
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "привилегии работы с БД",
		Nominative = "привилегия работы с БД")]
	public abstract class PrivilegeBase : PropertyChangedBase, IDomainObject
	{
		private UserRole _userRole;
		private PrivilegeName _privilegeName;
		private string _databaseName;
		private string _tableName;

		public virtual int Id { get; set; }

		[Display(Name = "Роль пользователя")]
		public virtual UserRole UserRole
		{
			get => _userRole;
			set => SetField(ref _userRole, value);
		}

		[Display(Name = "Имя привилегии")]
		public virtual PrivilegeName PrivilegeName
		{
			get => _privilegeName;
			set => SetField(ref _privilegeName, value);
		}

		[Display(Name = "Тип")]
		public abstract PrivilegeType PrivilegeType { get; }

		[Display(Name = "База данных")]
		public virtual string DatabaseName
		{
			get => _databaseName;
			set => SetField(ref _databaseName, value);
		}

		[Display(Name = "Имя таблицы БД")]
		public virtual string TableName
		{
			get => _tableName;
			set => SetField(ref _tableName, value);
		}

		public override string ToString() =>
			PrivilegeType == PrivilegeType.GlobalPrivilege
				? $"{PrivilegeName.Name} ON {DatabaseName}.{TableName}"
				: $"{PrivilegeName.Name} ON `{DatabaseName}`.{TableName}";
	}

	public enum PrivilegeType
	{
		GlobalPrivilege,
		DatabasePrivilege,
		TablePrivilege,
		SpecialPrivilege
	}
}

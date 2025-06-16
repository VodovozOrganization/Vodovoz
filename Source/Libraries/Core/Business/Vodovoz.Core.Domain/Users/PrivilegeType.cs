namespace Vodovoz.Core.Domain.Users
{
	/// <summary>
	/// Типы привилегий для работы с БД.
	/// </summary>
	public enum PrivilegeType
	{
		/// <summary>
		/// Глобальная привилегия, которая применяется ко всем базам данных и таблицам.
		/// </summary>
		GlobalPrivilege,
		/// <summary>
		/// Привилегия, которая применяется к конкретной базе данных.
		/// </summary>
		DatabasePrivilege,
		/// <summary>
		/// Привилегия, которая применяется к конкретной таблице в базе данных.
		/// </summary>
		TablePrivilege,
		/// <summary>
		/// Специальная привилегия, которая может включать в себя дополнительные права или ограничения.
		/// </summary>
		SpecialPrivilege
	}
}

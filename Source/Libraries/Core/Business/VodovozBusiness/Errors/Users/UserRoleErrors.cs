using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Users
{
	public static class UserRoleErrors
	{
		/// <summary>
		/// Роль не обнаружена на сервере
		/// </summary>
		/// <returns></returns>
		public static Error UserRoleFromServerNotExists() =>
			new Error(
				"UserRoleFromServerNotExists",
				"Роль не обнаружена на сервере");
		
		/// <summary>
		/// Привилегии роли для данной схемы или таблицы не обнаружены на сервере
		/// </summary>
		/// <returns></returns>
		public static Error PrivilegesOnThisSchemaOrDatabaseFromServerNotExists() =>
			new Error(
				"PrivilegesOnThisSchemaOrDatabaseFromServerNotExists",
				"Не найдены привилегии для этой схемы или таблицы на сервере");
		
		/// <summary>
		/// Количество привилегий роли на сервере не совпадает с программой
		/// </summary>
		/// <returns></returns>
		public static Error CountPrivilegesFromServerAndProgramNotEqual() =>
			new Error(
				"CountPrivilegesFromServerAndProgramNotEqual",
				"Количество привилегий у роли на сервере и у программы не совпадает");
		
		/// <summary>
		/// Привилегии роли на сервере не совпадают с программой
		/// </summary>
		/// <returns></returns>
		public static Error PrivilegesFromServerAndProgramNotSame() =>
			new Error(
				"PrivilegesFromServerAndProgramNotSame",
				"Привилегии у роли на сервере имеют другой состав чем в программе");
	}
}

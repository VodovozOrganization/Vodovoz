using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	/// <summary>
	/// ЭДО
	/// </summary>
	public static partial class EdoPermissions
	{
		/// <summary>
		/// Разрешено закрывать ЭДО задачу по Тендеру
		/// </summary>
		public static string CanCloseTenderEdoTask => nameof(CanCloseTenderEdoTask);

		/// <summary>
		/// Пользователь имеет доступ к справочнику настроек ЭДО-уведомлений
		/// </summary>
		[Display(
			Name = "Работа со справочником настроек ЭДО-уведомлений",
			Description = "Пользователь имеет доступ к справочнику настроек ЭДО-уведомлений")]
		public static string CanChangeEdoNotificationSettings => "CanChangeEdoNotificationSettings";

		/// <summary>
		/// Пользователь может переотправлять документы ЭДО с подбором новых кодов ЧЗ из пула.
		/// </summary>
		[Display(
			Name = "Переотправка документов ЭДО с кодами ЧЗ из пула",
			Description = "Пользователь может переотправлять документы ЭДО с подбором новых кодов ЧЗ из пула")]
		public static string CanResendEdoDocumentWithCodesFromPool => nameof(CanResendEdoDocumentWithCodesFromPool);

		/// <summary>
		/// Пользователь может переотправлять незавершённый УПД на другой аккаунт ЭДО клиента.
		/// </summary>
		[Display(
			Name = "Переотправка незавершённого УПД на другой аккаунт ЭДО",
			Description = "Пользователь может отправить незавершённый УПД на аннулирование и сразу переотправить его на другой аккаунт ЭДО клиента")]
		public static string CanResendEdoDocumentToChangedAccount => nameof(CanResendEdoDocumentToChangedAccount);
	}
}

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
	}
}

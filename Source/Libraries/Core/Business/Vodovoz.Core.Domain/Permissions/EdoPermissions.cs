using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	/// <summary>
	/// ЭДО
	/// </summary>
	public static partial class EdoPermissions
	{
		/// <summary>
		/// Пользователь может удалять лишние коды маркировки из документа.
		/// </summary>
		[Display(
			Name = "Удаление лишних кодов маркировки",
			Description = "Пользователь может удалять выбранные лишние коды маркировки из документа")]
		public static string CanDeleteExcessTrueMarkCodes => nameof(CanDeleteExcessTrueMarkCodes);

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
		/// Пользователь может переотправлять действующий УПД с аннулированием текущего документооборота.
		/// </summary>
		[Display(
			Name = "Переотправка действующего УПД с аннулированием",
			Description = "Пользователь может переотправить УПД в статусе «В процессе» или «Отправлен» с аннулированием текущего документооборота")]
		public static string CanResendEdoDocumentWithCancellation => nameof(CanResendEdoDocumentWithCancellation);

		/// <summary>
		/// Пользователь может работать с журналом расхождений ЭДО.
		/// </summary>
		[Display(
			Name = "Работа с журналом расхождений ЭДО",
			Description = "Пользователь может работать с журналом расхождений ЭДО")]
		public static string CanWorkWithEdoDiscrepancyJournal =>
			"can_work_with_edo_discrepancy_journal";

		/// <summary>
		/// Пользователь может аннулировать успешно завершенный ЭДО.
		/// </summary>
		[Display(
			Name = "Аннулирование успешно завершенного ЭДО",
			Description = "Пользователь может аннулировать успешно завершенный ЭДО")]
		public static string CanAnnulSuccessfullyCompletedEdo =>
			"can_annul_successfully_completed_edo";
	}
}

using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	public static partial class BookkeeppingPermissions
	{
		/// <summary>
		/// Отправка ЭДО документов за прошлые бухгалтерские периоды
		/// </summary>
		[Display(
			Name = "Отправка ЭДО документов за прошлые бухгалтерские периоды",
			Description = "Пользователь может переотправлять документы ЭДО за закрытый бухгалтерский период, "
				+ "а также редактировать даты закрытия бухгалтерского периода в общих настройках")]
		public static string CanSendEdoDocumentsForPreviousPeriods => "can_send_edo_documents_for_previous_periods";
	}
}

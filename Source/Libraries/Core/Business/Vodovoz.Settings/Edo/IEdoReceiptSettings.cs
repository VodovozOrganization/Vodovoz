namespace Vodovoz.Settings.Edo
{
	public interface IEdoReceiptSettings
	{
		/// <summary>
		/// Адрес API сервиса ЭДО чеков
		/// </summary>
		string EdoReceiptApiUrl { get; }


		/// <summary>
		/// Id текущего регламентирующего документа для отраслевых реквизитов
		/// Необходим для разрешительного режима в чеках
		/// </summary>
		int IndustryRequisiteRegulatoryDocumentId { get; }


		/// <summary>
		/// Максимальное количество кодов ЧЗ в одном чеке
		/// </summary>
		int MaxCodesInReceiptCount { get; }
	}
}

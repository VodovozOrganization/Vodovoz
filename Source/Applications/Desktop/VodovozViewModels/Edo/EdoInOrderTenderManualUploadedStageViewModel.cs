using QS.ViewModels;

namespace Vodovoz.ViewModels.Edo
{
	/// <summary>
	/// Блок стадии «Коды выгружены вручную» тендера на вкладке «ЭДО» заказа
	/// </summary>
	public class EdoInOrderTenderManualUploadedStageViewModel : WidgetViewModelBase
	{
		/// <summary>
		/// Описание того, что происходит на стадии
		/// </summary>
		public string Description =>
			"Сотрудник выгрузил коды маркировки и подтвердил, что загрузил их в ЕИС. Обработка тендера завершена.";
	}
}

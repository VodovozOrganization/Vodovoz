using QS.ViewModels;

namespace Vodovoz.ViewModels.Edo
{
	/// <summary>
	/// Блок стадии «Отправляется» тендера на вкладке «ЭДО» заказа
	/// </summary>
	public class EdoInOrderTenderSendingStageViewModel : WidgetViewModelBase
	{
		private const string _description =
			"Коды маркировки подготовлены и находятся на нужной организации. Документ ждёт, когда сотрудник вручную выгрузит коды "
			+ "и загрузит их в ЕИС. Выгрузка выполняется из задачи по госзаказу в журнале «Документооборот с клиентами».";

		public EdoInOrderTenderSendingStageViewModel()
		{
			Description = _description;
		}

		/// <summary>
		/// Описание того, что происходит на стадии
		/// </summary>
		public string Description { get; }
	}
}

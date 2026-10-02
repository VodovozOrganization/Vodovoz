using QS.ViewModels;

namespace Vodovoz.ViewModels.Edo
{
	/// <summary>
	/// Блок стадии трансфера «Ожидает запросов» на вкладке «ЭДО» заказа
	/// </summary>
	public class EdoInOrderTransferWaitingRequestsStageViewModel : WidgetViewModelBase
	{
		private const string _description =
			"Коды с небольших заказов передаются между организациями не по одному заказу, а пакетом. На этой стадии трансфер копит "
			+ "запросы от разных заказов в том же направлении (с той же организации на ту же организацию). Трансфер переходит "
			+ "к подготовке, когда наберётся нужное количество кодов или истечёт время ожидания. Оба значения задаются в настройках.\n"
			+ "Если в одном заказе много кодов, стадия пропускается, и трансфер сразу переходит к подготовке.";

		public EdoInOrderTransferWaitingRequestsStageViewModel()
		{
			Description = _description;
		}

		/// <summary>
		/// Описание того, что происходит на стадии
		/// </summary>
		public string Description { get; }
	}
}

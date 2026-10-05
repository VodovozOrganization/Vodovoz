using QS.ViewModels;

namespace Vodovoz.ViewModels.Edo
{
	/// <summary>
	/// Блок стадии «Забор кодов» чека на вкладке «ЭДО» заказа
	/// </summary>
	public class EdoInOrderSavedToPoolStageViewModel : WidgetViewModelBase
	{
		/// <summary>
		/// Описание того, что происходит на стадии
		/// </summary>
		public string Description =>
			"Коды маркировки из заказа сохранены в пул кодов организации и будут использованы позже при формировании чеков.\n";
	}
}

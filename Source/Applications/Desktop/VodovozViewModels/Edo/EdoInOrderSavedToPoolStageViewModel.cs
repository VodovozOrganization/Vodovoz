using QS.ViewModels;

namespace Vodovoz.ViewModels.Edo
{
	/// <summary>
	/// Блок стадии «Забор кодов» чека на вкладке «ЭДО» заказа
	/// </summary>
	public class EdoInOrderSavedToPoolStageViewModel : WidgetViewModelBase
	{
		private const string _description =
			"Коды маркировки из заказа сохранены в пул кодов организации и будут использованы позже при формировании чеков.\n";

		public EdoInOrderSavedToPoolStageViewModel()
		{
			Description = _description;
		}

		/// <summary>
		/// Описание того, что происходит на стадии
		/// </summary>
		public string Description { get; }
	}
}

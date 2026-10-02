using QS.ViewModels;

namespace Vodovoz.ViewModels.Edo
{
	/// <summary>
	/// Блок стадии «Забор кодов» чека на вкладке «ЭДО» заказа
	/// </summary>
	public class EdoInOrderSavedToPoolStageViewModel : WidgetViewModelBase
	{
		private const string _description =
			"Чек по заказу не пробивался. Коды маркировки из заказа сохранены в пул кодов организации и будут использованы позже "
			+ "при формировании других чеков. Это штатное завершение обработки, а не ошибка, поэтому дальнейших стадий нет.\n"
			+ "Так происходит, если одновременно выполнены условия:\n"
			+ "• заказ оплачен наличными;\n"
			+ "• сегодня уже был пробит чек на такую же сумму;\n"
			+ "• у клиента не включён признак «всегда отправлять чеки»;\n"
			+ "• отправка не была запущена пользователем вручную.";

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

using QS.ViewModels;

namespace Vodovoz.ViewModels.Edo
{
	/// <summary>
	/// Блок стадии «Распределение» документа (УПД, чек, тендер) на вкладке «ЭДО» заказа
	/// </summary>
	public class EdoInOrderDistributionStageViewModel : WidgetViewModelBase
	{
		private const string _commonDescription =
			"На этой стадии система готовит документ к отправке.\n"
			+ "Тип документа (УПД, чек или тендер) выбран при создании задачи: "
			+ "он зависит от формы оплаты заказа и от того, дал ли клиент согласие на ЭДО.\n"
			+ "Система проверяет коды маркировки из заказа в Честном знаке (действительны ли коды и на какой организации они числятся)\n"
			+ "и распределяет коды по товарам заказа. По результату проверки определяется следующий шаг:\n"
			+ "• все коды уже числятся на организации из заказа - документ сразу переходит к отправке;\n"
			+ "• часть кодов числится на другой организации - сначала выполняется трансфер кодов на организацию из заказа.\n"
			+ "\n"
			+ "Если документ долго остаётся на этой стадии, причину можно посмотреть на вкладке «Проблемы».";

		private const string _receiptDescriptionAddition =
			"Для чека на этой стадии также решается, нужно ли сохранять коды в пул (стадия «Забор кодов»).\n"
			+ "При необходимости недостающие или неподходящие коды подбираются из пула кодов организации.";

		/// <param name="documentType">Тип документа, для чека к описанию добавляется дополнение о решении пробивать чек</param>
		public EdoInOrderDistributionStageViewModel(EdoInOrderDocumentType documentType)
		{
			Description = documentType == EdoInOrderDocumentType.Receipt
				? _commonDescription + "\n\n" + _receiptDescriptionAddition
				: _commonDescription;
		}

		/// <summary>
		/// Описание того, что происходит на стадии
		/// </summary>
		public string Description { get; }
	}
}

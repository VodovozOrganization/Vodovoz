using QS.ViewModels;

namespace Vodovoz.ViewModels.Edo
{
	/// <summary>
	/// Блок стадии трансфера «Подготовка» на вкладке «ЭДО» заказа
	/// </summary>
	public class EdoInOrderTransferPreparingStageViewModel : WidgetViewModelBase
	{
		private const string _description =
			"Система формирует документ внутренней продажи (УПД) между организациями для передачи кодов маркировки.\n"
			+ "Когда документ будет сформирован, трансфер перейдёт на стадию «Отправляется» "
			+ "и данные об отправке в Такском появятся на следующих стадиях.";

		public EdoInOrderTransferPreparingStageViewModel()
		{
			Description = _description;
		}

		/// <summary>
		/// Описание того, что происходит на стадии
		/// </summary>
		public string Description { get; }
	}
}

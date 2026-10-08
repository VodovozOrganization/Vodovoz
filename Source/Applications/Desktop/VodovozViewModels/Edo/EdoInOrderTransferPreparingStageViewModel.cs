using QS.ViewModels;

namespace Vodovoz.ViewModels.Edo
{
	/// <summary>
	/// Блок стадии трансфера «Подготовка» на вкладке «ЭДО» заказа
	/// </summary>
	public class EdoInOrderTransferPreparingStageViewModel : WidgetViewModelBase
	{
		/// <summary>
		/// Описание того, что происходит на стадии
		/// </summary>
		public string Description =>
			"Система формирует документ внутренней продажи (УПД) между организациями для передачи кодов маркировки.\n"
			+ "Когда документ будет сформирован, трансфер перейдёт на стадию «Отправляется» "
			+ "и данные об отправке в Такском появятся на следующих стадиях.";
	}
}

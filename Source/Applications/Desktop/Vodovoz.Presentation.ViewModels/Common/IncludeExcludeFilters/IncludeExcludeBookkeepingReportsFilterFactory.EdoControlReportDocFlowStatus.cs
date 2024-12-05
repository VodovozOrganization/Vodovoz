using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters
{
	public partial class IncludeExcludeBookkeepingReportsFilterFactory
	{
		[Appellative(
			Nominative = "Стататус документооборота",
			NominativePlural = "Статусы документооборота")]
		public enum EdoControlReportDocFlowStatus
		{
			[Display(Name = "Неизвестно")]
			Unknown,
			[Display(Name = "В процессе")]
			InProgress,
			[Display(Name = "Документооборот завершен успешно")]
			Succeed,
			[Display(Name = "Предупреждение")]
			Warning,
			[Display(Name = "Ошибка")]
			Error,
			[Display(Name = "Не начат")]
			NotStarted,
			[Display(Name = "Завершен с различиями")]
			CompletedWithDivergences,
			[Display(Name = "Не принят")]
			NotAccepted,
			[Display(Name = "Ожидает аннулирования")]
			WaitingForCancellation,
			[Display(Name = "Аннулирован")]
			Cancelled,
			[Display(Name = "Подготовка к отправке")]
			PreparingToSend,
			[Display(Name = "Не отправлялся в ЭДО")]
			Unsended,
		}
	}
}

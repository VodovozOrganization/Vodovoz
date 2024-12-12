using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.Counterparties
{
	public partial class CallTaskFilterViewModel
	{
		public enum SortingParamType
		{
			[Display(Name = "Клиент")] Client,
			[Display(Name = "Адрес")] DeliveryPoint,
			[Display(Name = "№")] Id,
			[Display(Name = "Долг по адресу")] DebtByAddress,
			[Display(Name = "Долг по клиенту")] DebtByClient,
			[Display(Name = "Создатель задачи")] Deadline,
			[Display(Name = "Ответственный")] AssignedEmployee,
			[Display(Name = "Статус")] Status,
			[Display(Name = "Срочность")] ImportanceDegree
		}
	}
}

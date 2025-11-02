using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.ReportsParameters.Bottles
{
	public partial class ShortfallBattlesReportViewModel
	{
		public enum Drivers
		{
			[Display(Name = "Все")]
			AllDriver = -1,
			[Display(Name = "Отзвон не с адреса")]
			CallFromAnywhere = 3,
			[Display(Name = "Без отзвона")]
			NoCall = 2,
			[Display(Name = "Ларгусы")]
			Largus = 1,
			[Display(Name = "Наемники")]
			Hirelings = 0
		}
	}
}

using System.ComponentModel.DataAnnotations;

namespace Vodovoz
{
	public partial class RouteListClosingDlg
	{
		public enum RouteListPrintDocuments
		{
			[Display(Name = "Все")]
			All,
			[Display(Name = "Маршрутный лист")]
			RouteList,
			[Display(Name = "Штрафы")]
			Fines
		}

		#endregion
	}

}

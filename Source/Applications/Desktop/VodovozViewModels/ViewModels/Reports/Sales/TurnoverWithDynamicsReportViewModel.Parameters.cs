using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Vodovoz.ViewModels.Reports.Sales
{
	public partial class TurnoverWithDynamicsReportViewModel
    {
		public static class SliceValues
		{
			public static string Day => "Day";

			public static string Week => "Week";

			public static string Month => "Month";

			public static string Quarter => "Quarter";

			public static string Year => "Year";

			private static readonly IReadOnlyCollection<string> _values =
				new ReadOnlyCollection<string>(new string[] { Day, Week, Month, Quarter, Year });

			public static bool Contains(string value) => _values.Contains(value);
		}
    }
}

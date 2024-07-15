using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Errors;
using Vodovoz.Presentation.ViewModels.Reports;

namespace Vodovoz.Presentation.ViewModels.Store.Reports
{
	public class TurnoverOfWarehouseBalancesReport : IClosedXmlReport
	{
		private TurnoverOfWarehouseBalancesReport()
		{
			CreatedAt = DateTime.Now;
		}

		public string TemplatePath => throw new NotImplementedException();

		public DateTime CreatedAt { get; }

		public static async Task<Result<TurnoverOfWarehouseBalancesReport>> Generate(CancellationToken cancellationToken)
		{
			return Result.Failure<TurnoverOfWarehouseBalancesReport>(new Error("DebugError", "Test Error"));

			return new TurnoverOfWarehouseBalancesReport();
		}
	}
}

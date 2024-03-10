using QS.DomainModel.UoW;
using System.Linq;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;

namespace Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis
{
	public partial class PaymentsDiscrepanciesAnalysisViewModel
	{
		/// <summary>
		/// Оборотно-сальдовая ведомость
		/// </summary>
		public class TurnoverBalanceSheet
		{
			private static readonly OrderStatus[] _availableOrderStatuses = new OrderStatus[]
			{
				OrderStatus.Accepted,
				OrderStatus.InTravelList,
				OrderStatus.OnLoading,
				OrderStatus.OnTheWay,
				OrderStatus.Shipped,
				OrderStatus.UnloadingOnStock,
				OrderStatus.Closed
			};

			private TurnoverBalanceSheet()
			{
				
			}

			public static TurnoverBalanceSheet CreateFromXls(
				IUnitOfWork unitOfWork,
				ICounterpartyRepository counterpartyRepository,
				string fileName)
			{
				var counterpartiesDebts = counterpartyRepository.GetCounterpartiesCashlessBalance(unitOfWork, _availableOrderStatuses).ToList();

				var rowsFromXls = XlsParseHelper.GetRowsFromXls(fileName);

				return new TurnoverBalanceSheet();
			}
		}
	}
}

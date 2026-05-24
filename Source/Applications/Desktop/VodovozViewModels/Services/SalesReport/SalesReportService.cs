using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.EntityRepositories.Sale;
using VodovozBusiness.Nodes.SalesReport;

namespace Vodovoz.ViewModels.Services.SalesReport
{
	public class SalesReportService : ISalesReportService
	{
		private readonly ISalesReportRepository _salesReportRepository;
		private readonly INomenclatureSettings _nomenclatureSettings;

		public SalesReportService(
			ISalesReportRepository salesReportRepository,
			INomenclatureSettings nomenclatureSettings
			)
		{
			_salesReportRepository = salesReportRepository ?? throw new ArgumentNullException(nameof(salesReportRepository));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
		}

		public async Task<IList<SalesReportDataNode>> GetSalesReportDataAsync(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate,
			OrderDateFilterType orderDateType,
			SalesReportFilters filters,
			CancellationToken cancellationToken = default)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			return await _salesReportRepository.GetSalesReportData(uow, startDate, endDate, orderDateType, filters, cancellationToken);
		}

		public async Task<BottlesDataNode> GetBottlesDataAsync(
			IUnitOfWork uow,
			IEnumerable<int> orderIds,
			CancellationToken cancellationToken = default)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(orderIds is null || !orderIds.Any())
			{
				return new BottlesDataNode { Plan = 0, FactFromRouteList = 0, FactFromSelfDelivery = 0};
			}

			return await _salesReportRepository.GetBottlesData(
				uow,
				orderIds,
				_nomenclatureSettings.DefaultBottleNomenclatureId,
				cancellationToken);
		}
	}
}

using MoreLinq;
using NHibernate.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Presentation.ViewModels.Reports;

namespace Vodovoz.ViewModels.Orders.Reports.PotentialFreePromosets
{
	public partial class PotentialFreePromosetsReport
	{
		private const string _dateFormatString = "dd.MM.yyyy";

		private static IList<OrderStatus> _notDeliveredOrderStatuses =
			new List<OrderStatus> { OrderStatus.Canceled, OrderStatus.NotDelivered, OrderStatus.DeliveryCanceled };

		private readonly DateTime _startDate;
		private readonly DateTime _endDate;
		private readonly List<int> _selectedPromosets = new List<int>();

		private PotentialFreePromosetsReport(
			DateTime startDate,
			DateTime endDate,
			IEnumerable<int> selectedPromosets)
		{
			_startDate = startDate;
			_endDate = endDate;
			_selectedPromosets = (selectedPromosets ?? new List<int>()).ToList();
		}

		public string Title =>
			$"Отчет по потенциальным халявщикам c {_startDate.ToString(_dateFormatString)} по {_endDate.ToString(_dateFormatString)}";

		public List<PromosetReportRow> Rows { get; private set; }
			= new List<PromosetReportRow>();

		public async static Task<PotentialFreePromosetsReport> Create(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate,
			IEnumerable<int> selectedPromosets,
			CancellationToken cancellationToken)
		{
			var report = new PotentialFreePromosetsReport(startDate, endDate, selectedPromosets);
			await report.SetReportRows(uow, cancellationToken);

			return report;
		}
	}
}

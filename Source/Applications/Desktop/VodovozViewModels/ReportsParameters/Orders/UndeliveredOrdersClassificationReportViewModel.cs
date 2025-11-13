using Gamma.Utilities;
using QS.Report;
using QS.Report.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;

namespace Vodovoz.ViewModels.ReportsParameters.Orders
{
	public class UndeliveredOrdersClassificationReportViewModel : ReportParametersViewModelBase
	{
		private UndeliveredOrdersFilterViewModel _filterViewModel;

		public UndeliveredOrdersClassificationReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory
			) : base(rdlViewerViewModel, reportInfoFactory)
		{
			Title = "Сводка по классификации недовозов";
		}

		public void Load(UndeliveredOrdersFilterViewModel filterViewModel, bool withTransfer)
		{
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));

			Identifier = withTransfer
				? "Orders.UndeliveredOrdersClassificationWithTransferReport"
				: "Orders.UndeliveredOrdersClassificationReport";

			LoadReport();
		}

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				if(_filterViewModel == null)
				{
					return null;
				}

				string[] guiltySides = { "0" };
				if(_filterViewModel.RestrictGuiltySide.HasValue)
				{
					guiltySides = new[] { _filterViewModel.RestrictGuiltySide.Value.ToString() };
				}
				if(_filterViewModel.RestrictIsProblematicCases)
				{
					guiltySides = Enum.GetValues(typeof(GuiltyTypes))
						.Cast<GuiltyTypes>()
						.Where(t => !_filterViewModel.ExcludingGuiltiesForProblematicCases.Contains(t))
						.Select(g => g.ToString())
						.ToArray();
				}

				var parameters = new Dictionary<string, object>
				{
					{ "filter", GenerateTitle(_filterViewModel) },
					{ "driver_id", _filterViewModel.RestrictDriver?.Id ?? 0},
					{ "old_order_id", _filterViewModel.RestrictOldOrder?.Id ?? 0},
					{ "client_id", _filterViewModel.RestrictClient?.Id ?? 0},
					{ "address_id", _filterViewModel.RestrictAddress?.Id ?? 0},
					{ "author_subdivision_id", _filterViewModel.RestrictAuthorSubdivision?.Id ?? 0},
					{ "old_order_author_id", _filterViewModel.RestrictOldOrderAuthor?.Id ?? 0},
					{ "old_order_start_date", _filterViewModel.RestrictOldOrderStartDate},
					{ "old_order_end_date", _filterViewModel.RestrictOldOrderEndDate},
					{ "new_order_start_date", _filterViewModel.RestrictNewOrderStartDate},
					{ "new_order_end_date", _filterViewModel.RestrictNewOrderEndDate},
					{ "actions_with_invoice", _filterViewModel.RestrictActionsWithInvoice},
					{ "guilty_sides", guiltySides },
					{ "old_order_status", _filterViewModel.OldOrderStatus },
					{ "guilty_department_id", _filterViewModel.RestrictGuiltyDepartment?.Id ?? 0 },
					{ "in_process_at_subdivision_id", _filterViewModel.RestrictInProcessAtDepartment?.Id ?? 0 },
					{ "undelivery_status", _filterViewModel.RestrictUndeliveryStatus },
					{ "undelivery_author_id", _filterViewModel.RestrictUndeliveryAuthor?.Id ?? 0 }
				};

				return parameters;
			}
		}

		private string GenerateTitle(UndeliveredOrdersFilterViewModel filter)
		{
			var generateDate = $"Время выгрузки: {DateTime.Now}";
			StringBuilder title = new StringBuilder(generateDate);

			if(filter.RestrictOldOrder != null)
			{
				title.Append($", заказ: {filter.RestrictOldOrder.Id}");
			}

			if(filter.RestrictDriver != null)
			{
				title.Append($", водитель: {filter.RestrictDriver.GetPersonNameWithInitials()}");
			}

			if(filter.RestrictClient != null)
			{
				title.Append($", клиент: {filter.RestrictClient.Name}");
			}

			if(filter.RestrictAddress != null)
			{
				title.Append($", адрес: {filter.RestrictAddress.ShortAddress}");
			}

			if(filter.RestrictOldOrderAuthor != null)
			{
				title.Append($", автор заказа: {filter.RestrictOldOrderAuthor.GetPersonNameWithInitials()}");
			}

			if(filter.RestrictOldOrderStartDate != null)
			{
				title.Append($", дата заказа от: {filter.RestrictOldOrderStartDate:d}");
			}

			if(filter.RestrictOldOrderEndDate != null)
			{
				title.Append($", дата заказа до: {filter.RestrictOldOrderEndDate:d}");
			}

			if(filter.RestrictNewOrderStartDate != null)
			{
				title.Append($", дата переноса от: {filter.RestrictNewOrderStartDate:d}");
			}

			if(filter.RestrictNewOrderEndDate != null)
			{
				title.Append($", дата переноса до: {filter.RestrictNewOrderEndDate:d}");
			}

			if(filter.RestrictActionsWithInvoice != null)
			{
				title.Append($", действие с накладной: {filter.RestrictActionsWithInvoice.GetEnumTitle()}");
			}

			if(filter.RestrictAuthorSubdivision != null)
			{
				title.Append($", подразделение автора: {filter.RestrictAuthorSubdivision.ShortName}");
			}

			if(filter.OldOrderStatus != null)
			{
				title.Append($", начальный статус заказа: {filter.OldOrderStatus.GetEnumTitle()}");
			}

			if(filter.RestrictUndeliveryStatus != null)
			{
				title.Append($", статус недовоза: {filter.RestrictUndeliveryStatus.GetEnumTitle()}");
			}

			if(filter.RestrictGuiltySide != null && filter.RestrictGuiltyDepartment == null)
			{
				title.Append($", ответственный: {filter.RestrictGuiltySide.GetEnumTitle()}");
			}

			if(filter.RestrictGuiltyDepartment != null)
			{
				title.Append($", ответственное подразделение: {filter.RestrictGuiltyDepartment.ShortName}");
			}

			if(filter.RestrictUndeliveryAuthor != null)
			{
				title.Append($", автор недовоза: {filter.RestrictUndeliveryAuthor.GetPersonNameWithInitials()}");
			}

			if(filter.RestrictInProcessAtDepartment != null)
			{
				title.Append($", в работе у: {filter.RestrictInProcessAtDepartment.ShortName}");
			}

			if(filter.RestrictIsProblematicCases)
			{
				title.Append(", только проблемные случаи: да");
			}

			return title.ToString();
		}
	}
}

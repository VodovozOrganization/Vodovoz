using Gamma.Utilities;
using Gdk;
using Gtk;
using QS.Dialog.GtkUI;
using QS.Report;
using QSReport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;

namespace Vodovoz.ReportsParameters.Orders
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UndeliveredOrdersClassificationReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly UndeliveredOrdersFilterViewModel _filterViewModel;
		private readonly bool _withTransfer;

		public UndeliveredOrdersClassificationReport(UndeliveredOrdersFilterViewModel filterViewModel, bool withTransfer)
		{
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));
			_withTransfer = withTransfer;
			this.Build();
		}

		public string Title => "Сводка по классификации недовозов с переносами";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		private void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		private ReportInfo GetReportInfo()
		{
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

			return new ReportInfo
			{
				Identifier = _withTransfer ? "Orders.UndeliveredOrdersClassificationWithTransferReport" : "Orders.UndeliveredOrdersClassificationReport",

				Parameters = new Dictionary<string, object>
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
				}
			};
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

		public void Destroy()
		{

		}

		protected override void OnActivate()
		{
			base.OnActivate();
			OnUpdate(true);
		}

		protected override void OnAdded(Widget widget)
		{
			base.OnAdded(widget);
			OnUpdate(true);
		}

		protected override bool OnConfigureEvent(EventConfigure evnt)
		{
			return base.OnConfigureEvent(evnt);
		}

		protected override void OnShown()
		{
			base.OnShown();
			OnUpdate(true);
		}

		protected override bool OnVisibilityNotifyEvent(EventVisibility evnt)
		{
			return base.OnVisibilityNotifyEvent(evnt);
		}
	}
}

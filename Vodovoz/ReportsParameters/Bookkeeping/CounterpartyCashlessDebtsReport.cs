using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using Gtk;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Utilities.Enums;
using QSReport;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Services;
using Vodovoz.TempAdapters;

namespace Vodovoz.ReportsParameters.Bookkeeping
{
	public partial class CounterpartyCashlessDebtsReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly IDeliveryScheduleParametersProvider _deliveryScheduleParametersProvider;
		private readonly IInteractiveService _interactiveService;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;

		public CounterpartyCashlessDebtsReport(
			IDeliveryScheduleParametersProvider deliveryScheduleParametersProvider,
			IInteractiveService interactiveService,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IUnitOfWorkFactory unitOfWorkFactory)
		{
			_deliveryScheduleParametersProvider = deliveryScheduleParametersProvider
				?? throw new ArgumentNullException(nameof(deliveryScheduleParametersProvider));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			UoW = unitOfWorkFactory?.CreateWithoutRoot() ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			Build();
			Configure();
		}

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Долги по безналу";

		private void Configure()
		{
			ycheckExcludeClosingDocuments.Active = true;

			entryCounterparty.SetEntityAutocompleteSelectorFactory(
				_counterpartyJournalFactory.CreateCounterpartyAutocompleteSelectorFactory());
			entryCounterparty.Changed += (sender, args) => UpdateAvailableReports();

			enumcheckOrderStatuses.ExpandCheckButtons = false;
			enumcheckOrderStatuses.FillCheckButtons = false;
			enumcheckOrderStatuses.EnumType = typeof(OrderStatus);
			enumcheckOrderStatuses.SelectedValuesList = new List<Enum>
			{
				OrderStatus.Accepted, OrderStatus.InTravelList, OrderStatus.OnLoading, OrderStatus.OnTheWay, OrderStatus.UnloadingOnStock,
				OrderStatus.Shipped, OrderStatus.Closed
			};

			ybuttonSelectAllOrderStatuses.Clicked += (sender, args) => enumcheckOrderStatuses.SelectAll();
			ybuttonDeselectAllOrderStatuses.Clicked += (sender, args) => enumcheckOrderStatuses.UnselectAll();

			ybuttonCounterpartyDebtBalance.Clicked += (sender, args) =>
				LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo((Button)sender)));
			ybuttonNotPaidOrders.Clicked += (sender, args) =>
				LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo((Button)sender)));
			ybuttonCounterpartyDebtDetails.Clicked += (sender, args) =>
				LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo((Button)sender)));

			ybuttonInfo.Clicked += (sender, args) => ShowInfoMessage();

			UpdateAvailableReports();
		}

		private void UpdateAvailableReports()
		{
			if(entryCounterparty.Subject != null)
			{
				ybuttonCounterpartyDebtBalance.Sensitive = false;
				ybuttonCounterpartyDebtDetails.Sensitive = true;
			}
			else
			{
				ybuttonCounterpartyDebtBalance.Sensitive = true;
				ybuttonCounterpartyDebtDetails.Sensitive = false;
			}
		}

		private ReportInfo GetReportInfo(Button button)
		{
			var reportInfo = new ReportInfo
			{
				Title = button.Label,
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", periodPicker.StartDateOrNull },
					{ "end_date", periodPicker.EndDateOrNull },
					{ "counterparty_id", entryCounterparty.Subject?.GetIdOrNull() ?? 0 },
					{ "filters_text", GetFiltersText(button.Name) },
					{ "creation_date", DateTime.Now }
				}
			};

			switch(button.Name)
			{
				case nameof(ybuttonCounterpartyDebtBalance):
					reportInfo.Identifier = "Bookkeeping.CounterpartyDebtBalance";
					break;
				case nameof(ybuttonNotPaidOrders):
					reportInfo.Identifier = "Bookkeeping.NotPaidOrders";
					break;
				case nameof(ybuttonCounterpartyDebtDetails):
					reportInfo.Identifier = "Bookkeeping.CounterpartyDebtDetails";
					break;
				default:
					throw new NotSupportedException($"'{button.Name}' button name is not supported");
			}

			if(button.Name == nameof(ybuttonCounterpartyDebtBalance) || button.Name == nameof(ybuttonNotPaidOrders))
			{
				var orderStatuses = enumcheckOrderStatuses.SelectedValuesList.Any() ? enumcheckOrderStatuses.SelectedValuesList : (object)0;
				reportInfo.Parameters.Add("order_statuses", orderStatuses);
				reportInfo.Parameters.Add("exclude_closing_documents", ycheckExcludeClosingDocuments.Active);
				reportInfo.Parameters.Add("closing_document_delivery_schedule_id",
					_deliveryScheduleParametersProvider.ClosingDocumentDeliveryScheduleId);
			}

			return reportInfo;
		}

		private object GetFiltersText(string buttonName)
		{
			var resultString = "Выбранные фильтры:\n";

			DateTime? startDate = periodPicker.StartDateOrNull;
			DateTime? endDate = periodPicker.EndDateOrNull;

			if(startDate == null && endDate == null)
			{
				resultString += "";
			}
			else if(startDate != null && endDate == null)
			{
				resultString += $"Период даты доставки: Начиная с {startDate.Value:d}\n";
			}
			else if(startDate == null)
			{
				resultString += $"Период даты доставки: До {endDate.Value:d}\n";
			}
			else if(startDate == endDate)
			{
				resultString += $"Период даты доставки: На {startDate.Value:d}\n";
			}
			else
			{
				resultString += $"Период даты доставки: С {startDate.Value:d} по {endDate.Value:d}\n";
			}

			resultString += entryCounterparty.Subject == null
				? ""
				: $"Контрагент: {((Counterparty)entryCounterparty.Subject).Name}\n";

			if(buttonName == nameof(ybuttonCounterpartyDebtDetails))
			{
				return resultString;
			}

			resultString += ycheckExcludeClosingDocuments.Active
				? "Без закрывающих документов\n"
				: "";

			var selectedOrderStatuses = enumcheckOrderStatuses.SelectedValuesList.Cast<OrderStatus>().ToList();
			if(!selectedOrderStatuses.Any())
			{
				resultString += "Статусы заказов: Никакие";
			}
			else if(!EnumHelper.GetValuesList<OrderStatus>().Except(selectedOrderStatuses).Any())
			{
				resultString += "Статусы заказов: Все";
			}
			else
			{
				resultString += $"Статусы заказов: {string.Join(", ", selectedOrderStatuses.Select(x => x.GetEnumTitle()))}";
			}

			return resultString;
		}

		private void ShowInfoMessage()
		{
			_interactiveService.ShowMessage(
				ImportanceLevel.Info,
				"Во все отчёты попадают только:\n" +
				$"Контрагенты с формой '{PersonType.legal.GetEnumTitle()}'\n" +
				$"Заказы с формой оплаты '{PaymentType.cashless.GetEnumTitle()}', суммой больше 0 и статусом оплаты не равным '{OrderPaymentStatus.Paid.GetEnumTitle()}'\n\n" +
				$"<b>{ybuttonCounterpartyDebtBalance.Label}</b>:\n" +
				"Доступен только если не выбран контрагент \n\n" +
				$"<b>{ybuttonCounterpartyDebtDetails.Label}</b>:\n" +
				"Доступен только если выбран контрагент\n" +
				"Применяются только фильтры по дате и контрагенту"
			);
		}
	}
}

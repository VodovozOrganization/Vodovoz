using Gamma.Utilities;
using NHibernate.Linq;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Report.ViewModels;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Services;
using DateTimeHelpers;

namespace Vodovoz.ViewModels.ReportsParameters.Bookkeeping
{
	public class CounterpartyCashlessDebtsReportViewModel : ReportParametersViewModelBase, IDisposable
	{
		private const string _includeString = "_include";
		private const string _excludeString = "_exclude";

		private readonly ICommonServices _commonServices;
		private readonly IDeliveryScheduleParametersProvider _deliveryScheduleParametersProvider;
		private readonly IGenericRepository<CounterpartySubtype> _counterpartySubtypeRepository;
		private readonly IGenericRepository<Counterparty> _counterpartyRepository;
		private readonly IUnitOfWork _unitOfWork;

		private Dictionary<string, object> _parameters = new Dictionary<string, object>();
		private DateTime? _startDate;
		private DateTime? _endDate;
		private bool _isOrderByDate;

		public CounterpartyCashlessDebtsReportViewModel(
			ICommonServices commonServices,
			IDeliveryScheduleParametersProvider deliveryScheduleParametersProvider,
			IGenericRepository<CounterpartySubtype> counterpartySubtypeRepository,
			IGenericRepository<Counterparty> counterpartyRepository,
			RdlViewerViewModel rdlViewerViewModel) : base(rdlViewerViewModel)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_deliveryScheduleParametersProvider = deliveryScheduleParametersProvider ?? throw new ArgumentNullException(nameof(deliveryScheduleParametersProvider));
			_counterpartySubtypeRepository = counterpartySubtypeRepository ?? throw new ArgumentNullException(nameof(counterpartySubtypeRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			
			Title = "Долги по безналу";

			_unitOfWork = UnitOfWorkFactory.CreateWithoutRoot(Title);

			FilterViewModel = CreateCounterpartyCashlessDebtsReportIncludeExcludeFilter(_unitOfWork);

			ShowInfoMessageCommand = new DelegateCommand(ShowInfoMessage);
			GenerateCompanyDebtBalanceReportCommand = new DelegateCommand(GenerateCompanyDebtBalanceReport);
			GenerateNotPaidOrdersReportCommand = new DelegateCommand(GenerateNotPaidOrdersReport);
			GenerateCounterpartyDebtDetailsReportCommand = new DelegateCommand(GenerateCounterpartyDebtDetailsReport);
		}

		#region Properties
		public DelegateCommand ShowInfoMessageCommand { get; }
		public DelegateCommand GenerateCompanyDebtBalanceReportCommand { get; }
		public DelegateCommand GenerateNotPaidOrdersReportCommand { get; }
		public DelegateCommand GenerateCounterpartyDebtDetailsReportCommand { get; }
		protected override Dictionary<string, object> Parameters => _parameters;
		public IncludeExludeFiltersViewModel FilterViewModel { get; }

		public DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public bool IsOrderByDate
		{
			get => _isOrderByDate;
			set => SetField(ref _isOrderByDate, value);
		}
		#endregion Properties

		private void GenerateCompanyDebtBalanceReport()
		{
			if(StartDate.HasValue)
			{
				_parameters.Add("start_date", StartDate.Value.ToString("yyyy-MM-ddTHH:mm:ss"));
			}

			if(EndDate.HasValue)
			{
				_parameters.Add("end_date", EndDate.Value.LatestDayTime().ToString("yyyy-MM-ddTHH:mm:ss"));
			}

			_parameters = FilterViewModel.GetReportParametersSet();
			_parameters.Add( "creation_date", DateTime.Now);

			Identifier = "Bookkeeping.CounterpartyDebtBalance";

			LoadReport();
		}
		private void GenerateNotPaidOrdersReport()
		{

		}

		private void GenerateCounterpartyDebtDetailsReport()
		{

		}

		private bool IsReportFiltersSettingsValid()
		{
			return false;
		}


		private object GetFiltersText(string buttonName)
		{
			var resultString = "Выбранные фильтры:\n";

			if(StartDate == null && EndDate == null)
			{
				resultString += "";
			}
			else if(StartDate != null && EndDate == null)
			{
				resultString += $"Период даты доставки: Начиная с {StartDate.Value:d}\n";
			}
			else if(StartDate == null)
			{
				resultString += $"Период даты доставки: До {EndDate.Value:d}\n";
			}
			else if(StartDate == EndDate)
			{
				resultString += $"Период даты доставки: На {StartDate.Value:d}\n";
			}
			else
			{
				resultString += $"Период даты доставки: С {StartDate.Value:d} по {EndDate.Value:d}\n";
			}

			//resultString += entryCounterparty.Subject == null
			//	? ""
			//	: $"Контрагент: {((Counterparty)entryCounterparty.Subject).Name}\n";

			//if(buttonName == nameof(ybuttonCounterpartyDebtDetails))
			//{
			//	return resultString;
			//}

			//resultString += IsExcludeClosingDocuments
			//	? "Без закрывающих документов\n"
			//	: "";

			//resultString += ycheckExcludeChainStores.Active
			//	? "Без сетей\n"
			//	: "";

			//resultString += IsExpiredOnly
			//	? "Только просроченные\n"
			//	: "";

			//var selectedOrderStatuses = enumcheckOrderStatuses.SelectedValuesList.Cast<OrderStatus>().ToList();
			//if(!selectedOrderStatuses.Any())
			//{
			//	resultString += "Статусы заказов: Никакие";
			//}
			//else if(!EnumHelper.GetValuesList<OrderStatus>().Except(selectedOrderStatuses).Any())
			//{
			//	resultString += "Статусы заказов: Все";
			//}
			//else
			//{
			//	resultString += $"Статусы заказов: {string.Join(", ", selectedOrderStatuses.Select(x => x.GetEnumTitle()))}";
			//}

			return resultString;
		}

		private void ShowInfoMessage()
		{
			//_commonServices.InteractiveService.ShowMessage(
			//	ImportanceLevel.Info,
			//	"Во все отчёты попадают только:\n" +
			//	$"Контрагенты с формой '{PersonType.legal.GetEnumTitle()}'\n" +
			//	$"Заказы с формой оплаты '{PaymentType.Cashless.GetEnumTitle()}', суммой больше 0 и статусом оплаты не равным '{OrderPaymentStatus.Paid.GetEnumTitle()}'\n\n" +
			//	$"<b>{ybuttonCounterpartyDebtBalance.Label}</b>:\n" +
			//	"Доступен только если не выбран контрагент \n\n" +
			//	$"<b>{ybuttonCounterpartyDebtDetails.Label}</b>:\n" +
			//	"Доступен только если выбран контрагент\n\n" +
			//	$"Если <b>{chkOrderByDate.Label}</b> не активна:\n" +
			//	$"- <b>{ybuttonCounterpartyDebtBalance.Label}</b> сортируется по последнему столбцу\n" +
			//	$"- <b>{ybuttonNotPaidOrders.Label}</b> сортируются по последнему столбцу\n" +
			//	$"- <b>{ybuttonCounterpartyDebtDetails.Label}</b> сортировка была по дате платежа по убыванию,\n" +
			//	"а внутри блока платежа закрытые суммы заказов данным платежом.\n" +
			//	"Выше до всех платежей идут незакрытые суммы неоплаченных/частично оплаченных заказов.\n" +
			//	"Заказы внутри блока также сортируются по дате доставки по убыванию\n\n" +
			//	$"Если <b>{chkOrderByDate.Label}</b> активна:\n" +
			//	$"- <b>{ybuttonCounterpartyDebtBalance.Label}</b> сортируется по последнему столбцу\n" +
			//	$"- <b>{ybuttonNotPaidOrders.Label}</b> сортируются по столбцу \"Дата доставки\"\n" +
			//	$"- <b>{ybuttonCounterpartyDebtDetails.Label}</b> сортируется по столбцу \"Дата доставки заказа. Время операции\" по убыванию.\n" +
			//	"При этом заказы не привязаны к платежу и не разбиваются по закрытым/незакрытым суммам"
			//);
		}

		public IncludeExludeFiltersViewModel CreateCounterpartyCashlessDebtsReportIncludeExcludeFilter(IUnitOfWork unitOfWork)
		{
			var includeExludeFiltersViewModel = new IncludeExludeFiltersViewModel(_commonServices.InteractiveService);

			includeExludeFiltersViewModel.AddFilter<CounterpartyType>(filterConfig =>
			{
				filterConfig.RefreshFunc = (filter) =>
				{
					var values = Enum.GetValues(typeof(CounterpartyType));

					filter.FilteredElements.Clear();

					var counterpartySubtypeValues =
						_counterpartySubtypeRepository.Get(unitOfWork, counterpartySubtype => (string.IsNullOrWhiteSpace(includeExludeFiltersViewModel.CurrentSearchString))
							|| counterpartySubtype.Name.ToLower().Like($"%{includeExludeFiltersViewModel.CurrentSearchString.ToLower()}%"));

					// Заполнение начального списка

					foreach(var value in values)
					{
						if(value is CounterpartyType enumElement
							&& !filter.HideElements.Contains(enumElement)
							&& (string.IsNullOrWhiteSpace(includeExludeFiltersViewModel.CurrentSearchString)
								|| enumElement.GetEnumTitle().ToLower().Contains(includeExludeFiltersViewModel.CurrentSearchString.ToLower())
								|| (enumElement == CounterpartyType.AdvertisingDepartmentClient && counterpartySubtypeValues.Any())))
						{
							filter.FilteredElements.Add(new IncludeExcludeElement<CounterpartyType, CounterpartyType>()
							{
								Id = enumElement,
								Title = enumElement.GetEnumTitle(),
							});
						}
					}

					// Заполнение подтипов контрагента - клиентов рекламного отдела

					var advertisingDepartmentClientNode = filter.FilteredElements
						.FirstOrDefault(x => x.Number == nameof(CounterpartyType.AdvertisingDepartmentClient));

					if(counterpartySubtypeValues.Any())
					{
						var advertisingDepartmentClientValues = counterpartySubtypeValues
							.Select(x => new IncludeExcludeElement<int, CounterpartySubtype>
							{
								Id = x.Id,
								Parent = advertisingDepartmentClientNode,
								Title = x.Name,
							});

						foreach(var element in advertisingDepartmentClientValues)
						{
							advertisingDepartmentClientNode.Children.Add(element);
						}
					}
				};

				filterConfig.GetReportParametersFunc = (filter) =>
				{
					var result = new Dictionary<string, object>();

					// Тип контрагента

					var includeCounterpartyTypeValues = filter.IncludedElements
						.Where(x => x.GetType() == typeof(IncludeExcludeElement<CounterpartyType, CounterpartyType>))
						.Select(x => x.Number)
						.ToArray();

					if(includeCounterpartyTypeValues.Length > 0)
					{
						result.Add(typeof(CounterpartyType).Name + _includeString, includeCounterpartyTypeValues);
					}
					else
					{
						result.Add(typeof(CounterpartyType).Name + _includeString, new object[] { "0" });
					}

					var excludeCounterpartyTypeValues = filter.ExcludedElements
						.Where(x => x.GetType() == typeof(IncludeExcludeElement<CounterpartyType, CounterpartyType>))
						.Select(x => x.Number)
						.ToArray();

					if(excludeCounterpartyTypeValues.Length > 0)
					{
						result.Add(typeof(CounterpartyType).Name + _excludeString, excludeCounterpartyTypeValues);
					}
					else
					{
						result.Add(typeof(CounterpartyType).Name + _excludeString, new object[] { "0" });
					}

					// Клиент Рекламного Отдела

					var includeCounterpartySubtypeValues = filter.IncludedElements
						.Where(x => x.GetType() == typeof(IncludeExcludeElement<int, CounterpartySubtype>))
						.Select(x => x.Number)
						.ToArray();

					if(includeCounterpartySubtypeValues.Length > 0)
					{
						result.Add(typeof(CounterpartySubtype).Name + _includeString, includeCounterpartySubtypeValues);
					}
					else
					{
						result.Add(typeof(CounterpartySubtype).Name + _includeString, new object[] { "0" });
					}

					var excludeCounterpartySubtypeValues = filter.ExcludedElements
						.Where(x => x.GetType() == typeof(IncludeExcludeElement<int, CounterpartySubtype>))
						.Select(x => x.Number)
						.ToArray();

					if(excludeCounterpartySubtypeValues.Length > 0)
					{
						result.Add(typeof(CounterpartySubtype).Name + _excludeString, excludeCounterpartySubtypeValues);
					}
					else
					{
						result.Add(typeof(CounterpartySubtype).Name + _excludeString, new object[] { "0" });
					}

					return result;
				};
			});

			includeExludeFiltersViewModel.AddFilter(unitOfWork, _counterpartyRepository);

			var statusesToSelect = new[]
			{
				OrderStatus.Accepted,
				OrderStatus.InTravelList,
				OrderStatus.OnLoading,
				OrderStatus.OnTheWay,
				OrderStatus.Shipped,
				OrderStatus.UnloadingOnStock,
				OrderStatus.Closed
			};

			includeExludeFiltersViewModel.AddFilter<OrderStatus>(config =>
			{
				config.RefreshFilteredElements();

				foreach(var element in config.FilteredElements)
				{
					if(element is IncludeExcludeElement<OrderStatus, OrderStatus> enumElement &&
						statusesToSelect.Contains(enumElement.Id))
					{
						enumElement.Include = true;
					}
				}
			});

			includeExludeFiltersViewModel.AddFilter<DebtType>(config =>
			{
				config.RefreshFilteredElements();
			});

			var additionalParams = new Dictionary<string, string>
			{
				{ "Закрывающие документы", "is_closing_documents" },
				{ "Сети", "is_chain_stores" },
				{ "Просроченные", "is_expired" },
				{ "Ликвидирован", "is_liquidated" },
			};

			includeExludeFiltersViewModel.AddFilter("Дополнительные фильтры", additionalParams);

			return includeExludeFiltersViewModel;
		}

		public void Dispose()
		{
			_unitOfWork?.Dispose();
		}
	}
}

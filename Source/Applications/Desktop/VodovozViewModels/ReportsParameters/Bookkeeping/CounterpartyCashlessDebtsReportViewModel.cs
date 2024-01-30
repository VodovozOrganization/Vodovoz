using DateTimeHelpers;
using Gamma.Utilities;
using NHibernate.Linq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Report.ViewModels;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.ReportsParameters.Bookkeeping
{
	public class CounterpartyCashlessDebtsReportViewModel : ReportParametersViewModelBase, IDisposable
	{
		private const string _includeString = "_include";
		private const string _excludeString = "_exclude";

		private readonly ICommonServices _commonServices;
		private readonly IGenericRepository<CounterpartySubtype> _counterpartySubtypeRepository;
		private readonly IGenericRepository<Counterparty> _counterpartyRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly int _closingDocumentDeliveryScheduleId;

		private Dictionary<string, object> _parameters = new Dictionary<string, object>();
		private DateTime? _startDate;
		private DateTime? _endDate;
		private bool _isOrderByDate;
		private bool _isCanCreateCounterpartyDebtDetailsReport;

		public CounterpartyCashlessDebtsReportViewModel(
			ICommonServices commonServices,
			IGenericRepository<CounterpartySubtype> counterpartySubtypeRepository,
			IGenericRepository<Counterparty> counterpartyRepository,
			IDeliveryScheduleParametersProvider deliveryScheduleParametersProvider,
			RdlViewerViewModel rdlViewerViewModel) : base(rdlViewerViewModel)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_counterpartySubtypeRepository = counterpartySubtypeRepository ?? throw new ArgumentNullException(nameof(counterpartySubtypeRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));

			if(deliveryScheduleParametersProvider is null)
			{
				throw new ArgumentNullException(nameof(deliveryScheduleParametersProvider));
			}

			Title = "Долги по безналу";

			_unitOfWork = UnitOfWorkFactory.CreateWithoutRoot(Title);

			_closingDocumentDeliveryScheduleId = deliveryScheduleParametersProvider.ClosingDocumentDeliveryScheduleId;

			FilterViewModel = CreateCounterpartyCashlessDebtsReportIncludeExcludeFilter(_unitOfWork);
			FilterViewModel.SelectionChanged += OnFilterViewModelSelectionChanged;

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
		public IncludeExludeFiltersViewModel FilterViewModel { get; }

		protected override Dictionary<string, object> Parameters => _parameters;

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

		public bool IsCanCreateCounterpartyDebtDetailsReport
		{
			get => _isCanCreateCounterpartyDebtDetailsReport;
			set => SetField(ref _isCanCreateCounterpartyDebtDetailsReport, value);
		}
		#endregion Properties

		private void GenerateCompanyDebtBalanceReport()
		{
			Identifier = "Bookkeeping.CounterpartyDebtBalance";

			GenerateReport();
		}

		private void GenerateNotPaidOrdersReport()
		{
			Identifier = "Bookkeeping.NotPaidOrders";

			GenerateReport();
		}

		private void GenerateCounterpartyDebtDetailsReport()
		{
			if(!IsSelectedOneCounterpartyCheck())
			{
				_commonServices.InteractiveService.ShowMessage(
					QS.Dialog.ImportanceLevel.Error,
					"Чтобы сформировать отчет необходимо выбрать только одного контрагента");

				return;
			}

			Identifier = IsOrderByDate
				? "Bookkeeping.CounterpartyDebtDetails"
				: "Bookkeeping.CounterpartyDebtDetailsWithoutOrderByDate";

			GenerateReport(true);
		}

		private void GenerateReport(bool isReportBySingleCounterpartyDebt = false)
		{
			_parameters = FilterViewModel.GetReportParametersSet();

			_parameters.Add("start_date", StartDate.HasValue ? StartDate.Value.ToString("yyyy-MM-ddTHH:mm:ss") : string.Empty);
			_parameters.Add("end_date", EndDate.HasValue ? EndDate.Value.LatestDayTime().ToString("yyyy-MM-ddTHH:mm:ss") : string.Empty);
			_parameters.Add("closing_document_delivery_schedule_id", _closingDocumentDeliveryScheduleId);

			if(isReportBySingleCounterpartyDebt)
			{
				_parameters.Add("counterparty_id", GetSelectedCounterpartyId());
			}
			else
			{
				_parameters.Add("order_by_date", IsOrderByDate);
			}

			_parameters.Add("filters_text", GetFiltersText(_parameters, isReportBySingleCounterpartyDebt));

			LoadReport();
		}

		private void OnFilterViewModelSelectionChanged(object sender, EventArgs e)
		{
			IsCanCreateCounterpartyDebtDetailsReport = IsSelectedOneCounterpartyCheck();
		}

		private bool IsSelectedOneCounterpartyCheck()
		{
			var parameters = FilterViewModel.GetReportParametersSet();

			if(parameters.TryGetValue("Counterparty_include", out object value))
			{
				if(value is string[] includedCounterparties)
				{
					return includedCounterparties.Length == 1;
				}
			}

			return false;
		}

		private int GetSelectedCounterpartyId()
		{
			var parameters = FilterViewModel.GetReportParametersSet();

			if(parameters.TryGetValue("Counterparty_include", out object value))
			{
				if(value is string[] includedCounterparties)
				{
					if(includedCounterparties.Length == 1)
					{
						return int.Parse(includedCounterparties[0]);
					}
				}
			}

			throw new InvalidOperationException("Для формирования отчета необходимо, чтобы был выбран только один контрагент");
		}

		private string GetFiltersText(Dictionary<string, object> parameters, bool isReportBySingleCounterpartyDebt)
		{
			if(parameters.Count == 0)
			{
				return string.Empty;
			}

			var filtersText = new StringBuilder();

			filtersText.AppendLine("Выбранные фильтры:");
			filtersText.Append("Период даты доставки: ");

			if(StartDate == null && EndDate == null)
			{
				filtersText.AppendLine("не выбран");
			}
			else if(StartDate != null && EndDate == null)
			{
				filtersText.AppendLine($"Начиная с {StartDate.Value:d}");
			}
			else if(StartDate == null)
			{
				filtersText.AppendLine($"До {EndDate.Value:d}");
			}
			else if(StartDate == EndDate)
			{
				filtersText.AppendLine($"На {StartDate.Value:d}");
			}
			else
			{
				filtersText.AppendLine($"С {StartDate.Value:d} по {EndDate.Value:d}");
			}

			foreach (var parameter in parameters)
			{
				switch(parameter.Key)
				{
					case "is_closing_documents":
						filtersText.AppendLine((bool)parameter.Value ? "Только закрывающие документы" : "Исключить Закрывающие документы");
						break;
					case "is_chain_stores":
						filtersText.AppendLine((bool)parameter.Value && !isReportBySingleCounterpartyDebt ? "Только Сети" : "Исключить Сети");
						break;
					case "is_expired":
						filtersText.AppendLine((bool)parameter.Value && !isReportBySingleCounterpartyDebt ? "Только Просроченные" : "Исключить Просроченные");
						break;
					case "is_liquidated":
						filtersText.AppendLine((bool)parameter.Value && !isReportBySingleCounterpartyDebt ? "Только Ликвидирован" : "Исключить Ликвидирован");
						break;
					case "Counterparty_include":
						if(parameter.Value is string[] includedCounterparties && !isReportBySingleCounterpartyDebt)
						{
							filtersText.AppendLine($"Вкл.клиентов: {includedCounterparties.Length}");
						}
						break;
					case "Counterparty_exclude":
						if(parameter.Value is string[] excludedCounterparties && !isReportBySingleCounterpartyDebt)
						{
							filtersText.AppendLine($"Искл.клиентов: {excludedCounterparties.Length}");
						}
						break;
					case "CounterpartyType_include":
						if(parameter.Value is string[] includedCounterpartyTypes && !isReportBySingleCounterpartyDebt)
						{
							filtersText.AppendLine($"Вкл.типов клиентов: {includedCounterpartyTypes.Length}");
						}
						break;
					case "CounterpartyType_exclude":
						if(parameter.Value is string[] excludedCounterpartyTypes && !isReportBySingleCounterpartyDebt)
						{
							filtersText.AppendLine($"Искл.типов клиентов: {excludedCounterpartyTypes.Length}");
						}
						break;
					case "CounterpartySubtype_include":
						if(parameter.Value is string[] includedCounterpartySubtype && !isReportBySingleCounterpartyDebt)
						{
							filtersText.AppendLine($"Вкл.типов клиентов: {includedCounterpartySubtype.Length}");
						}
						break;
					case "CounterpartySubtype_exclude":
						if(parameter.Value is string[] excludedCounterpartySubtype && !isReportBySingleCounterpartyDebt)
						{
							filtersText.AppendLine($"Искл.типов клиентов: {excludedCounterpartySubtype.Length}");
						}
						break;
					case "OrderStatus_include":
						if(parameter.Value is string[] includedOrderStatus)
						{
							filtersText.AppendLine($"Вкл.статусов заказов: {includedOrderStatus.Length}");
						}
						break;
					case "OrderStatus_exclude":
						if(parameter.Value is string[] excludedOrderStatus)
						{
							filtersText.AppendLine($"Искл.статусов заказов: {excludedOrderStatus.Length}");
						}
						break;
					case "DebtType_include":
						if(parameter.Value is string[] includedDebtType && !isReportBySingleCounterpartyDebt)
						{
							filtersText.AppendLine($"Вкл.типов задолженности: {includedDebtType.Length}");
						}
						break;
					case "DebtType_exclude":
						if(parameter.Value is string[] excludedDebtType && !isReportBySingleCounterpartyDebt)
						{
							filtersText.AppendLine($"Искл.типов задолженности: {excludedDebtType.Length}");
						}
						break;
				}
			}

			return filtersText.ToString();
		}
		
		private void ShowInfoMessage()
		{
			_commonServices.InteractiveService.ShowMessage(
				ImportanceLevel.Info,
				"Во все отчёты попадают только:\n" +
				$"Контрагенты с формой '{PersonType.legal.GetEnumTitle()}'\n" +
				$"Заказы с формой оплаты '{PaymentType.Cashless.GetEnumTitle()}', суммой больше 0 и статусом оплаты не равным '{OrderPaymentStatus.Paid.GetEnumTitle()}'\n\n" +
				$"<b>Отчет \"Детализация по клиенту\"</b>:\n" +
				"Доступен только если выбран один контрагент\n\n" +
				$"Если <b>\"Сортировка по дате\"</b> не активна:\n" +
				$"- <b>отчет \"Баланс компании\"</b> сортируется по последнему столбцу\n" +
				$"- <b>отчет \"Неоплаченные заказы\"</b> сортируются по последнему столбцу\n" +
				$"- <b>в отчете \"Детализация по клиенту\"</b> сортировка была по дате платежа по убыванию,\n" +
				"а внутри блока платежа закрытые суммы заказов данным платежом.\n" +
				"Выше до всех платежей идут незакрытые суммы неоплаченных/частично оплаченных заказов.\n" +
				"Заказы внутри блока также сортируются по дате доставки по убыванию\n\n" +
				$"Если <b>\"Сортировка по дате\"</b> активна:\n" +
				$"- <b>\"Баланс компании\"</b> сортируется по последнему столбцу\n" +
				$"- <b>\"Неоплаченные заказы\"</b> сортируются по столбцу \"Дата доставки\"\n" +
				$"- <b>\"Детализация по клиенту\"</b> сортируется по столбцу \"Дата доставки заказа. Время операции\" по убыванию.\n" +
				"При этом заказы не привязаны к платежу и не разбиваются по закрытым/незакрытым суммам\n" +
				$"При формировании отчета <b>\"Детализация по клиенту\"</b> учитываются только параметры фильтра: \"Период\",\n" +
				$"\"Сортировка по дате\", \"Контрагенты\" (должен быть выбран только один контрагента), \"Статусы заказа\",\n" +
				$"\"Закрывающие документы\". Остальные фильтры на результат отчета не влияют."
			);
		}

		private IncludeExludeFiltersViewModel CreateCounterpartyCashlessDebtsReportIncludeExcludeFilter(IUnitOfWork unitOfWork)
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

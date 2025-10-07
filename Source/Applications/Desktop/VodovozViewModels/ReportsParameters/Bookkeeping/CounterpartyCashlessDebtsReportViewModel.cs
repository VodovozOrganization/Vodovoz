using DateTimeHelpers;
using Gamma.Utilities;
using NHibernate.Linq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.Extensions;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters;
using Vodovoz.Settings.Delivery;
using Vodovoz.Tools;

namespace Vodovoz.ViewModels.ReportsParameters.Bookkeeping
{
	public partial class CounterpartyCashlessDebtsReportViewModel : ReportParametersViewModelBase, IDisposable
	{
		private const string _includeString = "_include";
		private const string _excludeString = "_exclude";

		private readonly ICommonServices _commonServices;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IGenericRepository<CounterpartySubtype> _counterpartySubtypeRepository;
		private readonly IGenericRepository<Counterparty> _counterpartyRepository;
		private readonly IGenericRepository<Organization> _organizationRepository;
		private readonly IGenericRepository<Employee> _employeeRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly int _closingDocumentDeliveryScheduleId;

		private Dictionary<string, object> _parameters = new Dictionary<string, object>();
		private DateTime? _startDate;
		private DateTime? _endDate;
		private bool _isOrderByDate;
		private bool _isCanCreateCounterpartyDebtDetailsReport;
		private bool _showPhones;

		public CounterpartyCashlessDebtsReportViewModel(
			ICommonServices commonServices,
			IUnitOfWorkFactory uowFactory,
			IGenericRepository<CounterpartySubtype> counterpartySubtypeRepository,
			IGenericRepository<Counterparty> counterpartyRepository,
			IGenericRepository<Organization> organizationRepository,
			IDeliveryScheduleSettings deliveryScheduleSettings,
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IGenericRepository<Employee> employeeRepository,
			ICurrentPermissionService currentPermissionService
			) : base(rdlViewerViewModel, reportInfoFactory)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_counterpartySubtypeRepository = counterpartySubtypeRepository ?? throw new ArgumentNullException(nameof(counterpartySubtypeRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));

			if(deliveryScheduleSettings is null)
			{
				throw new ArgumentNullException(nameof(deliveryScheduleSettings));
			}

			Title = "Долги по безналу";

			_unitOfWork = _uowFactory.CreateWithoutRoot(Title);

			_closingDocumentDeliveryScheduleId = deliveryScheduleSettings.ClosingDocumentDeliveryScheduleId;

			FilterViewModel = CreateCounterpartyCashlessDebtsReportIncludeExcludeFilter(_unitOfWork);
			FilterViewModel.SelectionChanged += OnFilterViewModelSelectionChanged;

			ShowInfoMessageCommand = new DelegateCommand(ShowInfoMessage);
			GenerateCompanyDebtBalanceReportCommand = new DelegateCommand(GenerateCompanyDebtBalanceReport);
			GenerateNotPaidOrdersReportCommand = new DelegateCommand(GenerateNotPaidOrdersReport);
			GenerateCounterpartyDebtDetailsReportCommand = new DelegateCommand(GenerateCounterpartyDebtDetailsReport);

			CanShowPhones = currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.ReportPermissions.Sales.CanGetContactsInSalesReports);
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

		public bool ShowPhones
		{
			get => _showPhones;
			set => SetField(ref _showPhones, value);
		}

		public bool CanShowPhones { get; }

		public bool IsCanCreateCounterpartyDebtDetailsReport
		{
			get => _isCanCreateCounterpartyDebtDetailsReport;
			set => SetField(ref _isCanCreateCounterpartyDebtDetailsReport, value);
		}

		#endregion Properties

		private void GenerateCompanyDebtBalanceReport()
		{
			Identifier = "Bookkeeping.CounterpartyDebtBalance";

			GenerateReport(CounterpartyCashlessDebtsReportType.DebtBalance);
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
					ImportanceLevel.Error,
					"Чтобы сформировать отчет необходимо выбрать только одного контрагента");

				return;
			}

			Identifier = IsOrderByDate
				? "Bookkeeping.CounterpartyDebtDetails"
				: "Bookkeeping.CounterpartyDebtDetailsWithoutOrderByDate";

			GenerateReport(CounterpartyCashlessDebtsReportType.DebtDetails);
		}

		private void GenerateReport(CounterpartyCashlessDebtsReportType? reportType = null)
		{
			_parameters = FilterViewModel.GetReportParametersSet(out var sb);

			_parameters.Add("start_date", StartDate.HasValue ? StartDate.Value.ToString(DateTimeFormats.QueryDateTimeFormat) : string.Empty);
			_parameters.Add("end_date", EndDate.HasValue ? EndDate.Value.LatestDayTime().ToString(DateTimeFormats.QueryDateTimeFormat) : string.Empty);
			_parameters.Add("closing_document_delivery_schedule_id", _closingDocumentDeliveryScheduleId);

			if(reportType == CounterpartyCashlessDebtsReportType.DebtDetails)
			{
				_parameters.Add("counterparty_id", GetSelectedCounterpartyId());
				_parameters.Add("filters_text", GetFiltersText(_parameters, true));
			}
			else
			{
				_parameters.Add("order_by_date", IsOrderByDate);
				_parameters.Add("filters_text", GetFiltersText(_parameters, false));
			}

			if(reportType == CounterpartyCashlessDebtsReportType.DebtBalance)
			{
				_parameters.Add("show_phones", ShowPhones);
			}
			
			LoadReport();
		}

		private void OnFilterViewModelSelectionChanged(object sender, EventArgs e)
		{
			IsCanCreateCounterpartyDebtDetailsReport = IsSelectedOneCounterpartyCheck();
		}

		private bool IsSelectedOneCounterpartyCheck()
		{
			var parameters = FilterViewModel.GetReportParametersSet(out var sb);

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
			var parameters = FilterViewModel.GetReportParametersSet(out var sb);

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

			foreach(var parameter in parameters)
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
					case "is_tender":
							filtersText.AppendLine((bool)parameter.Value && !isReportBySingleCounterpartyDebt ? "Только Тендер" : "Исключить Тендер");
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
					case "Organization_include":
						if(parameter.Value is string[] includedOrganizations)
						{
							var includeOrganizations = FilterViewModel.GetIncludedElements<Organization>().Select(x => x.Title.Trim('\n'));							
							filtersText.AppendLine($"Вкл.организации: {string.Join(", ", includeOrganizations)}");
						}
						break;
					case "Organization_exclude":
						if(parameter.Value is string[] excludedOrganizations)
						{
							var includeOrganizations = FilterViewModel.GetExcludedElements<Organization>().Select(x => x.Title.Trim('\n'));
							filtersText.AppendLine($"Искл.организации:  {string.Join(", ", includeOrganizations)}");
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

				filterConfig.GetReportParametersFunc = CustomReportParametersFunc.CounterpartyTypeReportParametersFunc;
			});

			includeExludeFiltersViewModel.AddFilter(unitOfWork, _counterpartyRepository);
				
			AddSalesManagerFilter(unitOfWork, includeExludeFiltersViewModel);
			
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
				{ "Тендер", "is_tender" },
			};

			includeExludeFiltersViewModel.AddFilter("Дополнительные фильтры", additionalParams);

			includeExludeFiltersViewModel.AddFilter(unitOfWork, _organizationRepository);

			return includeExludeFiltersViewModel;
		}

		private void AddSalesManagerFilter(IUnitOfWork unitOfWork, IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			includeExludeFiltersViewModel.AddFilter(unitOfWork, _employeeRepository, config =>
			{
				config.Title = "Менеджеры КА";
				config.DefaultName = "SalesManager";
				config.RefreshFunc = filter =>
				{
					Expression<Func<Employee, bool>> specificationExpression = null;

					var splitedWords = includeExludeFiltersViewModel.CurrentSearchString.Split(' ');

					foreach(var word in splitedWords)
					{
						if(string.IsNullOrWhiteSpace(word))
						{
							continue;
						}

						Expression<Func<Employee, bool>> searchInFullNameSpec = employee =>
							employee.Name.ToLower().Like($"%{word.ToLower()}%")
							|| employee.LastName.ToLower().Like($"%{word.ToLower()}%")
							|| employee.Patronymic.ToLower().Like($"%{word.ToLower()}%");

						specificationExpression = specificationExpression.CombineWith(searchInFullNameSpec);
					}

					var elementsToAdd = _employeeRepository.Get(
							unitOfWork,
							specificationExpression,
							limit: IncludeExludeFiltersViewModel.DefaultLimit)
						.Select(x => new IncludeExcludeElement<int, Employee>
						{
							Id = x.Id,
							Title = $"{x.LastName} {x.Name} {x.Patronymic}",
						});

					filter.FilteredElements.Clear();

					foreach(var element in elementsToAdd)
					{
						filter.FilteredElements.Add(element);
					}
				};
			});
		}
		
		public void Dispose()
		{
			_unitOfWork?.Dispose();
		}
	}
}

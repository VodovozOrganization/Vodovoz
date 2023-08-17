using Gamma.Utilities;
using NHibernate.Linq;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Report;
using QSReport;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Extensions;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.ViewWidgets.Reports;

namespace Vodovoz.Reports
{
	public partial class SalesReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly IncludeExludeFiltersViewModel _filterViewModel;
		private readonly bool _userIsSalesRepresentative;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IInteractiveService _interactiveService;
		private readonly IGenericRepository<Nomenclature> _nomenclatureRepository;
		private readonly IGenericRepository<ProductGroup> _productGroupRepository;
		private readonly IGenericRepository<PaymentFrom> _paymentFromRepository;
		private readonly IGenericRepository<Counterparty> _counterpartyRepository;
		private readonly IGenericRepository<Organization> _organizationRepository;
		private readonly IGenericRepository<DiscountReason> _discountReasonRepository;
		private readonly IGenericRepository<Subdivision> _subdivisionRepository;
		private readonly IGenericRepository<Employee> _employeeGenericRepository;
		private readonly IGenericRepository<GeoGroup> _geographicalGroupRepository;
		private readonly IGenericRepository<PromotionalSet> _promotionalSetRepository;
		private readonly bool _canSeePhones;

		public event EventHandler<LoadReportEventArgs> LoadReport;
		public string Title => "Отчет по продажам";

		public IncludeExludeFiltersViewModel FilterViewModel => _filterViewModel;

		public SalesReport(
			IEmployeeRepository employeeRepository,
			IInteractiveService interactiveService,
			IncludeExludeFiltersViewModel includeExludeFiltersViewModel,
			IGenericRepository<Nomenclature> nomenclatureRepository,
			IGenericRepository<Counterparty> counterpartyRepository,
			IGenericRepository<Organization> organizationRepository,
			IGenericRepository<DiscountReason> discountReasonRepository,
			IGenericRepository<Subdivision> subdivisionRepository,
			IGenericRepository<Employee> employeeGenericRepository,
			IGenericRepository<GeoGroup> geographicalGroupRepository,
			IGenericRepository<PromotionalSet> promotionalSetRepository,
			IGenericRepository<ProductGroup> productGroupRepository,
			IGenericRepository<PaymentFrom> paymentFromRepository)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_productGroupRepository = productGroupRepository ?? throw new ArgumentNullException(nameof(productGroupRepository));
			_paymentFromRepository = paymentFromRepository ?? throw new ArgumentNullException(nameof(paymentFromRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_employeeGenericRepository = employeeGenericRepository ?? throw new ArgumentNullException(nameof(employeeGenericRepository));
			_geographicalGroupRepository = geographicalGroupRepository ?? throw new ArgumentNullException(nameof(geographicalGroupRepository));
			_promotionalSetRepository = promotionalSetRepository ?? throw new ArgumentNullException(nameof(promotionalSetRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			UoW.Session.DefaultReadOnly = true;
			_filterViewModel = includeExludeFiltersViewModel ?? throw new ArgumentNullException(nameof(includeExludeFiltersViewModel));

			_userIsSalesRepresentative =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(Permissions.User.IsSalesRepresentative)
				&& !ServicesConfig.CommonServices.UserService.GetCurrentUser().IsAdmin;

			_canSeePhones = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(Permissions.Report.SalesReport.CanGenerateDetailedReportWithPhones);

			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			dateperiodpicker.StartDate = dateperiodpicker.EndDate = DateTime.Today;
			buttonInfo.Clicked += (sender, args) => ShowInfoWindow();

			ycheckbuttonDetail.Toggled += (sender, args) =>
			{
				ycheckbuttonPhones.Sensitive = _canSeePhones && ycheckbuttonDetail.Active;
			};

			SetupFilter();
		}

		private void ShowInfoWindow()
		{
			var info =
				"<b>1.</b> Подсчет продаж ведется на основе заказов. В отчете учитываются заказы со статусами:" +
				$"\n\t'{OrderStatus.Accepted.GetEnumTitle()}'" +
				$"\n\t'{OrderStatus.InTravelList.GetEnumTitle()}'" +
				$"\n\t'{OrderStatus.OnLoading.GetEnumTitle()}'" +
				$"\n\t'{OrderStatus.OnTheWay.GetEnumTitle()}'" +
				$"\n\t'{OrderStatus.Shipped.GetEnumTitle()}'" +
				$"\n\t'{OrderStatus.UnloadingOnStock.GetEnumTitle()}'" +
				$"\n\t'{OrderStatus.Closed.GetEnumTitle()}'" +
				$"\n\t'{OrderStatus.WaitForPayment.GetEnumTitle()}' и заказ - самовывоз с оплатой после отгрузки." +
				"\nВ отчет <b>не попадают</b> заказы, являющиеся закрывашками по контракту." +
				"\nФильтр по дате отсекает заказы, если дата доставки не входит в выбранный период." +

				"\n\n<b>2.</b> Подсчет тары ведется следующим образом:" +
				"\n\tПлановое значение - сумма бутылей на возврат попавших в отчет заказов;" +
				"\n\tФактическое значение - сумма фактически возвращенных бутылей по адресам маршрутного листа." +
				"\n\t\tФактическое значение возвращенных бутылей по адресу зависит от того, доставлен<b>(*)</b> заказ или нет:" +
				"\n\t\t\t <b>-</b> Если да - берется кол-во бутылей, которое по факту забрал водитель. " +
				"Это кол-во может быть вручную указано при закрытии МЛ;" +

				"\n\t\t\t <b>-</b> Если не доставлен - берется кол-во бутылей на возврат из заказа;" +
				"\n\t\t\t <b>-</b> Если заказ является самовывозом - берется значение возвращенной тары, указанное в отпуске самовывоза;" +
				$"\n\t\t <b>*</b> Заказ считается доставленным, если его статус в МЛ: '{RouteListItemStatus.Completed.GetEnumTitle()}' или " +
				$"'{RouteListItemStatus.EnRoute.GetEnumTitle()}' и статус МЛ '{RouteListStatus.Closed.GetEnumTitle()}' " +
				$"или '{RouteListStatus.OnClosing.GetEnumTitle()}'." +
				"\n\nДетальный отчет аналогичен обычному, лишь предоставляет расширенную информацию.";

			_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Справка по работе с отчетом");
		}

		private ReportInfo GetReportInfo()
		{
			var parameters = FilterViewModel.GetReportParametersSet();

			parameters.Add("start_date", dateperiodpicker.StartDateOrNull);
			parameters.Add("end_date", dateperiodpicker.EndDateOrNull);
			parameters.Add("creation_date", DateTime.Now);
			parameters.Add("show_phones", ycheckbuttonPhones.Active);

			if(_userIsSalesRepresentative)
			{
				var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(UoW);

				parameters["Employee_include"] = new[] { currentEmployee.Id.ToString() };
				parameters["Employee_exclude"] = new[] { "0" };
			}

			return new ReportInfo
			{
				Identifier = ycheckbuttonDetail.Active ? "Sales.SalesReportDetail" : "Sales.SalesReport",
				Parameters = parameters
			};
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			if(dateperiodpicker.StartDate != default)
			{
				OnUpdate(true);
			}
			else
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Заполните дату.");
			}
		}

		private void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		private void SetupFilter()
		{
			FilterViewModel.AddFilter<NomenclatureCategory>(config =>
			{
				config.IncludedElements.ListChanged += (_) => UpdateNomenclaturesSpecification();
				config.ExcludedElements.ListChanged += (_) => UpdateNomenclaturesSpecification();
			});

			FilterViewModel.AddFilter(UoW, _nomenclatureRepository);

			FilterViewModel.AddFilter(UoW, _productGroupRepository, x => x.Parent?.Id, x => x.Id, config =>
			{
				config.IncludedElements.ListChanged += (_) => UpdateNomenclaturesSpecification();
				config.ExcludedElements.ListChanged += (_) => UpdateNomenclaturesSpecification();
			});

			FilterViewModel.AddFilter(UoW, _counterpartyRepository);

			FilterViewModel.AddFilter(UoW, _organizationRepository);

			FilterViewModel.AddFilter(UoW, _discountReasonRepository);

			FilterViewModel.AddFilter(UoW, _subdivisionRepository);

			if(!_userIsSalesRepresentative)
			{
				FilterViewModel.AddFilter(UoW, _employeeGenericRepository, config =>
				{
					config.Title = "Авторы заказов";
				});
			}

			FilterViewModel.AddFilter(UoW, _geographicalGroupRepository);

			FilterViewModel.AddFilter<PaymentType>(filterConfig =>
			{
				filterConfig.RefreshFunc = (filter) =>
				{
					var values = Enum.GetValues(typeof(PaymentType));

					filter.FilteredElements.Clear();

					var terminalValues = Enum.GetValues(typeof(PaymentByTerminalSource))
						.Cast<PaymentByTerminalSource>()
						.Where(x => string.IsNullOrWhiteSpace(FilterViewModel.CurrentSearchString)
							|| x.GetEnumTitle().ToLower().Contains(FilterViewModel.CurrentSearchString.ToLower()));

					var paymentValues = _paymentFromRepository.Get(UoW, paymentFrom =>
						(FilterViewModel.ShowArchived || !paymentFrom.IsArchive)
						&& (string.IsNullOrWhiteSpace(FilterViewModel.CurrentSearchString))
							|| paymentFrom.Name.ToLower().Like($"%{FilterViewModel.CurrentSearchString.ToLower()}%"));

					// Заполнение начального списка

					foreach(var value in values)
					{
						if(value is PaymentType enumElement
							&& !filter.HideElements.Contains(enumElement)
							&& (string.IsNullOrWhiteSpace(FilterViewModel.CurrentSearchString)
								|| enumElement.GetEnumTitle().ToLower().Contains(FilterViewModel.CurrentSearchString.ToLower())
								|| (enumElement == PaymentType.Terminal && terminalValues.Any())
								|| (enumElement == PaymentType.PaidOnline && paymentValues.Any())))
						{
							filter.FilteredElements.Add(new IncludeExcludeElement<PaymentType, PaymentType>()
							{
								Id = enumElement,
								Title = enumElement.GetEnumTitle(),
							});
						}
					}

					// Заполнение группы Терминал

					var terminalNode = filter.FilteredElements
						.Where(x => x.Number == nameof(PaymentType.Terminal))
						.FirstOrDefault();

					if(terminalValues.Any())
					{
						foreach(var value in terminalValues)
						{
							if(value is PaymentByTerminalSource enumElement)
							{
								terminalNode.Children.Add(new IncludeExcludeElement<PaymentByTerminalSource, PaymentByTerminalSource>()
								{
									Id = enumElement,
									Parent = terminalNode,
									Title = enumElement.GetEnumTitle(),
								});
							}
						}
					}

					// Заполнение подгруппы Оплачено онлайн

					var paidOnlineNode = filter.FilteredElements
						.Where(x => x.Number == nameof(PaymentType.PaidOnline))
						.FirstOrDefault();

					if(paymentValues.Any())
					{
						var paymentFromValues = paymentValues
							.Select(x => new IncludeExcludeElement<int, PaymentFrom>
							{
								Id = x.Id,
								Parent = paidOnlineNode,
								Title = x.Name,
							});

						foreach(var element in paymentFromValues)
						{
							paidOnlineNode.Children.Add(element);
						}
					}
				};

				filterConfig.GetReportParametersFunc = (filter) =>
				{
					var result = new Dictionary<string, object>();

					// Тип оплаты

					var includePaymentTypeValues = filter.IncludedElements
						.Where(x => x.GetType() == typeof(IncludeExcludeElement<PaymentType, PaymentType>))
						.Select(x => x.Number)
						.ToArray();

					if(includePaymentTypeValues.Length > 0)
					{
						result.Add(typeof(PaymentType).Name + "_include", includePaymentTypeValues);
					}
					else
					{
						result.Add(typeof(PaymentType).Name + "_include", new object[] { "0" });
					}

					var excludePaymentTypeValues = filter.ExcludedElements
						.Where(x => x.GetType() == typeof(IncludeExcludeElement<PaymentType, PaymentType>))
						.Select(x => x.Number)
						.ToArray();

					if(excludePaymentTypeValues.Length > 0)
					{
						result.Add(typeof(PaymentType).Name + "_exclude", excludePaymentTypeValues);
					}
					else
					{
						result.Add(typeof(PaymentType).Name + "_exclude", new object[] { "0" });
					}

					// Оплата по термииналу

					var includePaymentByTerminalSourceValues = filter.IncludedElements
						.Where(x => x.GetType() == typeof(IncludeExcludeElement<PaymentByTerminalSource, PaymentByTerminalSource>))
						.Select(x => x.Number)
						.ToArray();

					if(includePaymentByTerminalSourceValues.Length > 0)
					{
						result.Add(typeof(PaymentByTerminalSource).Name + "_include", includePaymentByTerminalSourceValues);
					}
					else
					{
						result.Add(typeof(PaymentByTerminalSource).Name + "_include", new object[] { "0" });
					}

					var excludePaymentByTerminalSourceValues = filter.ExcludedElements
						.Where(x => x.GetType() == typeof(IncludeExcludeElement<PaymentByTerminalSource, PaymentByTerminalSource>))
						.Select(x => x.Number)
						.ToArray();

					if(excludePaymentByTerminalSourceValues.Length > 0)
					{
						result.Add(typeof(PaymentByTerminalSource).Name + "_exclude", excludePaymentByTerminalSourceValues);
					}
					else
					{
						result.Add(typeof(PaymentByTerminalSource).Name + "_exclude", new object[] { "0" });
					}

					// Оплачено онлайн

					var includePaymentFromValues = filter.IncludedElements
						.Where(x => x.GetType() == typeof(IncludeExcludeElement<int, PaymentFrom>))
						.Select(x => x.Number)
						.ToArray();

					if(includePaymentFromValues.Length > 0)
					{
						result.Add(typeof(PaymentFrom).Name + "_include", includePaymentFromValues);
					}
					else
					{
						result.Add(typeof(PaymentFrom).Name + "_include", new object[] { "0" });
					}

					var excludePaymentFromValues = filter.ExcludedElements
						.Where(x => x.GetType() == typeof(IncludeExcludeElement<int, PaymentFrom>))
						.Select(x => x.Number)
						.ToArray();

					if(excludePaymentFromValues.Length > 0)
					{
						result.Add(typeof(PaymentFrom).Name + "_exclude", excludePaymentFromValues);
					}
					else
					{
						result.Add(typeof(PaymentFrom).Name + "_exclude", new object[] { "0" });
					}

					return result;
				};
			});

			FilterViewModel.AddFilter(UoW, _promotionalSetRepository);

			var filterView = new IncludeExludeFiltersView(FilterViewModel);

			vboxParameters.Add(filterView);
			filterView.Show();
		}

		private void UpdateNomenclaturesSpecification()
		{
			var nomenclauresFilter = FilterViewModel.GetFilter<IncludeExcludeEntityFilter<Nomenclature>>();
			
			nomenclauresFilter.Specification = null;

			nomenclauresFilter.ClearIncludesCommand.Execute();
			nomenclauresFilter.ClearExcludesCommand.Execute();

			var nomenclatureCategoryFilter = FilterViewModel.GetFilter<IncludeExcludeEnumFilter<NomenclatureCategory>>();

			if(nomenclatureCategoryFilter != null)
			{
				var nomenclatureCategoryIncluded = nomenclatureCategoryFilter?.GetIncluded().ToArray();

				var nomenclatureCategoryExcluded = nomenclatureCategoryFilter?.GetExcluded().ToArray();

				if(nomenclatureCategoryIncluded.Length > 0)
				{
					nomenclauresFilter.Specification = nomenclauresFilter.Specification.CombineWith(nomenclature => nomenclatureCategoryIncluded.Contains(nomenclature.Category));
				}

				if(nomenclatureCategoryExcluded.Length > 0)
				{
					nomenclauresFilter.Specification = nomenclauresFilter.Specification.CombineWith(nomenclature => !nomenclatureCategoryExcluded.Contains(nomenclature.Category));
				}
			}

			var productGroupFilter = FilterViewModel.GetFilter<IncludeExcludeEntityWithHierarchyFilter<ProductGroup>>();

			if(productGroupFilter != null)
			{
				var productGroupIncluded = productGroupFilter.GetIncluded().ToArray();

				var productGroupExcluded = productGroupFilter.GetExcluded().ToArray();

				if(productGroupIncluded.Length > 0)
				{
					nomenclauresFilter.Specification = nomenclauresFilter.Specification.CombineWith(nomenclature => productGroupIncluded.Contains(nomenclature.ProductGroup.Id));
				}

				if(productGroupExcluded.Length > 0)
				{
					nomenclauresFilter.Specification = nomenclauresFilter.Specification.CombineWith(nomenclature => !productGroupExcluded.Contains(nomenclature.ProductGroup.Id));
				}
			}
		}
	}
}

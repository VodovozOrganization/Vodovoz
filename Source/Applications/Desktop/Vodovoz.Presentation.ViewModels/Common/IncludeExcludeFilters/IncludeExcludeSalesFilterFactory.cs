using Gamma.Utilities;
using NHibernate.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Client.ClientClassification;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Sale;
using Vodovoz.Extensions;
using Vodovoz.Tools;

namespace Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters
{
	public class IncludeExcludeSalesFilterFactory : IIncludeExcludeSalesFilterFactory
	{
		private const string _includeString = "_include";
		private const string _excludeString = "_exclude";

		private readonly IInteractiveService _interactiveService;
		private readonly IGenericRepository<Nomenclature> _nomenclatureRepository;
		private readonly IGenericRepository<CounterpartySubtype> _counterpartySubtypeRepository;
		private readonly IGenericRepository<Counterparty> _counterpartyRepository;
		private readonly IGenericRepository<Organization> _organizationRepository;
		private readonly IGenericRepository<DiscountReason> _discountReasonRepository;
		private readonly IGenericRepository<Subdivision> _subdivisionRepository;
		private readonly IGenericRepository<Employee> _employeeRepository;
		private readonly IGenericRepository<GeoGroup> _geographicalGroupRepository;
		private readonly IGenericRepository<PromotionalSet> _promotionalSetRepository;
		private readonly IGenericRepository<ProductGroup> _productGroupRepository;
		private readonly IGenericRepository<PaymentFrom> _paymentFromRepository;
		private readonly IGenericRepository<Warehouse> _warehouseRepository;

		public IncludeExcludeSalesFilterFactory(
			IInteractiveService interactiveService,
			IGenericRepository<Nomenclature> nomenclatureRepository,
			IGenericRepository<CounterpartySubtype> counterpartySubtypeRepository,
			IGenericRepository<Counterparty> counterpartyRepository,
			IGenericRepository<ProductGroup> productGroupRepository,
			IGenericRepository<Organization> organizationRepository,
			IGenericRepository<DiscountReason> discountReasonRepository,
			IGenericRepository<Subdivision> subdivisionRepository,
			IGenericRepository<Employee> employeeRepository,
			IGenericRepository<GeoGroup> geographicalGroupRepository,
			IGenericRepository<PromotionalSet> promotionalSetRepository,
			IGenericRepository<PaymentFrom> paymentFromRepository,
			IGenericRepository<Warehouse> warehouseRepository)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_counterpartySubtypeRepository = counterpartySubtypeRepository ?? throw new ArgumentNullException(nameof(counterpartySubtypeRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_geographicalGroupRepository = geographicalGroupRepository ?? throw new ArgumentNullException(nameof(geographicalGroupRepository));
			_promotionalSetRepository = promotionalSetRepository ?? throw new ArgumentNullException(nameof(promotionalSetRepository));
			_productGroupRepository = productGroupRepository ?? throw new ArgumentNullException(nameof(productGroupRepository));
			_paymentFromRepository = paymentFromRepository ?? throw new ArgumentNullException(nameof(paymentFromRepository));
			_warehouseRepository = warehouseRepository ?? throw new ArgumentNullException(nameof(warehouseRepository));
		}

		public IncludeExludeFiltersViewModel CreateSalesReportIncludeExcludeFilter(IUnitOfWork unitOfWork, int? onlyEmployeeId)
		{
			var includeExludeFiltersViewModel = CreateDefaultIncludeExludeFiltersViewModel();

			AddNomenclatureCategoryFilter(includeExludeFiltersViewModel);
			AddNomenclatureFilter(unitOfWork, includeExludeFiltersViewModel);
			AddProductGroupFilter(unitOfWork, includeExludeFiltersViewModel);
			AddCounterpartyTypeFilter(unitOfWork, includeExludeFiltersViewModel);
			AddCounterpartyFilter(unitOfWork, includeExludeFiltersViewModel);
			AddOrganizationFilter(unitOfWork, includeExludeFiltersViewModel);
			AddDiscountReasonFilter(unitOfWork, includeExludeFiltersViewModel);
			AddSubdivisionFilter(unitOfWork, includeExludeFiltersViewModel);

			if(onlyEmployeeId.HasValue)
			{
				AddEmployeeFilter(unitOfWork, includeExludeFiltersViewModel, e => e.Id == onlyEmployeeId, onlyEmployeeId);
				includeExludeFiltersViewModel.Filters.Last().RefreshFilteredElementsCommand.Execute();
			}
			else
			{
				AddEmployeeFilter(unitOfWork, includeExludeFiltersViewModel);
			}
			
			AddGeographicalGroupFilter(unitOfWork, includeExludeFiltersViewModel);
			AddPaymentTypeFilter(unitOfWork, includeExludeFiltersViewModel);
			AddPromotionalSetFilter(unitOfWork, includeExludeFiltersViewModel);

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

			AddOrderStatusFilter(includeExludeFiltersViewModel, statusesToSelect);
			AddCounterpartyCompositeClassificationFilter(includeExludeFiltersViewModel);
			AddSalesManagerFilter(unitOfWork, includeExludeFiltersViewModel);
			
			return includeExludeFiltersViewModel;
		}

		public IncludeExludeFiltersViewModel CreateTurnoverOfWarehouseBalancesReportFilterViewModel(IUnitOfWork unitOfWork)
		{
			var includeExludeFiltersViewModel = CreateDefaultIncludeExludeFiltersViewModel();
			AddNomenclatureCategoryFilter(includeExludeFiltersViewModel);
			AddNomenclatureFilter(unitOfWork, includeExludeFiltersViewModel);
			AddProductGroupFilter(unitOfWork, includeExludeFiltersViewModel);
			AddWarehouseFilter(unitOfWork, includeExludeFiltersViewModel);

			return includeExludeFiltersViewModel;
		}
		
		public IncludeExludeFiltersViewModel CreateCallCenterMotivationReportIncludeExcludeFilter(IUnitOfWork unitOfWork)
		{
			var includeExludeFiltersViewModel = CreateDefaultIncludeExludeFiltersViewModel();

			AddNomenclatureCategoryFilter(includeExludeFiltersViewModel);
			AddNomenclatureFilter(unitOfWork, includeExludeFiltersViewModel);
			AddProductGroupFilter(unitOfWork, includeExludeFiltersViewModel);
			AddCounterpartyTypeFilter(unitOfWork, includeExludeFiltersViewModel);
			AddCounterpartyFilter(unitOfWork, includeExludeFiltersViewModel);
			AddOrganizationFilter(unitOfWork, includeExludeFiltersViewModel);
			AddDiscountReasonFilter(unitOfWork, includeExludeFiltersViewModel);
			AddSubdivisionFilter(unitOfWork, includeExludeFiltersViewModel);
			AddEmployeeFilter(unitOfWork, includeExludeFiltersViewModel);
			AddPromotionalSetFilter(unitOfWork, includeExludeFiltersViewModel);
			
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

			AddOrderStatusFilter(includeExludeFiltersViewModel, statusesToSelect);
			
			var additionalParams = new Dictionary<string, string>
			{
				{ "Самовывоз", "is_self_delivery" },
				{ "Первичные клиенты", "is_first_client" },
			};
			
			includeExludeFiltersViewModel.AddFilter("Дополнительные фильтры", additionalParams);
			
			return includeExludeFiltersViewModel;
		}

		private void AddWarehouseFilter(IUnitOfWork unitOfWork, IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			includeExludeFiltersViewModel.AddFilter(unitOfWork, _warehouseRepository);
		}

		private static void AddCounterpartyCompositeClassificationFilter(IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			includeExludeFiltersViewModel.AddFilter<CounterpartyCompositeClassification>(config =>
			{
				config.RefreshFilteredElements();
			});
		}
		
		private static void AddOrderStatusFilter(IncludeExludeFiltersViewModel includeExludeFiltersViewModel, OrderStatus[] statusesToSelect)
		{
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
		}

		private void AddPromotionalSetFilter(IUnitOfWork unitOfWork, IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			includeExludeFiltersViewModel.AddFilter(unitOfWork, _promotionalSetRepository);
		}

		private void AddPaymentTypeFilter(IUnitOfWork unitOfWork, IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			includeExludeFiltersViewModel.AddFilter<PaymentType>(filterConfig =>
			{
				filterConfig.RefreshFunc = (filter) =>
				{
					var values = Enum.GetValues(typeof(PaymentType));

					filter.FilteredElements.Clear();

					var terminalValues = Enum.GetValues(typeof(PaymentByTerminalSource))
						.Cast<PaymentByTerminalSource>()
						.Where(x => string.IsNullOrWhiteSpace(includeExludeFiltersViewModel.CurrentSearchString)
							|| x.GetEnumTitle().ToLower().Contains(includeExludeFiltersViewModel.CurrentSearchString.ToLower()));

					var paymentValues = _paymentFromRepository.Get(unitOfWork, paymentFrom =>
						(includeExludeFiltersViewModel.ShowArchived || !paymentFrom.IsArchive)
						&& (
								string.IsNullOrWhiteSpace(includeExludeFiltersViewModel.CurrentSearchString)
								|| paymentFrom.Name.ToLower().Like($"%{includeExludeFiltersViewModel.CurrentSearchString.ToLower()}%")
							));

					// Заполнение начального списка

					foreach(var value in values)
					{
						if(value is PaymentType enumElement
							&& !filter.HideElements.Contains(enumElement)
							&& (string.IsNullOrWhiteSpace(includeExludeFiltersViewModel.CurrentSearchString)
								|| enumElement.GetEnumTitle().ToLower().Contains(includeExludeFiltersViewModel.CurrentSearchString.ToLower())
								|| enumElement == PaymentType.Terminal && terminalValues.Any()
								|| enumElement == PaymentType.PaidOnline && paymentValues.Any()))
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
						.FirstOrDefault(x => x.Number == nameof(PaymentType.PaidOnline));

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

				filterConfig.GetReportParametersFunc = CustomReportParametersFunc.PaymentTypeReportParametersFunc;
			});
		}

		private void AddGeographicalGroupFilter(IUnitOfWork unitOfWork, IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			includeExludeFiltersViewModel.AddFilter(unitOfWork, _geographicalGroupRepository);
		}

		private void AddEmployeeFilter(
			IUnitOfWork unitOfWork,
			IncludeExludeFiltersViewModel includeExludeFiltersViewModel,
			Expression<Func<Employee, bool>> specificationExpression = null,
			int? onlyEmployeeId = null)
		{
			includeExludeFiltersViewModel.AddFilter(unitOfWork, _employeeRepository, config =>
			{
				config.Title = "Авторы заказов";
				config.DefaultName = "OrderAuthor";
				config.GenitivePluralTitle = "Авторов заказов";

				config.RefreshFunc = (IncludeExcludeEntityFilter<Employee> filter) =>
				{
					var splitedWords = includeExludeFiltersViewModel.CurrentSearchString.Split(' ');
					var currentSpecification = specificationExpression;

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

						currentSpecification = currentSpecification.CombineWith(searchInFullNameSpec);
					}

					var elementsToAdd = _employeeRepository.Get(
							unitOfWork,
							currentSpecification,
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

						if(onlyEmployeeId.HasValue && onlyEmployeeId.Value == element.Id)
						{
							element.Include = true;
							element.IsEditable = false;
						}
					}
				};
			});
		}

		private void AddSalesManagerFilter(IUnitOfWork unitOfWork, IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			includeExludeFiltersViewModel.AddFilter(unitOfWork, _employeeRepository, config =>
			{
				config.Title = "Менеджеры КА";
				config.GenitivePluralTitle = "Менеджеров КА";
				config.DefaultName = "SalesManager";
				config.RefreshFunc = (IncludeExcludeEntityFilter<Employee> filter) =>
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
		
		private void AddSubdivisionFilter(IUnitOfWork unitOfWork, IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			includeExludeFiltersViewModel.AddFilter(unitOfWork, _subdivisionRepository);
		}

		private void AddDiscountReasonFilter(IUnitOfWork unitOfWork, IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			includeExludeFiltersViewModel.AddFilter(unitOfWork, _discountReasonRepository);
		}

		private void AddOrganizationFilter(IUnitOfWork unitOfWork, IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			includeExludeFiltersViewModel.AddFilter(unitOfWork, _organizationRepository);
		}

		private void AddCounterpartyFilter(IUnitOfWork unitOfWork, IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			includeExludeFiltersViewModel.AddFilter(unitOfWork, _counterpartyRepository);
		}

		private void AddCounterpartyTypeFilter(IUnitOfWork unitOfWork, IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			includeExludeFiltersViewModel.AddFilter<CounterpartyType>(filterConfig =>
			{
				filterConfig.RefreshFunc = (filter) =>
				{
					var values = Enum.GetValues(typeof(CounterpartyType));

					filter.FilteredElements.Clear();

					var counterpartySubtypeValues = _counterpartySubtypeRepository.Get(unitOfWork, counterpartySubtype => string.IsNullOrWhiteSpace(includeExludeFiltersViewModel.CurrentSearchString)
							|| counterpartySubtype.Name.ToLower().Like($"%{includeExludeFiltersViewModel.CurrentSearchString.ToLower()}%"));

					// Заполнение начального списка

					foreach(var value in values)
					{
						if(value is CounterpartyType enumElement
							&& !filter.HideElements.Contains(enumElement)
							&& (string.IsNullOrWhiteSpace(includeExludeFiltersViewModel.CurrentSearchString)
								|| enumElement.GetEnumTitle().ToLower().Contains(includeExludeFiltersViewModel.CurrentSearchString.ToLower())
								|| enumElement == CounterpartyType.AdvertisingDepartmentClient && counterpartySubtypeValues.Any()))
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
		}

		private void AddProductGroupFilter(IUnitOfWork unitOfWork, IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			includeExludeFiltersViewModel.AddFilter(unitOfWork, _productGroupRepository, x => x.Parent?.Id, x => x.Id, config =>
			{
				config.IncludedElements.ListChanged += (_) => UpdateNomenclaturesSpecification(includeExludeFiltersViewModel);
				config.ExcludedElements.ListChanged += (_) => UpdateNomenclaturesSpecification(includeExludeFiltersViewModel);
			});
		}

		private void AddNomenclatureFilter(IUnitOfWork unitOfWork, IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			includeExludeFiltersViewModel.AddFilter(unitOfWork, _nomenclatureRepository);
		}

		private void AddNomenclatureCategoryFilter(IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			includeExludeFiltersViewModel.AddFilter<NomenclatureCategory>(config =>
			{
				config.IncludedElements.ListChanged += (_) => UpdateNomenclaturesSpecification(includeExludeFiltersViewModel);
				config.ExcludedElements.ListChanged += (_) => UpdateNomenclaturesSpecification(includeExludeFiltersViewModel);
			});
		}

		private IncludeExludeFiltersViewModel CreateDefaultIncludeExludeFiltersViewModel()
		{
			return new IncludeExludeFiltersViewModel(_interactiveService);
		}

		private void UpdateNomenclaturesSpecification(IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			var nomenclauresFilter = includeExludeFiltersViewModel.GetFilter<IncludeExcludeEntityFilter<Nomenclature>>();

			nomenclauresFilter.Specification = null;

			nomenclauresFilter.ClearIncludesCommand.Execute();
			nomenclauresFilter.ClearExcludesCommand.Execute();

			var nomenclatureCategoryFilter = includeExludeFiltersViewModel.GetFilter<IncludeExcludeEnumFilter<NomenclatureCategory>>();

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

			var productGroupFilter = includeExludeFiltersViewModel.GetFilter<IncludeExcludeEntityWithHierarchyFilter<ProductGroup>>();

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

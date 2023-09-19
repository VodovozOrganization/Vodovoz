using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories;
using Vodovoz.Presentation.ViewModels.Common;
using Gamma.Utilities;
using Vodovoz.Extensions;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using QS.Dialog;
using Vodovoz.ViewModels.ViewModels.Flyers;
using System.Linq.Expressions;
using QS.DomainModel.Entity;

namespace Vodovoz.ViewModels.Factories
{
	public class IncludeExcludeSalesFilterFactory : IIncludeExcludeSalesFilterFactory
	{
		private readonly IInteractiveService _interactiveService;
		private readonly IGenericRepository<Nomenclature> _nomenclatureRepository;
		private readonly IGenericRepository<Counterparty> _counterpartyRepository;
		private readonly IGenericRepository<Organization> _organizationRepository;
		private readonly IGenericRepository<DiscountReason> _discountReasonRepository;
		private readonly IGenericRepository<Subdivision> _subdivisionRepository;
		private readonly IGenericRepository<Employee> _employeeRepository;
		private readonly IGenericRepository<GeoGroup> _geographicalGroupRepository;
		private readonly IGenericRepository<PromotionalSet> _promotionalSetRepository;
		private readonly IGenericRepository<ProductGroup> _productGroupRepository;
		private readonly IGenericRepository<PaymentFrom> _paymentFromRepository;

		public IncludeExcludeSalesFilterFactory(
			IInteractiveService interactiveService,
			IGenericRepository<Nomenclature> nomenclatureRepository,
			IGenericRepository<Counterparty> counterpartyRepository,
			IGenericRepository<ProductGroup> productGroupRepository,
			IGenericRepository<Organization> organizationRepository,
			IGenericRepository<DiscountReason> discountReasonRepository,
			IGenericRepository<Subdivision> subdivisionRepository,
			IGenericRepository<Employee> employeeRepository,
			IGenericRepository<GeoGroup> geographicalGroupRepository,
			IGenericRepository<PromotionalSet> promotionalSetRepository,
			IGenericRepository<PaymentFrom> paymentFromRepository)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_geographicalGroupRepository = geographicalGroupRepository ?? throw new ArgumentNullException(nameof(geographicalGroupRepository));
			_promotionalSetRepository = promotionalSetRepository ?? throw new ArgumentNullException(nameof(promotionalSetRepository));
			_productGroupRepository = productGroupRepository ?? throw new ArgumentNullException(nameof(productGroupRepository));
			_paymentFromRepository = paymentFromRepository ?? throw new ArgumentNullException(nameof(paymentFromRepository));
		}

		public IncludeExludeFiltersViewModel CreateSalesReportIncludeExcludeFilter(IUnitOfWork unitOfWork, bool userIsSalesRepresentative)
		{
			var includeExludeFiltersViewModel = CreateDefaultIncludeExludeFiltersViewModel();

			includeExludeFiltersViewModel.AddFilter<NomenclatureCategory>(config =>
			{
				config.IncludedElements.ListChanged += (_) => UpdateNomenclaturesSpecification(includeExludeFiltersViewModel);
				config.ExcludedElements.ListChanged += (_) => UpdateNomenclaturesSpecification(includeExludeFiltersViewModel);
			});

			includeExludeFiltersViewModel.AddFilter(unitOfWork, _nomenclatureRepository);

			includeExludeFiltersViewModel.AddFilter(unitOfWork, _productGroupRepository, x => x.Parent?.Id, x => x.Id, config =>
			{
				config.IncludedElements.ListChanged += (_) => UpdateNomenclaturesSpecification(includeExludeFiltersViewModel);
				config.ExcludedElements.ListChanged += (_) => UpdateNomenclaturesSpecification(includeExludeFiltersViewModel);
			});

			includeExludeFiltersViewModel.AddFilter(unitOfWork, _counterpartyRepository);

			includeExludeFiltersViewModel.AddFilter(unitOfWork, _organizationRepository);

			includeExludeFiltersViewModel.AddFilter(unitOfWork, _discountReasonRepository);

			includeExludeFiltersViewModel.AddFilter(unitOfWork, _subdivisionRepository);

			if(!userIsSalesRepresentative)
			{
				includeExludeFiltersViewModel.AddFilter(unitOfWork, _employeeRepository, config =>
				{
					config.Title = "Авторы заказов";

					config.RefreshFunc = (IncludeExcludeEntityFilter<Employee> filter) =>
					{
						Expression<Func<Employee, bool>> specificationExpression = null;

						Expression<Func<Employee, bool>> searchInFullNameSpec = employee =>
							string.IsNullOrWhiteSpace(includeExludeFiltersViewModel.CurrentSearchString)
							|| employee.Name.ToLower().Like($"%{includeExludeFiltersViewModel.CurrentSearchString.ToLower()}%")
							|| employee.LastName.ToLower().Like($"%{includeExludeFiltersViewModel.CurrentSearchString.ToLower()}%")
							|| employee.Patronymic.ToLower().Like($"%{includeExludeFiltersViewModel.CurrentSearchString.ToLower()}%");

						specificationExpression = specificationExpression.CombineWith(searchInFullNameSpec);

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

			includeExludeFiltersViewModel.AddFilter(unitOfWork, _geographicalGroupRepository);

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
						&& (string.IsNullOrWhiteSpace(includeExludeFiltersViewModel.CurrentSearchString))
							|| paymentFrom.Name.ToLower().Like($"%{includeExludeFiltersViewModel.CurrentSearchString.ToLower()}%"));

					// Заполнение начального списка

					foreach(var value in values)
					{
						if(value is PaymentType enumElement
							&& !filter.HideElements.Contains(enumElement)
							&& (string.IsNullOrWhiteSpace(includeExludeFiltersViewModel.CurrentSearchString)
								|| enumElement.GetEnumTitle().ToLower().Contains(includeExludeFiltersViewModel.CurrentSearchString.ToLower())
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

			includeExludeFiltersViewModel.AddFilter(unitOfWork, _promotionalSetRepository);

			var statusesToSelect = new[] {
				OrderStatus.Accepted,
				OrderStatus.InTravelList,
				OrderStatus.OnLoading,
				OrderStatus.OnTheWay,
				OrderStatus.Shipped,
				OrderStatus.UnloadingOnStock,
				OrderStatus.WaitForPayment,
				OrderStatus.Closed };

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

			return includeExludeFiltersViewModel;
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

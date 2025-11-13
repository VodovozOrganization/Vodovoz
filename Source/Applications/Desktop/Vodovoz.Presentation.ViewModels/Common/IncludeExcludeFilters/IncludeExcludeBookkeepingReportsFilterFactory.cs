using Gamma.Utilities;
using NHibernate.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters
{
	public partial class IncludeExcludeBookkeepingReportsFilterFactory : IIncludeExcludeBookkeepingReportsFilterFactory
	{
		private const string _includeString = "_include";
		private const string _excludeString = "_exclude";

		private readonly IInteractiveService _interactiveService;
		private readonly IGenericRepository<CounterpartySubtype> _counterpartySubtypeRepository;
		private readonly IGenericRepository<Counterparty> _counterpartyRepository;
		private readonly IGenericRepository<PaymentFrom> _paymentFromRepository;

		public IncludeExcludeBookkeepingReportsFilterFactory(
			IInteractiveService interactiveService,
			IGenericRepository<CounterpartySubtype> counterpartySubtypeRepository,
			IGenericRepository<Counterparty> counterpartyRepository,
			IGenericRepository<PaymentFrom> paymentFromRepository)
		{
			_interactiveService = interactiveService ?? throw new System.ArgumentNullException(nameof(interactiveService));
			_counterpartySubtypeRepository = counterpartySubtypeRepository ?? throw new ArgumentNullException(nameof(counterpartySubtypeRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_paymentFromRepository = paymentFromRepository ?? throw new ArgumentNullException(nameof(paymentFromRepository));
		}

		public IncludeExludeFiltersViewModel CreateEdoControlReportIncludeExcludeFilter(IUnitOfWork unitOfWork)
		{
			var filter = CreateDefaultIncludeExludeFiltersViewModel();

			AddCounterpartyTypeFilter(unitOfWork, filter);
			AddCounterpartyFilter(unitOfWork, filter);
			AddCounterpartyPersonTypeFilter(filter);
			AddPaymentTypeFilter(unitOfWork, filter);
			AddEdoDocFlowStatusFilter(filter);
			AddDeliveryTypeFilter(filter);
			AddAddressTransferTypeFilter(filter);

			return filter;
		}

		private IncludeExludeFiltersViewModel CreateDefaultIncludeExludeFiltersViewModel()
		{
			return new IncludeExludeFiltersViewModel(_interactiveService);
		}

		private void AddCounterpartyTypeFilter(IUnitOfWork unitOfWork, IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			includeExludeFiltersViewModel.AddFilter<CounterpartyType>(filterConfig =>
			{
				filterConfig.RefreshFunc = (filter) =>
				{
					var values = Enum.GetValues(typeof(CounterpartyType));

					filter.FilteredElements.Clear();

					var counterpartySubtypeValues =
					_counterpartySubtypeRepository
					.Get(unitOfWork, counterpartySubtype => string.IsNullOrWhiteSpace(includeExludeFiltersViewModel.CurrentSearchString)
							|| counterpartySubtype.Name.ToLower().Like($"%{includeExludeFiltersViewModel.CurrentSearchString.ToLower()}%"));

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

		private void AddCounterpartyFilter(IUnitOfWork unitOfWork, IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			includeExludeFiltersViewModel.AddFilter(unitOfWork, _counterpartyRepository);
		}

		private void AddCounterpartyPersonTypeFilter(IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			includeExludeFiltersViewModel.AddFilter<PersonType>(config =>
			{
				config.RefreshFilteredElements();
			});
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
						&& string.IsNullOrWhiteSpace(includeExludeFiltersViewModel.CurrentSearchString)
							|| paymentFrom.Name.ToLower().Like($"%{includeExludeFiltersViewModel.CurrentSearchString.ToLower()}%"));

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

		private void AddEdoDocFlowStatusFilter(IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			includeExludeFiltersViewModel.AddFilter<EdoControlReportDocFlowStatus>(config =>
			{
				config.RefreshFilteredElements();
			});
		}

		private void AddDeliveryTypeFilter(IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			includeExludeFiltersViewModel.AddFilter<EdoControlReportOrderDeliveryType>(config =>
			{
				config.RefreshFilteredElements();
			});
		}

		private void AddAddressTransferTypeFilter(IncludeExludeFiltersViewModel includeExludeFiltersViewModel)
		{
			includeExludeFiltersViewModel.AddFilter<EdoControlReportAddressTransferType>(config =>
			{
				config.RefreshFilteredElements();
			});
		}
	}
}

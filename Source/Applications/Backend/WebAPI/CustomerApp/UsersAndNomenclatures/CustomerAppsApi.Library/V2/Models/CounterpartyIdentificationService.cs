using System;
using System.Threading;
using System.Threading.Tasks;
using CustomerAppsApi.Library.V2.Dto.Counterparties;
using CustomerAppsApi.Library.V2.Models.Identification;

namespace CustomerAppsApi.Library.V2.Models
{
	/// <summary>
	/// Фасад сервисов идентификации клиента
	/// </summary>
	public class CounterpartyIdentificationService : ICounterpartyIdentificationService
	{
		private readonly ICounterpartyPhoneSearchService _phoneSearchService;
		private readonly ICounterpartyNameSearchService _nameSearchService;
		private readonly ICounterpartyAddressSearchService _addressSearchService;
		private readonly ICounterpartyOrderHistoryService _orderHistoryService;

		public CounterpartyIdentificationService(
			ICounterpartyPhoneSearchService phoneSearchService,
			ICounterpartyNameSearchService nameSearchService,
			ICounterpartyAddressSearchService addressSearchService,
			ICounterpartyOrderHistoryService orderHistoryService)
		{
			_phoneSearchService = phoneSearchService
				?? throw new ArgumentNullException(nameof(phoneSearchService));
			_nameSearchService = nameSearchService
				?? throw new ArgumentNullException(nameof(nameSearchService));
			_addressSearchService = addressSearchService
				?? throw new ArgumentNullException(nameof(addressSearchService));
			_orderHistoryService = orderHistoryService
				?? throw new ArgumentNullException(nameof(orderHistoryService));
		}

		/// <inheritdoc />
		public Task<CounterpartySearchResultDto> SearchByPhone(
			CounterpartyPhoneSearchDto request,
			CancellationToken cancellationToken = default)
		{
			return _phoneSearchService.SearchAsync(request, cancellationToken);
		}

		/// <inheritdoc />
		public Task<CounterpartySearchResultDto> SearchByName(
			CounterpartyNameSearchDto request,
			CancellationToken cancellationToken = default)
		{
			return _nameSearchService.SearchAsync(request, cancellationToken);
		}

		/// <inheritdoc />
		public Task<CounterpartySearchResultDto> SearchByAddress(
			CounterpartyAddressSearchDto request,
			CancellationToken cancellationToken = default)
		{
			return _addressSearchService.SearchAsync(request, cancellationToken);
		}

		/// <inheritdoc />
		public Task<CounterpartyOrderHistoryDto> GetOrderHistory(
			CounterpartyOrderHistoryRequestDto request,
			CancellationToken cancellationToken = default)
		{
			return _orderHistoryService.GetAsync(request, cancellationToken);
		}
	}
}

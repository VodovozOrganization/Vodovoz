using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dadata;
using Dadata.Model;
using NLog;
using RevenueService.Client.Dto;
using RevenueService.Client.Parsers;

namespace RevenueService.Client
{
	public class RevenueServiceClient : IRevenueServiceClient
	{
		private readonly string _accessToken;
		private const int _queryCount = 100;
		private static ILogger _logger = LogManager.GetCurrentClassLogger();

		public RevenueServiceClient(string accessToken)
		{
			_accessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
		}

		public async Task<RevenueServiceResponseDto> GetCounterpartyInfoAsync(DadataRequestDto query, CancellationToken cancellationToken)
		{
			var api = new SuggestClientAsync(_accessToken);

			var request = string.IsNullOrEmpty(query.Kpp)
				? new FindPartyRequest(query.Inn, count: _queryCount)
				: new FindPartyRequest(query.Inn, query.Kpp, _queryCount);

			var suggestionList = new List<CounterpartyRevenueServiceDto>();

			SuggestResponse<Party> response;

			try
			{
				response = await api.FindParty(request, cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.Error(ex, "Ошибка при загрузке реквизитов контрагента.");

				return new RevenueServiceResponseDto()
				{
					ErrorMessage = $"Ошибка при загрузке реквизитов контрагента: {ex.Message}",
					CounterpartyDetailsList = suggestionList
				};
			}

			var branchTypeParser = new BranchTypeParsser();
			var personTypeParser = new CounterpartyTypeParser();

			foreach(var suggestion in response.suggestions)
			{
				var dto = new CounterpartyRevenueServiceDto
				{
					Inn = suggestion.data.inn,
					Kpp = suggestion.data.kpp,
					ShortName = suggestion.data.name?.@short,
					FullName = suggestion.data.name?.full,
					Address = suggestion.data.address?.value,
					PersonSurname = suggestion.data.fio?.surname,
					PersonName = suggestion.data.fio?.name,
					PersonPatronymic = suggestion.data.fio?.patronymic,
					ManagerFullName = suggestion.data.management?.name,
					BranchType = branchTypeParser.Parse(suggestion.data.branch_type),
					CounterpartyType = personTypeParser.Parse(suggestion.data.type),
					Opf = suggestion.data.opf?.@short,
					OpfFull = suggestion.data.opf?.@full,
					Emails = suggestion.data.emails?.Select(x => x.value).ToArray(),
					Phones = suggestion.data.phones?.Select(x => x.value).ToArray(),
					State = suggestion.data.state.status,
					StateDate = suggestion.data.state.actuality_date,
				};

				var postalCode = suggestion.data.address?.data.postal_code;

				if(!string.IsNullOrEmpty(postalCode) && dto.Address.Length >= postalCode.Length)
				{
					var leftAddressSubstring = dto.Address.Substring(0, postalCode.Length);

					if(!int.TryParse(leftAddressSubstring, out _))
					{
						dto.Address = $"{postalCode}, {dto.Address}";
					}
				}

				suggestionList.Add(dto);
			}

			return new RevenueServiceResponseDto
			{
				CounterpartyDetailsList = suggestionList
			};
		}

		public async Task<PartyStatus> GetCounterpartyStatus(
			string inn,
			string kpp,
			CancellationToken cancellationToken)
		{
			var query = new DadataRequestDto
			{
				Inn = inn,
				Kpp = kpp
			};

			var response = await GetCounterpartyInfoAsync(query, cancellationToken);

			try
			{
				var counterpartyDetails = response.CounterpartyDetailsList.Single();

				return counterpartyDetails.State;
			}
			catch(InvalidOperationException ex) when(ex.Message == "Последовательность содержит более одного элемента"
			                                         && ex.TargetSite.Name == "Single")
			{
				throw new InvalidOperationException($"Найдено несколько записей в ФНС с ИНН: \"{inn}\" и КПП: \"{kpp}\"", ex);
			}
			catch(InvalidOperationException ex) when(ex.Message == "Последовательность не содержит элементов"
			                                         && ex.TargetSite.Name == "Single")
			{
				throw new InvalidOperationException($"Не найдено ни одной записи в ФНС с ИНН: \"{inn}\" и КПП: \"{kpp}\"", ex);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dadata;
using Dadata.Model;
using QS.Dialog;
using QS.Services;
using RevenueService.Client.Dto;
using RevenueService.Client.Parsers;

namespace RevenueService.Client
{
	public class RevenueServiceClient : IRevenueServiceClient
	{
		private readonly string _accessToken;
		private const int _queryCount = 100;

		public RevenueServiceClient(string accessToken)
		{
			_accessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
		}

		public async Task<RevenueServiceResponseDto> GetCounterpartyInfoAsync(DadataRequestDto query, CancellationToken cancellationToken)
		{
			var api = new SuggestClientAsync(_accessToken);
			var request = string.IsNullOrEmpty(query.Kpp) ? new FindPartyRequest(query.Inn, count: _queryCount) : new FindPartyRequest(query.Inn, query.Kpp, _queryCount);

			var suggestionList = new List<CounterpartyRevenueServiceDto>();

			SuggestResponse<Party> response;

			try
			{
				response = await api.FindParty(request, cancellationToken);
			}
			catch(Exception ex)
			{
				return new RevenueServiceResponseDto()
				{
					ErrorMessage = $"Ошибка при загрузке реквизитов контрагента: {ex.Message}",
					CounterpartyDetailsList = suggestionList
				};
			}

			var branchTypeParser = new BranchTypeParsser();
			var personTypeParser = new PersonTypeParser();

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
					PersonType = personTypeParser.Parse(suggestion.data.type),
					Emails = suggestion.data.emails?.Select(x => x.value).ToArray(),
					Phones = suggestion.data.phones?.Select(x => x.value).ToArray(),
					State = suggestion.data.state.status.ToString()
				};

				suggestionList.Add(dto);
			}

			return new RevenueServiceResponseDto
			{
				CounterpartyDetailsList = suggestionList
			};
		}
	}
}

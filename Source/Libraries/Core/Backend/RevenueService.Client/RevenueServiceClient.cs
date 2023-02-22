using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dadata;
using Dadata.Model;
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

		public async Task<IList<CounterpartyDto>> GetCounterpartyInfoAsync(DadataRequestDto query, CancellationToken cancellationToken)
		{
			var api = new SuggestClientAsync(_accessToken);
			var request = string.IsNullOrEmpty(query.Kpp) ? new FindPartyRequest(query.Inn, count: _queryCount) : new FindPartyRequest(query.Inn, query.Kpp, _queryCount);
			var response = await api.FindParty(request, cancellationToken);
			var suggestionList = new List<CounterpartyDto>();

			var branchTypeParser = new BranchTypeParsser();
			var typeOfOwnershipParser = new TypeOfOwnershipParser();

			foreach(var suggestion in response.suggestions)
			{
				var dto = new CounterpartyDto
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
					TypeOfOwnerShip = typeOfOwnershipParser.Parse(suggestion.data.type),
					Emails = suggestion.data.emails?.Select(x => x.value).ToArray(),
					Phones = suggestion.data.phones?.Select(x => x.value).ToArray(),
					State = suggestion.data.state.status.ToString()
				};

				suggestionList.Add(dto);
			}

			return suggestionList;
		}
	}
}

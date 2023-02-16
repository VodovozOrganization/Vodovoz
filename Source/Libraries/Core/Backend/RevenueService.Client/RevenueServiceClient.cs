using System;
using System.Collections.Generic;
using System.Linq;
using Dadata;

namespace RevenueService.Client
{
	public class RevenueServiceClient : IRevenueServiceClient
	{
		private readonly string _accessToken;

		public RevenueServiceClient(string accessToken)
		{
			_accessToken = accessToken?? throw new ArgumentNullException(nameof(accessToken));
		}

		public IList<RevenueServiceCounterpartyDto> GetCounterpartyInfoFromRevenueService(string inn, string kpp = null)
		{
			//var token =  "86d39d5fd5c9c9f6436acce73a8cc298561e5975";
			var api = new SuggestClientAsync(_accessToken);
			var response = api.SuggestParty(inn).Result;
			var suggestionList = new List<RevenueServiceCounterpartyDto>();

			foreach(var suggestion in response.suggestions)
			{
				var dto = new RevenueServiceCounterpartyDto
				{
					Inn = suggestion.data.inn,
					Kpp = suggestion.data.kpp,
					ShortName = suggestion.data.name?.@short,
					FullName = suggestion.data.name?.full,
					Address = suggestion.data.address?.value,
					PersonSurname = suggestion.data.fio?.surname,
					PersonName = suggestion.data.fio?.name,
					PersonPatronymic = suggestion.data.fio?.patronymic,
					LegalPersonFullName = suggestion.data.management?.name,
					BranchType = suggestion.data.branch_type.ToString(),
					TypeOfOwnerShip = suggestion.data.type.ToString(),
					Emails = suggestion.data.emails?.Select(x => x.value).ToArray(),
					Phones = suggestion.data.phones?.Select(x => x.value).ToArray()
				};

				suggestionList.Add(dto);
			}

			return suggestionList;
		}
	}
}

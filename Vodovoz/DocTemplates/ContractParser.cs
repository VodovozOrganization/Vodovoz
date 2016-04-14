using System;
using QSDocTemplates;
using Vodovoz.Domain.Client;

namespace Vodovoz.DocTemplates
{
	public class ContractParser : DocParserBase<CounterpartyContract>
	{
		public ContractParser()
		{
		}

		public override void UpdateFields()
		{
			fieldsList.Clear();

			//Сам договор
			AddField(x => x.Number, PatternFieldType.FString);
			AddField(x => x.IssueDate, PatternFieldType.FDate);

			//Организаци
			AddField(x => x.Organization.FullName, PatternFieldType.FString);
			AddField(x => x.Organization.Address, PatternFieldType.FString);
			AddField(x => x.Organization.INN, PatternFieldType.FString);
			AddField(x => x.Organization.KPP, PatternFieldType.FString);
			AddField(x => x.Organization.JurAddress, PatternFieldType.FString);
			AddField(x => x.Organization.OGRN, PatternFieldType.FString);

			SortFields();
		}
	}
}


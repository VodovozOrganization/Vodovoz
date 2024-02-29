﻿using QS.DocTemplates;
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

			if(RootObject != null)
			{
				RootObject.Organization.SetActiveOrganizationVersion(RootObject.Organization.OrganizationVersionOnDate(RootObject.IssueDate));
			}

			//Сам договор
			AddField(x => x.Number, PatternFieldType.FString);
			AddField(x => x.ContractFullNumber, PatternFieldType.FString);
			AddField(x => x.IssueDate, PatternFieldType.FDate);

			//Организаци
			AddField(x => x.Organization.FullName, PatternFieldType.FString);
			AddField(x => x.Organization.ActiveOrganizationVersion.Address, PatternFieldType.FString);
			AddField(x => x.Organization.INN, PatternFieldType.FString);
			AddField(x => x.Organization.KPP, PatternFieldType.FString);
			AddField(x => x.Organization.ActiveOrganizationVersion.JurAddress, PatternFieldType.FString);
			AddField(x => x.Organization.OGRN, PatternFieldType.FString);
			//Расчетный счет
			AddField(x => x.Organization.DefaultAccount.Number, PatternFieldType.FString);
			AddField(x => x.Organization.DefaultAccount.InBank.Bik, PatternFieldType.FString);
			AddField(x => x.Organization.DefaultAccount.BankCorAccount.CorAccountNumber, PatternFieldType.FString);
			AddField(x => x.Organization.DefaultAccount.InBank.Name, PatternFieldType.FString);
			//Директор организации
			AddField(x => x.Organization.ActiveOrganizationVersion.Leader.FullName, PatternFieldType.FString);
			AddField(x => x.Organization.ActiveOrganizationVersion.Leader.ShortName, PatternFieldType.FString);

			//Клиент
			AddField(x => x.Counterparty.FullName, PatternFieldType.FString);
			AddField(x => x.Counterparty.Address, PatternFieldType.FString);
			AddField(x => x.Counterparty.INN, PatternFieldType.FString);
			AddField(x => x.Counterparty.KPP, PatternFieldType.FString);
			AddField(x => x.Counterparty.JurAddress, PatternFieldType.FString);
			//Расчетный счет
			if(RootObject?.Counterparty?.DefaultAccount != null)
			{
				AddField(x => x.Counterparty.DefaultAccount.Number, PatternFieldType.FString);
				AddField(x => x.Counterparty.DefaultAccount.InBank.Bik, PatternFieldType.FString);
				AddField(x => x.Counterparty.DefaultAccount.BankCorAccount.CorAccountNumber, PatternFieldType.FString);
				AddField(x => x.Counterparty.DefaultAccount.InBank.Name, PatternFieldType.FString);
			}
			//Директор клиента
			AddField(x => x.Counterparty.SignatoryFIO, PatternFieldType.FString);
			AddField(x => x.Counterparty.SignatoryPost, PatternFieldType.FString);
			AddField(x => x.Counterparty.SignatoryBaseOf, PatternFieldType.FString);
				
			SortFields();
		}
	}
}


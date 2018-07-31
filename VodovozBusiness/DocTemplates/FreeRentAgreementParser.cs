using System;
using System.Linq;
using QSDocTemplates;
using Vodovoz.Domain.Client;

namespace Vodovoz.DocTemplates
{
	public class FreeRentAgreementParser : DocParserBase<FreeRentAgreement>
	{
		public FreeRentAgreementParser()
		{
		}

		public override void UpdateFields()
		{
			tablesList.Clear();
			customTablesList.Clear();
			fieldsList.Clear();

			//Сам договор
			AddField(x => x.Contract.IssueDate, PatternFieldType.FDate);
			AddField(x => x.Contract.Counterparty.FullName,  PatternFieldType.FString);
			AddField(x => x.Contract.Counterparty.SignatoryBaseOf,  PatternFieldType.FString);
			AddField(x => x.Contract.Number, PatternFieldType.FString);
			AddField(x => x.Contract.Counterparty.SignatoryFIO, PatternFieldType.FString);
			AddField(x => x.Contract.Counterparty.SignatoryPost, PatternFieldType.FString);
			//Само соглашение
			AddField(x => x.FullNumberText, PatternFieldType.FString);
			AddField(x => x.StartDate, PatternFieldType.FDate);
			AddField(x => x.IssueDate, PatternFieldType.FDate);
			AddField(x => x.DeliveryPoint.CompiledAddressWOAddition, PatternFieldType.FString);

			AddField(x => String.Join(", ",
									  x.DeliveryPoint != null && x.DeliveryPoint.Phones.Any()
									  ? x.DeliveryPoint.Phones.Select(p => p.ToString())
									  : x.Contract.Counterparty.Phones.Select(p => p.ToString())
									 ), "Телефоны", PatternFieldType.FString);

			AddTable(x => x.Equipment)
				.AddColumn(x => x.WaterAmount, PatternFieldType.FString)
				.AddColumn(x => x.Nomenclature.OfficialName, PatternFieldType.FString)
				.AddColumn(x => x.Deposit, PatternFieldType.FString)
				.AddColumn(x => x.Nomenclature.SumOfDamage, PatternFieldType.FString);
			
			SortFields();
		}
	}
}


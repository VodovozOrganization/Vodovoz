using System;
using QSDocTemplates;
using Vodovoz.Domain.Client;

namespace Vodovoz.DocTemplates
{
	public class WaterAgreementParser : DocParserBase<WaterSalesAgreement>
	{
		public WaterAgreementParser()
		{
		}

		public override void UpdateFields()
		{
			fieldsList.Clear();
			//Сам договор
			AddField(x => x.Contract.IssueDate, PatternFieldType.FDate);
			AddField(x => x.Contract.Counterparty.FullName,  PatternFieldType.FString);
			AddField(x => x.Contract.Counterparty.SignatoryBaseOf,  PatternFieldType.FString);
			AddField(x => x.Contract.Number, PatternFieldType.FString);
			AddField(x => x.Contract.Counterparty.SignatoryFIO, PatternFieldType.FString);
			AddField(x => x.Contract.Counterparty.SignatoryPost, PatternFieldType.FString);
			//Само соглашение
			AddField(x => x.AgreementNumber, PatternFieldType.FString);
			AddField(x => x.StartDate, PatternFieldType.FDate);
			AddField(x => x.IssueDate, PatternFieldType.FDate);
			AddField(x => x.DeliveryPoint.CompiledAddress, PatternFieldType.FString);

			AddTable(x => x.FixedPrices)
				.AddColumn(x => x.Nomenclature.OfficialName, PatternFieldType.FString)
				.AddColumn(x => x.Price, PatternFieldType.FCurrency);
			

			SortFields();
		}
	}
}


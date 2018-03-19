using System;
using System.Collections.Generic;
using QSDocTemplates;
using Vodovoz.Domain.Client;
using Vodovoz.Repositories.Client;

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
			AddField(x => x.FullNumberText, PatternFieldType.FString);
			AddField(x => x.StartDate, PatternFieldType.FDate);
			AddField(x => x.IssueDate, PatternFieldType.FDate);
			AddField(x => x.DeliveryPoint.CompiledAddress, PatternFieldType.FString);

			AddTable(x => x.FixedPrices)
				.AddColumn(x => x.Nomenclature.OfficialName, PatternFieldType.FString)
				.AddColumn(x => x.Price, PatternFieldType.FString);

			SortFields();
		}

		public void AddPricesTable(List<WaterPriceNode> header, List<WaterPriceNode> priceNodes)
		{
			AddCustomTable("ЦеныНаВодуШапка", header)
				.AddColumn(x => x.Count, PatternFieldType.FString)
				.AddColumn(x => x.Water1, PatternFieldType.FString)
				.AddColumn(x => x.Water2, PatternFieldType.FString)
				.AddColumn(x => x.Water3, PatternFieldType.FString);

			AddCustomTable("ЦеныНаВоду", priceNodes)
				.AddColumn(x => x.Count, PatternFieldType.FString)
				.AddColumn(x => x.Water1, PatternFieldType.FString)
				.AddColumn(x => x.Water2, PatternFieldType.FString)
				.AddColumn(x => x.Water3, PatternFieldType.FString);

			SortFields();
		}

	}
}


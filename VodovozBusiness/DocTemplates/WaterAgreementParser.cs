using System;
using System.Collections.Generic;
using System.Linq;
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

			AddField(x => String.Join(", ",
									  x.DeliveryPoint != null && x.DeliveryPoint.Phones.Any()
									  ? x.DeliveryPoint.Phones.Select(p => p.ToString())
									  : x.Contract.Counterparty.Phones.Select(p => p.ToString())
									 ), "Телефоны", PatternFieldType.FString);

			SortFields();
		}

		public void AddPricesTable(List<WaterPriceNode> priceNodes)
		{
			customTablesList.Clear();

			foreach(var fPrice in RootObject.FixedPrices){
				foreach(var node in priceNodes){
					if(fPrice.Nomenclature.Id == node.Id1)
						node.Water1 = fPrice.Price.ToString();
					if(fPrice.Nomenclature.Id == node.Id2)
						node.Water2 = fPrice.Price.ToString();
					if(fPrice.Nomenclature.Id == node.Id3)
						node.Water3 = fPrice.Price.ToString();
					if(fPrice.Nomenclature.Id == node.Id4)
						node.Water4 = fPrice.Price.ToString();
				}
			}

			AddCustomTable("ЦеныНаВоду", priceNodes)
				.AddColumn(x => x.StringCount, PatternFieldType.FString)
				.AddColumn(x => x.Water1, PatternFieldType.FString)
				.AddColumn(x => x.Water2, PatternFieldType.FString)
				.AddColumn(x => x.Water3, PatternFieldType.FString)
				.AddColumn(x => x.Water4, PatternFieldType.FString);

			SortFields();
		}
	}
}


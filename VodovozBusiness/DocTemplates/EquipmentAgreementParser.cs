using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QSDocTemplates;
using Vodovoz.Domain.Client;

namespace Vodovoz.DocTemplates
{
	public class EquipmentAgreementParser : DocParserBase<SalesEquipmentAgreement>
	{
		public override void UpdateFields()
		{
			fieldsList.Clear();
			//Сам договор
			AddField(x => x.Contract.IssueDate, PatternFieldType.FDate);
			AddField(x => x.Contract.Counterparty.FullName, PatternFieldType.FString);
			AddField(x => x.Contract.Counterparty.SignatoryBaseOf, PatternFieldType.FString);
			AddField(x => x.Contract.Number, PatternFieldType.FString);
			AddField(x => x.Contract.Counterparty.SignatoryFIO, PatternFieldType.FString);
			AddField(x => x.Contract.Counterparty.SignatoryPost, PatternFieldType.FString);
			//Само соглашение
			AddField(x => x.FullNumberText, PatternFieldType.FString);
			AddField(x => x.StartDate, PatternFieldType.FDate);
			AddField(x => x.IssueDate, PatternFieldType.FDate);
			AddField(x => x.DeliveryPoint.CompiledAddress, PatternFieldType.FString);

			SortFields();
		}

		public void AddPricesTable(List<SalesEquipment> prices)
		{
			decimal sumPrice = 0m;
			int sumCount = 0;
			decimal sumSum = 0m;
			List<SalesEquipmentParserNode> result = new List<SalesEquipmentParserNode>();
			foreach (var item in prices)
			{
				SalesEquipmentParserNode node = new SalesEquipmentParserNode();
				node.Number = (prices.IndexOf(item) + 1).ToString();
				node.Name = item.Nomenclature.OfficialName;
				node.Price = item.Price.ToString();
				sumPrice += item.Price;
				node.Count = item.Count.ToString();
				sumCount += item.Count;
				node.Sum = (item.Price * item.Count).ToString();
				sumSum += item.Price * item.Count;
				result.Add(node);
			}

			SalesEquipmentParserNode summaryNode = new SalesEquipmentParserNode();
			summaryNode.Number = "";
			summaryNode.Name = "Итог";
			summaryNode.Price = sumPrice.ToString();
			summaryNode.Count = sumCount.ToString();
			summaryNode.Sum = sumSum.ToString();
			result.Add(summaryNode);

			AddCustomTable("Оборудование", result)
				.AddColumn(x => x.Number, PatternFieldType.FString)
				.AddColumn(x => x.Name, PatternFieldType.FString)
				.AddColumn(x => x.Price, PatternFieldType.FString)
				.AddColumn(x => x.Count, PatternFieldType.FString)
				.AddColumn(x => x.Sum, PatternFieldType.FString);

			SortFields();
		}

	}

	public class SalesEquipmentParserNode
	{
		[Display(Name = "Номер")]
		public string Number { get; set; }
		[Display(Name = "Название")]
		public string Name { get; set; }
		[Display(Name = "Количество")]
		public string Count { get; set; }
		[Display(Name = "Цена")]
		public string Price { get; set; }
		[Display(Name = "Сумма")]
		public string Sum { get; set; }
	}
}

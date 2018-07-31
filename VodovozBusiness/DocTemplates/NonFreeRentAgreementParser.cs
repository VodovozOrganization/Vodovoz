using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QSDocTemplates;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;

namespace Vodovoz.DocTemplates
{
	public class NonFreeRentAgreementParser : DocParserBase<NonfreeRentAgreement>
	{
		public NonFreeRentAgreementParser()
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
			AddField(x => x.DeliveryPoint.CompiledAddressWOAddition, PatternFieldType.FString);

			AddField(x => String.Join(", ",
									  x.DeliveryPoint != null && x.DeliveryPoint.Phones.Any() 
			                          ? x.DeliveryPoint.Phones.Select(p => p.ToString()) 
			                          : x.Contract.Counterparty.Phones.Select(p => p.ToString())
			                         ), "Телефоны", PatternFieldType.FString);

			SortFields();
		}

		public void AddTableNomenclatures(List<PaidRentEquipment> list)
		{
			List<Nomenclature> result = new List<Nomenclature>();
			foreach(var item in list) {
				for(int i = 0; i < item.Count; i++) {
					result.Add(item.Nomenclature);
				}
			}
			AddCustomTable<Nomenclature>("СписокОборудования", result)
				.AddColumn(x => x.OfficialName, PatternFieldType.FString);
			
			SortFields();
		}

		public void AddTableEquipmentTypes(List<PaidRentEquipment> list)
		{
			var ids = list.Select(x => x.PaidRentPackage.Id).Distinct().ToArray();
			var equipList = list.Where(x => ids.Contains(x.PaidRentPackage.Id)).ToList();

			List<NonFreeRentParserNode> result = new List<NonFreeRentParserNode>();
			foreach (var item in equipList)
			{
				var node = new NonFreeRentParserNode();
				node.EquipmentType = item.PaidRentPackage.Name;
				node.Deposit = item.Deposit.ToString();
				node.Price = item.Price.ToString();
				decimal advance = item.Price * 2;
				node.Advance = advance.ToString();
				node.FirstPayment = (advance + item.Deposit).ToString();
				result.Add(node);
			}

			AddCustomTable<NonFreeRentParserNode>("ТипыОборудования", result)
				.AddColumn(x => x.EquipmentType, PatternFieldType.FString)
				.AddColumn(x => x.Deposit, PatternFieldType.FString)
				.AddColumn(x => x.Price, PatternFieldType.FString)
				.AddColumn(x => x.Advance, PatternFieldType.FString)
				.AddColumn(x => x.FirstPayment, PatternFieldType.FString);

			SortFields();
		}
	}

	public class NonFreeRentParserNode
	{
		[Display(Name = "Название")]
		public string EquipmentType { get; set; }
		[Display(Name = "Залог")]
		public string Deposit { get; set; }
		[Display(Name = "Цена")]
		public string Price { get; set; }
		[Display(Name = "Аванс")]
		public string Advance { get; set; }
		[Display(Name = "Платеж")]
		public string FirstPayment { get; set; }
	}


}


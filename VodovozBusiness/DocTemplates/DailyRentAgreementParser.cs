using System;
using System.Collections.Generic;
using System.Linq;
using QSDocTemplates;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;

namespace Vodovoz.DocTemplates
{
	public class DailyRentAgreementParser : DocParserBase<DailyRentAgreement>
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
			AddCustomTable<PaidRentEquipment>("ТипыОборудования", list)
				.AddColumn(x => x.Nomenclature.OfficialName, PatternFieldType.FString)
				.AddColumn(x => x.Deposit, PatternFieldType.FString)
				.AddColumn(x => x.Price, PatternFieldType.FString)
				.AddColumn(x => x.Nomenclature.SumOfDamage, PatternFieldType.FString);

			SortFields();
		}
	}
}


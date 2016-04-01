using System;
using Vodovoz.Domain;
using System.Collections.Generic;

namespace Vodovoz.ExportTo1c.Catalogs
{
	public class CounterpartyCatalog:GenericCatalog<Counterparty>
	{
		public CounterpartyCatalog(ExportData exportData)
			:base(exportData)
		{			
		}
		protected override string Name
		{
			get{return "Контрагенты";}
		}
			
		public override ReferenceNode GetReferenceTo(Counterparty counterparty)
		{
			int id = GetReferenceId(counterparty);
			var code1c = String.IsNullOrWhiteSpace(counterparty.Code1c) 
				? counterparty.Id.ToString() : counterparty.Code1c;
			return new ReferenceNode(id,
				new PropertyNode("Код",
					Common1cTypes.String,
					code1c
				),
				new PropertyNode("ЭтоГруппа",
					Common1cTypes.Boolean)
			);
		}

		protected override PropertyNode[] GetProperties(Counterparty counterparty)
		{
			var properties = new List<PropertyNode>();
			properties.Add(
				new PropertyNode("ПометкаУдаления",
					Common1cTypes.Boolean
				)
			);
			properties.Add(
				new PropertyNode("Наименование",
					Common1cTypes.String,
					counterparty.Name
				)
			);
			properties.Add(
				new PropertyNode("Родитель",
					Common1cTypes.ReferenceContract			
				)
			);
			var counterpartyType = counterparty.PersonType == PersonType.legal ? "ЮрЛицо" : "ФизЛицо";
			properties.Add(
				new PropertyNode("ЮрФизЛицо",
					Common1cTypes.String,
					counterpartyType
				)
			);
			var account = counterparty.DefaultAccount;
			if (account == null)
				properties.Add(
					new PropertyNode("ОсновнойБанковскийСчет",
						Common1cTypes.ReferenceAccount
					)
				);
			else
				properties.Add(
					new PropertyNode("ОсновнойБанковскийСчет",
						Common1cTypes.ReferenceAccount,
						exportData.AccountDirectory.GetReferenceTo(counterparty.DefaultAccount,counterparty)
					)
				);
			properties.Add(
				new PropertyNode("Комментарий",
					Common1cTypes.String,
					counterparty.Comment
				)
			);
			properties.Add(
				new PropertyNode("НаименованиеПолное",
					Common1cTypes.String,
					counterparty.FullName
				)
			);
			properties.Add(
				new PropertyNode("ИНН",
					Common1cTypes.String,
					counterparty.INN
				)
			);
			properties.Add(
				new PropertyNode("КПП",
					Common1cTypes.String,
					counterparty.KPP
				)
			);
			properties.Add(
				new PropertyNode("ГоловнойКонтрагент",
					Common1cTypes.ReferenceContract
				)
			);
			return properties.ToArray();
		}
	}
}


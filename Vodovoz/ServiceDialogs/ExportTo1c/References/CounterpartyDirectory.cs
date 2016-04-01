using System;
using Vodovoz.Domain;
using System.Collections.Generic;

namespace Vodovoz.ExportTo1c.References
{
	public class CounterpartyDirectory:GenericDirectory<Counterparty>
	{
		public CounterpartyDirectory(ExportData exportData)
			:base(exportData)
		{			
		}
		protected override string Name
		{
			get{return "Контрагенты";}
		}
			
		public override ExportReferenceNode GetReferenceTo(Counterparty counterparty)
		{
			int id = GetReferenceId(counterparty);
			var code1c = String.IsNullOrWhiteSpace(counterparty.Code1c) 
				? Exports.VodovozTo1cID(counterparty.Id) : counterparty.Code1c;
			return new ExportReferenceNode(id,
				new ExportPropertyNode("Код",
					Common1cTypes.String,
					code1c
				),
				new ExportPropertyNode("ЭтоГруппа",
					Common1cTypes.Boolean)
			);
		}

		protected override ExportPropertyNode[] GetProperties(Counterparty counterparty)
		{
			var properties = new List<ExportPropertyNode>();
			properties.Add(
				new ExportPropertyNode("ПометкаУдаления",
					Common1cTypes.Boolean
				)
			);
			properties.Add(
				new ExportPropertyNode("Наименование",
					Common1cTypes.String,
					counterparty.Name
				)
			);
			properties.Add(
				new ExportPropertyNode("Родитель",
					Common1cTypes.ReferenceContract			
				)
			);
			var counterpartyType = counterparty.PersonType == PersonType.legal ? "ЮрЛицо" : "ФизЛицо";
			properties.Add(
				new ExportPropertyNode("ЮрФизЛицо",
					Common1cTypes.String,
					counterpartyType
				)
			);
			var account = counterparty.DefaultAccount;
			if (account == null)
				properties.Add(
					new ExportPropertyNode("ОсновнойБанковскийСчет",
						Common1cTypes.ReferenceAccount
					)
				);
			else
				properties.Add(
					new ExportPropertyNode("ОсновнойБанковскийСчет",
						Common1cTypes.ReferenceAccount,
						exportData.AccountDirectory.GetReferenceTo(counterparty.DefaultAccount,counterparty)
					)
				);
			properties.Add(
				new ExportPropertyNode("Комментарий",
					Common1cTypes.String,
					counterparty.Comment
				)
			);
			properties.Add(
				new ExportPropertyNode("НаименованиеПолное",
					Common1cTypes.String,
					counterparty.FullName
				)
			);
			properties.Add(
				new ExportPropertyNode("ИНН",
					Common1cTypes.String,
					counterparty.INN
				)
			);
			properties.Add(
				new ExportPropertyNode("КПП",
					Common1cTypes.String,
					counterparty.KPP
				)
			);
			properties.Add(
				new ExportPropertyNode("ГоловнойКонтрагент",
					Common1cTypes.ReferenceContract
				)
			);
			return properties.ToArray();
		}
	}
}


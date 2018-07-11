using System;
using System.Collections.Generic;
using Vodovoz.Domain.Client;

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
			
		public override ReferenceNode CreateReferenceTo(Counterparty counterparty)
		{
			if(String.IsNullOrWhiteSpace(counterparty.INN))
				exportData.Errors.Add($"Для контрагента {counterparty.Id} - '{counterparty.Name}' не заполнен ИНН.");

			int id = GetReferenceId(counterparty);
			return new ReferenceNode(id,
				new PropertyNode("ИНН",
					Common1cTypes.String,
			                     counterparty.INN)
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
				new PropertyNode("Код",
					Common1cTypes.String,
				                 counterparty.Code1c
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
						exportData.AccountCatalog.CreateReferenceTo(counterparty.DefaultAccount,counterparty)
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


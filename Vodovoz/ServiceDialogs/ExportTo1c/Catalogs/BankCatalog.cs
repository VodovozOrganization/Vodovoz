using System;
using QSBanks;
using System.Collections.Generic;

namespace Vodovoz.ExportTo1c.Catalogs
{
	public class BankCatalog:GenericCatalog<Bank>
	{
		public BankCatalog(ExportData exportData)
			:base(exportData)
		{			
		}
		protected override string Name
		{
			get{return "Банки";}
		}
		public override ReferenceNode CreateReferenceTo(Bank bank)
		{
			int id = GetReferenceId(bank);
			return new ReferenceNode(id,
				new PropertyNode("Код",
					Common1cTypes.String,
					bank.Bik
				),
				new PropertyNode("ЭтоГруппа",
					Common1cTypes.ReferenceCounterparty
				)
			);
		}
		protected override PropertyNode[] GetProperties(Bank bank)
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
					bank.Name
				)
			);
			properties.Add(
				new PropertyNode("Родитель",
					Common1cTypes.ReferenceBank
				)
			);
			properties.Add(
				new PropertyNode("Город",
					Common1cTypes.String,
					bank.City
				)
			);
			properties.Add(
				new PropertyNode("КоррСчет",
					Common1cTypes.String,
					bank.CorAccount
				)
			);
			properties.Add(
				new PropertyNode("Адрес",
					Common1cTypes.String,
					bank.RegionText
				)
			);
			properties.Add(
				new PropertyNode("Телефоны",
					Common1cTypes.String
				)
			);
			return properties.ToArray();
		}
	}
}


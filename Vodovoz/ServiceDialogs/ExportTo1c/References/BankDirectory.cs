using System;
using QSBanks;
using System.Collections.Generic;

namespace Vodovoz.ExportTo1c.References
{
	public class BankDirectory:GenericDirectory<Bank>
	{
		public BankDirectory(ExportData exportData)
			:base(exportData)
		{			
		}
		protected override string Name
		{
			get{return "Банки";}
		}
		public override ExportReferenceNode GetReferenceTo(Bank bank)
		{
			int id = GetReferenceId(bank);
			return new ExportReferenceNode(id,
				new ExportPropertyNode("Код",
					Common1cTypes.String,
					bank.Bik
				),
				new ExportPropertyNode("ЭтоГруппа",
					Common1cTypes.ReferenceCounterparty
				)
			);
		}
		protected override ExportPropertyNode[] GetProperties(Bank bank)
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
					bank.Name
				)
			);
			properties.Add(
				new ExportPropertyNode("Родитель",
					Common1cTypes.ReferenceBank
				)
			);
			properties.Add(
				new ExportPropertyNode("Город",
					Common1cTypes.String,
					bank.City
				)
			);
			properties.Add(
				new ExportPropertyNode("КоррСчет",
					Common1cTypes.String,
					bank.CorAccount
				)
			);
			properties.Add(
				new ExportPropertyNode("Адрес",
					Common1cTypes.String,
					bank.GetRegionString
				)
			);
			properties.Add(
				new ExportPropertyNode("Телефоны",
					Common1cTypes.String
				)
			);
			return properties.ToArray();
		}
	}
}


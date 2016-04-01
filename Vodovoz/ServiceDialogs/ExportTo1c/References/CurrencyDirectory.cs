using System;
using System.Collections.Generic;

namespace Vodovoz.ExportTo1c.References
{
	public class CurrencyDirectory:GenericDirectory<Currency>
	{
		public CurrencyDirectory(ExportData exportData)
			:base(exportData)
		{			
		}
		protected override string Name
		{
			get{return "Валюты";}
		}
		public override ExportReferenceNode GetReferenceTo(Currency currency)
		{
			int id = GetReferenceId(currency);
			return new ExportReferenceNode(id,
				new ExportPropertyNode("Код",
					"Строка",
					currency.ExportId
				)
			);
		}
		protected override ExportPropertyNode[] GetProperties(Currency currency)
		{
			var properties = new List<ExportPropertyNode>();
			properties.Add(
				new ExportPropertyNode("Наименование",
					Common1cTypes.String,
					currency.Name
				)
			);
			properties.Add(
				new ExportPropertyNode("НаименованиеПолное",
					Common1cTypes.String,
					currency.FullName
				)
			);
			return properties.ToArray();
		}
	}
}


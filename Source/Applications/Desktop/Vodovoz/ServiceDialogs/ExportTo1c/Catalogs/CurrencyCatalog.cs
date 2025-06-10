﻿using System;
using System.Collections.Generic;
using Vodovoz.ServiceDialogs.ExportTo1c;

namespace Vodovoz.ExportTo1c.Catalogs
{
	public class CurrencyCatalog:GenericCatalog<Currency>
	{
		public CurrencyCatalog(ExportData exportData)
			:base(exportData)
		{			
		}
		protected override string Name
		{
			get{return "Валюты";}
		}
		public override ReferenceNode CreateReferenceTo(Currency currency)
		{
			int id = GetReferenceId(currency);
			return new ReferenceNode(id,
				new PropertyNode("Код",
					"Строка",
					currency.ExportId
				)
			);
		}
		protected override PropertyNode[] GetProperties(Currency currency)
		{
			var properties = new List<PropertyNode>();
			properties.Add(
				new PropertyNode("Наименование",
					Common1cTypes.String,
					currency.Name
				)
			);
			properties.Add(
				new PropertyNode("НаименованиеПолное",
					Common1cTypes.String,
					currency.FullName
				)
			);
			return properties.ToArray();
		}
	}
}


using System;
using System.Collections.Generic;

namespace Vodovoz.ExportTo1c.Catalogs
{
	public class WarehouseCatalog:GenericCatalog<Warehouse1c>
	{
		public WarehouseCatalog(ExportData exportData)
			:base(exportData)
		{			
		}

		protected override string Name
		{
			get{return "Склады";}
		}
		public override ReferenceNode GetReferenceTo(Warehouse1c warehouse)
		{
			int id = GetReferenceId(warehouse);
			return new ReferenceNode(id,
				new PropertyNode("Код",
					Common1cTypes.String,
					warehouse.ExportId
				),
				new PropertyNode("ЭтоГруппа",
					Common1cTypes.Boolean
				)
			);
		}
		protected override PropertyNode[] GetProperties(Warehouse1c warehouse)
		{
			var properties = new List<PropertyNode>();
			var warehouseTypeProperty = new PropertyNode("ВидСклада",
				Common1cTypes.EnumWarehouseTypes,
				warehouse.Type
			);
			warehouseTypeProperty.AdditionalAttributes.Add(new System.Xml.Linq.XAttribute("НеЗамещать", "true"));
			properties.Add(warehouseTypeProperty);
			properties.Add(
				new PropertyNode("Наименование",
					Common1cTypes.String,
					warehouse.Name
				)
			);
			return properties.ToArray();
		}
	}
}


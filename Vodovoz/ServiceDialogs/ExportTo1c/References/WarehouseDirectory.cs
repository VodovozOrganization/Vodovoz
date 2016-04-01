using System;
using System.Collections.Generic;

namespace Vodovoz.ExportTo1c.References
{
	public class WarehouseDirectory:GenericDirectory<Warehouse1c>
	{
		public WarehouseDirectory(ExportData exportData)
			:base(exportData)
		{			
		}

		protected override string Name
		{
			get{return "Склады";}
		}
		public override ExportReferenceNode GetReferenceTo(Warehouse1c warehouse)
		{
			int id = GetReferenceId(warehouse);
			return new ExportReferenceNode(id,
				new ExportPropertyNode("Код",
					Common1cTypes.String,
					warehouse.ExportId
				),
				new ExportPropertyNode("ЭтоГруппа",
					Common1cTypes.Boolean
				)
			);
		}
		protected override ExportPropertyNode[] GetProperties(Warehouse1c warehouse)
		{
			var properties = new List<ExportPropertyNode>();
			var warehouseTypeProperty = new ExportPropertyNode("ВидСклада",
				Common1cTypes.EnumWarehouseTypes,
				warehouse.Type
			);
			warehouseTypeProperty.AdditionalAttributes.Add(new System.Xml.Linq.XAttribute("НеЗамещать", "true"));
			properties.Add(warehouseTypeProperty);
			properties.Add(
				new ExportPropertyNode("Наименование",
					Common1cTypes.String,
					warehouse.Name
				)
			);
			return properties.ToArray();
		}
	}
}


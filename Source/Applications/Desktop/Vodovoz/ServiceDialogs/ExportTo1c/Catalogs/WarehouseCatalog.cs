using System;
using System.Collections.Generic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.ServiceDialogs.ExportTo1c;

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
		public override ReferenceNode CreateReferenceTo(Warehouse1c warehouse)
		{
			int id = GetReferenceId(warehouse);

			var referenceNode = new ReferenceNode(id, new PropertyNode("ЭтоГруппа", Common1cTypes.Boolean));

			if(exportData.ExportMode != Export1cMode.ComplexAutomation)
			{
				referenceNode.Properties.Add(new PropertyNode("Код", Common1cTypes.String, warehouse.ExportId));
			}

			return referenceNode;
		}
		protected override PropertyNode[] GetProperties(Warehouse1c warehouse)
		{
			var properties = new List<PropertyNode>();
			var warehouseTypeProperty = new PropertyNode("ТипСклада",
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


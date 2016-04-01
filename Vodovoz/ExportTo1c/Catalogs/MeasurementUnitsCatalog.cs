using System;
using QSBusinessCommon.Domain;
using System.Collections.Generic;

namespace Vodovoz.ExportTo1c.Catalogs
{
	public class MeasurementUnitsCatalog:GenericCatalog<MeasurementUnits>
	{
		public MeasurementUnitsCatalog(ExportData exportData)
			:base(exportData)
		{			
		}

		protected override string Name
		{
			get{return "КлассификаторЕдиницИзмерения";}
		}
		public override ReferenceNode CreateReferenceTo(MeasurementUnits unit)
		{			
			int id = GetReferenceId(unit);
			return new ReferenceNode(id,
				new PropertyNode("Код",
					Common1cTypes.String,
					unit.Id
				)
			);
		}
		protected override PropertyNode[] GetProperties(MeasurementUnits unit)
		{
			var properties = new List<PropertyNode>();
			properties.Add(
				new PropertyNode("Наименование",
					Common1cTypes.String,
					unit.Name
				)
			);
			properties.Add(
				new PropertyNode("НаименованиеПолное",
					Common1cTypes.String,
					unit.Name
				)
			);
			return properties.ToArray();
		}
	}
}


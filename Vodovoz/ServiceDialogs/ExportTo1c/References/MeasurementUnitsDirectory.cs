using System;
using QSBusinessCommon.Domain;
using System.Collections.Generic;

namespace Vodovoz.ExportTo1c.References
{
	public class MeasurementUnitsDirectory:GenericDirectory<MeasurementUnits>
	{
		public MeasurementUnitsDirectory(ExportData exportData)
			:base(exportData)
		{			
		}

		protected override string Name
		{
			get{return "КлассификаторЕдиницИзмерения";}
		}
		public override ExportReferenceNode GetReferenceTo(MeasurementUnits unit)
		{			
			int id = GetReferenceId(unit);
			return new ExportReferenceNode(id,
				new ExportPropertyNode("Код",
					Common1cTypes.String,
					unit.Id
				)
			);
		}
		protected override ExportPropertyNode[] GetProperties(MeasurementUnits unit)
		{
			var properties = new List<ExportPropertyNode>();
			properties.Add(
				new ExportPropertyNode("Наименование",
					Common1cTypes.String,
					unit.Name
				)
			);
			properties.Add(
				new ExportPropertyNode("НаименованиеПолное",
					Common1cTypes.String,
					unit.Name
				)
			);
			return properties.ToArray();
		}
	}
}


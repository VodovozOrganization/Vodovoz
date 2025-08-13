using System.Collections.Generic;
using ExportTo1c.Library.ExportNodes;
using QS.BusinessCommon.Domain;
using Vodovoz.EntityRepositories.Orders;

namespace ExportTo1c.Library.Catalogs
{
	/// <summary>
	/// Единицы измерения
	/// </summary>
	public class MeasurementUnitsCatalog : GenericCatalog<MeasurementUnits>
	{
		public MeasurementUnitsCatalog(ExportData exportData)
			: base(exportData)
		{
		}

		protected override string Name => exportData.ExportMode == Export1cMode.ComplexAutomation
			? "УпаковкиЕдиницыИзмерения"
			: "КлассификаторЕдиницИзмерения";

		public override ReferenceNode CreateReferenceTo(MeasurementUnits unit)
		{
			int id = GetReferenceId(unit);

			var referenceNode = new ReferenceNode(id,
				new PropertyNode("Код",
					Common1cTypes.String,
					unit.Id
				)
			);

			if(exportData.ExportMode == Export1cMode.ComplexAutomation)
			{
				referenceNode.Properties.Add(
					new PropertyNode("Наименование",
						Common1cTypes.String,
						unit.Name
					)
				);
			}

			return referenceNode;
		}

		protected override PropertyNode[] GetProperties(MeasurementUnits unit)
		{
			var properties = new List<PropertyNode>();

			if(exportData.ExportMode != Export1cMode.ComplexAutomation)
			{
				properties.Add(
					new PropertyNode("Наименование",
						Common1cTypes.String,
						unit.Name
					)
				);
			}

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

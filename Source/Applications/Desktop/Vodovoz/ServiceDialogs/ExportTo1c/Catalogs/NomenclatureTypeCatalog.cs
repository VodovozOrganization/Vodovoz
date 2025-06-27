using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.ServiceDialogs.ExportTo1c;

namespace Vodovoz.ExportTo1c.Catalogs
{
	public class NomenclatureType1cTypeCatalog : GenericCatalog<NomenclatureType1c>
	{
		public NomenclatureType1cTypeCatalog(ExportData exportData)
			:base(exportData)
		{			
		}
		protected override string Name
		{
			get{return "ВидыНоменклатуры";}
		}

		public override ReferenceNode CreateReferenceTo(NomenclatureType1c NomenclatureType1c)
		{
			int id = GetReferenceId(NomenclatureType1c);

			return new ReferenceNode(id, new PropertyNode("Наименование", Common1cTypes.String, NomenclatureType1c.Name));
		}

		protected override PropertyNode[] GetProperties(NomenclatureType1c nomenclatureType1c)
		{
			var properties = new List<PropertyNode>();
			properties.Add(
				new PropertyNode("Наименование",
					Common1cTypes.String,
					nomenclatureType1c.Name
				)
			);

			if(exportData.ExportMode == Export1cMode.ComplexAutomation)
			{
				properties.Add(
					new PropertyNode("ТипНоменклатуры",
						Common1cTypes.EnumNomenclatureTypes,
						nomenclatureType1c.Name
					)
				);
			}

			if(exportData.ExportMode != Export1cMode.ComplexAutomation)
			{
				properties.Add(
					new PropertyNode("Услуга",
						Common1cTypes.Boolean,
						nomenclatureType1c.IsService
					)
				);
			}

			return properties.ToArray();
		}			
	}
}


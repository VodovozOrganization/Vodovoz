using System;
using System.Collections.Generic;
using Vodovoz.Domain.Goods;

namespace Vodovoz.ExportTo1c.Catalogs
{
	public class NomenclatureGroupCatalog:GenericCatalog<Folder1c>
	{
		public NomenclatureGroupCatalog(ExportData exportData)
			:base(exportData)
		{			
		}
		protected override string Name
		{
			get{return "Номенклатура";}
		}

		public override ReferenceNode CreateReferenceTo(Folder1c folder)
		{
			int id = GetReferenceId(folder);

			return new ReferenceNode(id,
				new PropertyNode("Код",
					Common1cTypes.String,
					folder.Code1c
				),
				new PropertyNode("ЭтоГруппа",
					Common1cTypes.Boolean,
					"true"
				)
			);
		}

		protected override PropertyNode[] GetProperties(Folder1c folder)
		{
			var properties = new List<PropertyNode>();

			properties.Add(
				new PropertyNode("Наименование",
					Common1cTypes.String,
					folder.Name
				)
			);
			properties.Add(
				new PropertyNode("Родитель",
					Common1cTypes.ReferenceNomenclature
				)
			);
			properties.Add(
				new PropertyNode("БазоваяЕдиницаИзмерения",
					Common1cTypes.ReferenceMeasurementUnit
				)
			);
			properties.Add(
				new PropertyNode("Комментарий",
					Common1cTypes.String
				)
			);
			properties.Add(
				new PropertyNode("НомерГТД",
					"СправочникСсылка.НомераГТД"
				)
			);
			properties.Add(
				new PropertyNode("СтавкаНДС",
					Common1cTypes.EnumVAT
				)
			);
			properties.Add(
				new PropertyNode("СтранаПроисхождения",
					Common1cTypes.ReferenceCountry
				)
			);
			properties.Add(
				new PropertyNode("ПометкаУдаления",
					Common1cTypes.Boolean
				)
			);
			properties.Add(
				new PropertyNode("Услуга",
					Common1cTypes.Boolean
				)
			);
			properties.Add(
				new PropertyNode("НаименованиеПолное",
					Common1cTypes.String,
					folder.Name
				)
			);
			return properties.ToArray();
		}			
	}
}


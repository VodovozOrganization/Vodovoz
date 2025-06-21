using System;
using System.Collections.Generic;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.ServiceDialogs.ExportTo1c;

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

			var referenceNode = new ReferenceNode(id,
				new PropertyNode("Код",
					Common1cTypes.String,
					folder.Code1c
				)
			);
			
			referenceNode.Properties.Add(new PropertyNode("ЭтоГруппа", Common1cTypes.Boolean, "true"));
			
			return referenceNode;
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

			if(folder.Parent != null)
			{
				properties.Add(
					new PropertyNode("Родитель",
						Common1cTypes.ReferenceNomenclature,
					                 exportData.NomenclatureGroupCatalog.CreateReferenceTo(folder.Parent)
					)
				);
			}
			else
			{
				properties.Add(
					new PropertyNode("Родитель",
						Common1cTypes.ReferenceNomenclature
					)
				);
			}
			
			return properties.ToArray();
		}			
	}
}


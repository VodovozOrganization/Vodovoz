using System;
using Vodovoz.Domain;
using System.Collections.Generic;

namespace Vodovoz.ExportTo1c.Catalogs
{
	public class NomenclatureCatalog:GenericCatalog<Nomenclature>
	{
		public NomenclatureCatalog(ExportData exportData)
			:base(exportData)
		{			
		}
		protected override string Name
		{
			get{return "Номенклатура";}
		}

		public override ReferenceNode CreateReferenceTo(Nomenclature nomenclature)
		{
			int id = GetReferenceId(nomenclature);
			//Если id<=0 то этот объект загружался не из базы -> выгружаем как группу для 1с
			bool isGroup = nomenclature.Id <= 0;
			if (isGroup)
			{
				return new ReferenceNode(id,
					new PropertyNode("Код",
						Common1cTypes.String,
						nomenclature.Code1c
					),
					new PropertyNode("ЭтоГруппа",
						Common1cTypes.Boolean,
						"true"
					)
				);
			}
			else
			{
				var code1c = String.IsNullOrWhiteSpace(nomenclature.Code1c) ? nomenclature.Id.ToString() : nomenclature.Code1c;
				return new ReferenceNode(id,
					new PropertyNode("Код",
						Common1cTypes.String,
						code1c
					),
					new PropertyNode("ЭтоГруппа",
						Common1cTypes.Boolean
					)
				);
			}
		}

		protected override PropertyNode[] GetProperties(Nomenclature nomenclature)
		{
			var properties = new List<PropertyNode>();
			properties.Add(
				new PropertyNode("Наименование",
					Common1cTypes.String,
					nomenclature.Name
				)
			);
			Nomenclature parentNomenclature;
			//Если id<=0 то этот объект загружался не из базы -> выгружаем как группу для 1с
			bool isGroup = nomenclature.Id <=0;
			if (exportData.CategoryToNomenclatureMap.TryGetValue(nomenclature.Category, out parentNomenclature) && !isGroup)
			{
				properties.Add(
					new PropertyNode("Родитель",
						Common1cTypes.ReferenceNomenclature,	
						exportData.NomenclatureCatalog.CreateReferenceTo(parentNomenclature)
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
			if (nomenclature.Unit != null)
			{
				properties.Add(
					new PropertyNode("БазоваяЕдиницаИзмерения",
						Common1cTypes.ReferenceMeasurementUnit,
						exportData.MeasurementUnitCatalog.CreateReferenceTo(nomenclature.Unit)
					)
				);
			}
			else
			{
				properties.Add(
					new PropertyNode("БазоваяЕдиницаИзмерения",
						Common1cTypes.ReferenceMeasurementUnit
					)
				);
			}
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
			if (!isGroup)
			{
				var vat = nomenclature.VAT.GetAttribute<Value1c>().Value;
				properties.Add(
					new PropertyNode("СтавкаНДС",
						Common1cTypes.EnumVAT,
						vat
					)
				);
			}
			else
			{
				var vat = nomenclature.VAT.GetAttribute<Value1c>().Value;
				properties.Add(
					new PropertyNode("СтавкаНДС",
						Common1cTypes.EnumVAT
					)
				);
			}
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
			var isService = nomenclature.Category == NomenclatureCategory.service;
			if (isService)
				properties.Add(
					new PropertyNode("Услуга",
						Common1cTypes.Boolean,
						"true"
					)
				);
			else
				properties.Add(
					new PropertyNode("Услуга",
						Common1cTypes.Boolean
					)
				);
			properties.Add(
				new PropertyNode("НаименованиеПолное",
					Common1cTypes.String,
					nomenclature.Name
				)
			);
			return properties.ToArray();
		}			
	}
}


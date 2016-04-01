using System;
using Vodovoz.Domain;
using System.Collections.Generic;

namespace Vodovoz.ExportTo1c.References
{
	public class NomenclatureDirectory:GenericDirectory<Nomenclature>
	{
		public NomenclatureDirectory(ExportData exportData)
			:base(exportData)
		{			
		}
		protected override string Name
		{
			get{return "Номенклатура";}
		}

		public override ExportReferenceNode GetReferenceTo(Nomenclature nomenclature)
		{
			int id = GetReferenceId(nomenclature);
			//Если id<=0 то этот объект загружался не из базы -> выгружаем как папку для 1с
			bool isGroup = nomenclature.Id <= 0;
			if (isGroup)
			{
				return new ExportReferenceNode(id,
					new ExportPropertyNode("Код",
						Common1cTypes.String,
						nomenclature.Code1c
					),
					new ExportPropertyNode("ЭтоГруппа",
						Common1cTypes.Boolean,
						"true"
					)
				);
			}
			else
			{
				var code1c = String.IsNullOrWhiteSpace(nomenclature.Code1c) ? nomenclature.Id.ToString() : nomenclature.Code1c;
				return new ExportReferenceNode(id,
					new ExportPropertyNode("Код",
						Common1cTypes.String,
						code1c
					),
					new ExportPropertyNode("ЭтоГруппа",
						Common1cTypes.Boolean
					)
				);
			}
		}

		protected override ExportPropertyNode[] GetProperties(Nomenclature nomenclature)
		{
			var properties = new List<ExportPropertyNode>();
			properties.Add(
				new ExportPropertyNode("Наименование",
					Common1cTypes.String,
					nomenclature.Name
				)
			);
			Nomenclature parentNomenclature;
			//Если id<=0 то этот объект загружался не из базы -> выгружаем как папку для 1с
			bool isGroup = nomenclature.Id <=0;
			if (exportData.CategoryToNomenclatureMap.TryGetValue(nomenclature.Category, out parentNomenclature) && !isGroup)
			{
				properties.Add(
					new ExportPropertyNode("Родитель",
						Common1cTypes.ReferenceNomenclature,	
						exportData.NomenclatureDirectory.GetReferenceTo(parentNomenclature)
					)
				);
			}
			else
			{
				properties.Add(
					new ExportPropertyNode("Родитель",
						Common1cTypes.ReferenceNomenclature
					)
				);
			}
			if (nomenclature.Unit != null)
			{
				properties.Add(
					new ExportPropertyNode("БазоваяЕдиницаИзмерения",
						Common1cTypes.ReferenceMeasurementUnit,
						exportData.MeasurementUnitsDirectory.GetReferenceTo(nomenclature.Unit)
					)
				);
			}
			else
			{
				properties.Add(
					new ExportPropertyNode("БазоваяЕдиницаИзмерения",
						Common1cTypes.ReferenceMeasurementUnit
					)
				);
			}
			properties.Add(
				new ExportPropertyNode("Комментарий",
					Common1cTypes.String
				)
			);
			properties.Add(
				new ExportPropertyNode("НомерГТД",
					"СправочникСсылка.НомераГТД"
				)
			);
			if (!isGroup)
			{
				var vat = nomenclature.VAT.GetAttribute<Value1c>().Value;
				properties.Add(
					new ExportPropertyNode("СтавкаНДС",
						"ПеречислениеСсылка.СтавкиНДС",
						vat
					)
				);
			}
			else
			{
				var vat = nomenclature.VAT.GetAttribute<Value1c>().Value;
				properties.Add(
					new ExportPropertyNode("СтавкаНДС",
						"ПеречислениеСсылка.СтавкиНДС"
					)
				);
			}
			properties.Add(
				new ExportPropertyNode("СтранаПроисхождения",
					Common1cTypes.ReferenceCountry
				)
			);
			properties.Add(
				new ExportPropertyNode("ПометкаУдаления",
					Common1cTypes.Boolean
				)
			);
			properties.Add(
				new ExportPropertyNode("Услуга",
					Common1cTypes.Boolean
				)
			);
			properties.Add(
				new ExportPropertyNode("НаименованиеПолное",
					Common1cTypes.String
				)
			);
			return properties.ToArray();
		}			
	}
}


using System.Collections.Generic;
using System.Linq;
using ExportTo1c.Library.ExportDefaults;
using ExportTo1c.Library.ExportNodes;
using Gamma.Utilities;
using Vodovoz.Core.Domain.Attributes;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Orders;

namespace ExportTo1c.Library.Catalogs
{
	/// <summary>
	/// Номенклатура
	/// </summary>
	public class NomenclatureCatalog : GenericCatalog<Nomenclature>
	{
		public NomenclatureCatalog(ExportData exportData)
			: base(exportData)
		{
		}

		protected override string Name
		{
			get { return "Номенклатура"; }
		}

		public override ReferenceNode CreateReferenceTo(Nomenclature nomenclature)
		{
			int id = GetReferenceId(nomenclature);

			if(string.IsNullOrWhiteSpace(nomenclature.Code1c))
				exportData.Errors.Add($"Для номенклатуры {nomenclature.Id} - '{nomenclature.Name}' не заполнен код 1с.");

			var referenceNode = new ReferenceNode(id,
				new PropertyNode("Код",
					Common1cTypes.String,
					nomenclature.Code1c
				));

			referenceNode.Properties.Add(new PropertyNode("ЭтоГруппа", Common1cTypes.Boolean));

			return referenceNode;
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

			if(nomenclature.Folder1C == null)
				exportData.Errors.Add($"Для номенклатуры {nomenclature.Id} - '{nomenclature.Name}' не заполнена папка 1с.");
			else
			{
				properties.Add(
					new PropertyNode("Родитель",
						Common1cTypes.ReferenceNomenclature,
						exportData.NomenclatureGroupCatalog.CreateReferenceTo(nomenclature.Folder1C)
					)
				);
			}

			if(nomenclature.Unit != null)
			{
				properties.Add(
					new PropertyNode("ЕдиницаИзмерения",
						Common1cTypes.ReferenceMeasurementUnit(exportData.ExportMode),
						exportData.MeasurementUnitCatalog.CreateReferenceTo(nomenclature.Unit)
					)
				);
			}
			else
			{
				properties.Add(
					new PropertyNode("ЕдиницаИзмерения",
						Common1cTypes.ReferenceMeasurementUnit(exportData.ExportMode)
					)
				);
			}

			properties.Add(
				new PropertyNode("Описание",
					Common1cTypes.String
				)
			);

			if(exportData.ExportMode != Export1cMode.ComplexAutomation)
			{
				properties.Add(new PropertyNode("НомерГТД", "СправочникСсылка.НомераГТД"));
			}

			if(exportData.ExportMode == Export1cMode.ComplexAutomation)
			{
				var vatCatalog = new VatCatalog(exportData)
				{
					Vat = nomenclature.VAT
				};

				var vatReference = vatCatalog.CreateReferenceTo(vatCatalog);

				properties.Add(
					new PropertyNode("СтавкаНДС",
						Common1cTypes.ReferenceVat,
						vatReference
					)
				);
			}
			else
			{
				var vat = nomenclature.VAT.GetAttribute<Value1cType>().Value;

				properties.Add(
					new PropertyNode("ВидСтавкиНДС",
						Common1cTypes.EnumVATTypes,
						vat
					)
				);
			}

			var isService = !Nomenclature.GetCategoriesForGoods().Contains(nomenclature.Category);

			if(isService)
			{
				if(exportData.ExportMode != Export1cMode.ComplexAutomation)
				{
					properties.Add(new PropertyNode("Услуга", Common1cTypes.Boolean, "true"));
				}
			}
			else
			{
				if(exportData.ExportMode != Export1cMode.ComplexAutomation)
				{
					properties.Add(new PropertyNode("Услуга", Common1cTypes.Boolean));
				}
			}

			if(isService)
				properties.Add(
					new PropertyNode("ВидНоменклатуры",
						Common1cTypes.ReferenceNomenclatureType,
						exportData.NomenclatureTypeCatalog.CreateReferenceTo(NomenclatureType1c.ServicesType(exportData.ExportMode))
					)
				);
			else
				properties.Add(
					new PropertyNode("ВидНоменклатуры",
						Common1cTypes.ReferenceNomenclatureType,
						exportData.NomenclatureTypeCatalog.CreateReferenceTo(NomenclatureType1c.GoodsType(exportData.ExportMode))
					)
				);

			properties.Add(
				new PropertyNode("НаименованиеПолное",
					Common1cTypes.String,
					nomenclature.OfficialName
				)
			);
			return properties.ToArray();
		}
	}
}

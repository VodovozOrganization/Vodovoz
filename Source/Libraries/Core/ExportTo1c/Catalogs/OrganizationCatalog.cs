using System.Collections.Generic;
using ExportTo1c.Library.ExportNodes;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Orders;

namespace ExportTo1c.Library.Catalogs
{
	/// <summary>
	/// Организация
	/// </summary>
	public class OrganizationCatalog : GenericCatalog<Organization>
	{
		public OrganizationCatalog(ExportData exportData)
			: base(exportData)
		{
		}

		protected override string Name
		{
			get { return "Организации"; }
		}

		public override ReferenceNode CreateReferenceTo(Organization organization)
		{
			int id = GetReferenceId(organization);
			var referenceNode = new ReferenceNode(id,
				new PropertyNode("ИНН",
					Common1cTypes.String,
					organization.INN
				)
			);

			if(exportData.ExportMode == Export1cMode.ComplexAutomation)
			{
				referenceNode.Properties.Add(
					new PropertyNode("КПП",
						Common1cTypes.String,
						organization.KPP));
			}

			return referenceNode;
		}

		protected override PropertyNode[] GetProperties(Organization organization)
		{
			var properties = new List<PropertyNode>();
			properties.Add(
				new PropertyNode("Наименование",
					Common1cTypes.String,
					organization.FullName
				)
			);
			properties.Add(
				new PropertyNode("Префикс",
					Common1cTypes.String
				)
			);
			properties.Add(
				new PropertyNode("ИНН",
					Common1cTypes.String,
					organization.INN
				)
			);
			properties.Add(
				new PropertyNode("ДатаРегистрации",
					Common1cTypes.Date
				)
			);

			properties.Add(
				new PropertyNode("ОГРН",
					Common1cTypes.String,
					organization.OGRN
				)
			);

			if(exportData.ExportMode != Export1cMode.ComplexAutomation)
			{
				properties.Add(
					new PropertyNode("КПП",
						Common1cTypes.String,
						organization.KPP
					)
				);
			}

			if(exportData.ExportMode != Export1cMode.ComplexAutomation)
			{
				properties.Add(
					new PropertyNode("ВидСтавокЕСНиПФР",
						"ПеречислениеСсылка.УдалитьВидыСтавокЕСНиПФР",
						"ДляНеСельскохозяйственныхПроизводителей"
					)
				);
			}

			properties.Add(
				new PropertyNode("ЮридическоеФизическоеЛицо",
					Common1cTypes.EnumNaturalOrLegal,
					"ЮридическоеЛицо"
				)
			);
			return properties.ToArray();
		}
	}
}

using System;
using System.Collections.Generic;
using Vodovoz.Domain;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.ServiceDialogs.ExportTo1c;

namespace Vodovoz.ExportTo1c.Catalogs
{
	public class OrganizationCatalog:GenericCatalog<Organization>
	{
		public OrganizationCatalog(ExportData exportData)
			:base(exportData)
		{			
		}
		protected override string Name
		{
			get{return "Организации";}
		}

		public override ReferenceNode CreateReferenceTo(Organization organization)
		{
			int id = GetReferenceId(organization);
			return new ReferenceNode(id,
				new PropertyNode("ИНН",
					Common1cTypes.String,
					organization.INN
				)		
			);
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
			
			properties.Add(
				new PropertyNode("КПП",
					Common1cTypes.String,
					organization.KPP
				)
			);

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


using System;
using System.Collections.Generic;
using Vodovoz.Domain;

namespace Vodovoz.ExportTo1c.Catalogs
{
	public class OrganizationCatalog:GenericCatalog<Organization>
	{
		static readonly string defaultOrganizationCode = "00003";
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
			var code1c = defaultOrganizationCode;
			return new ReferenceNode(id,
				new PropertyNode("Код",
					Common1cTypes.String,
					code1c
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
				new PropertyNode("КодПоОКАТО",
					Common1cTypes.String
				)
			);
			properties.Add(
				new PropertyNode("НаименованиеОКВЭД",
					Common1cTypes.String
				)
			);
			properties.Add(
				new PropertyNode("КодОКОНХ",
					Common1cTypes.String
				)
			);
			properties.Add(
				new PropertyNode("КодОКОПФ",
					Common1cTypes.String
				)
			);
			properties.Add(
				new PropertyNode("КодПоОКПО",
					Common1cTypes.String
				)
			);
			properties.Add(
				new PropertyNode("ОГРН",
					Common1cTypes.String,
					organization.OGRN
				)
			);		
			properties.Add(
				new PropertyNode("НаименованиеИМНС",
					Common1cTypes.String
				)
			);
			properties.Add(
				new PropertyNode("КодИМНС",
					Common1cTypes.String
				)
			);
			properties.Add(
				new PropertyNode("НаименованиеПлательщикаПриПеречисленииВБюджет",
					Common1cTypes.String
				)
			);
			properties.Add(
				new PropertyNode("КПП",
					Common1cTypes.String,
					organization.KPP
				)
			);
			properties.Add(
				new PropertyNode("ВидСтавокЕСНиПФР",
					"ПеречислениеСсылка.ВидыСтавокЕСНиПФР",
					"ДляНеСельскохозяйственныхПроизводителей"
				)
			);
			properties.Add(
				new PropertyNode("ЮрФизЛицо",
					Common1cTypes.EnumNaturalOrLegal,
					"ЮрЛицо"
				)
			);
			return properties.ToArray();
		}
	}
}


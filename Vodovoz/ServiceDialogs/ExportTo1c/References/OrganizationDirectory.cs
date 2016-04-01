using System;
using System.Collections.Generic;
using Vodovoz.Domain;

namespace Vodovoz.ExportTo1c.References
{
	public class OrganizationDirectory:GenericDirectory<Organization>
	{
		static readonly string defaultOrganizationCode = "00003";
		public OrganizationDirectory(ExportData exportData)
			:base(exportData)
		{			
		}
		protected override string Name
		{
			get{return "Организации";}
		}

		public override ExportReferenceNode GetReferenceTo(Organization organization)
		{
			int id = GetReferenceId(organization);
			var code1c = defaultOrganizationCode;
			return new ExportReferenceNode(id,
				new ExportPropertyNode("Код",
					Common1cTypes.String,
					code1c
				)		
			);
		}

		protected override ExportPropertyNode[] GetProperties(Organization organization)
		{
			var properties = new List<ExportPropertyNode>();
			properties.Add(
				new ExportPropertyNode("Наименование",
					Common1cTypes.String,
					organization.FullName
				)
			);
			properties.Add(
				new ExportPropertyNode("Префикс",
					Common1cTypes.String
				)
			);
			properties.Add(
				new ExportPropertyNode("ИНН",
					Common1cTypes.String,
					organization.INN
				)
			);
			properties.Add(
				new ExportPropertyNode("ДатаРегистрации",
					Common1cTypes.Date
				)
			);
			properties.Add(
				new ExportPropertyNode("КодПоОКАТО",
					Common1cTypes.String
				)
			);
			properties.Add(
				new ExportPropertyNode("НаименованиеОКВЭД",
					Common1cTypes.String
				)
			);
			properties.Add(
				new ExportPropertyNode("КодОКОНХ",
					Common1cTypes.String
				)
			);
			properties.Add(
				new ExportPropertyNode("КодОКОПФ",
					Common1cTypes.String
				)
			);
			properties.Add(
				new ExportPropertyNode("КодПоОКПО",
					Common1cTypes.String
				)
			);
			properties.Add(
				new ExportPropertyNode("ОГРН",
					Common1cTypes.String,
					organization.OGRN
				)
			);		
			properties.Add(
				new ExportPropertyNode("НаименованиеИМНС",
					Common1cTypes.String
				)
			);
			properties.Add(
				new ExportPropertyNode("КодИМНС",
					Common1cTypes.String
				)
			);
			properties.Add(
				new ExportPropertyNode("НаименованиеПлательщикаПриПеречисленииВБюджет",
					Common1cTypes.String
				)
			);
			properties.Add(
				new ExportPropertyNode("КПП",
					Common1cTypes.String,
					organization.KPP
				)
			);
			properties.Add(
				new ExportPropertyNode("ВидСтавокЕСНиПФР",
					"ПеречислениеСсылка.ВидыСтавокЕСНиПФР",
					"ДляНеСельскохозяйственныхПроизводителей"
				)
			);
			properties.Add(
				new ExportPropertyNode("ЮрФизЛицо",
					Common1cTypes.EnumNaturalOrLegal,
					"ЮрЛицо"
				)
			);
			return properties.ToArray();
		}
	}
}


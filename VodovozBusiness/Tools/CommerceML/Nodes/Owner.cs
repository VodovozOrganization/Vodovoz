using System;
using System.Xml.Linq;
using Vodovoz.Domain;

namespace Vodovoz.Tools.CommerceML.Nodes
{
	public class Owner : GuidNodeBase, IXmlConvertable 
	{
		public Owner(Export export, Organization organization)
		{
			myExport = export;
			this.organization = organization;
		}

		Export myExport;
		Organization organization;

		public override Guid Guid => Guid.Parse("7632aa34-408c-48ad-a3b7-b9c732694c01");

		public XElement ToXml()
		{
			var xml = new XElement("Владелец");
			xml.Add(new XElement("Ид", Guid));
			xml.Add(new XElement("Наименование", organization.Name));
			xml.Add(new XElement("ПолноеНаименование", organization.FullName));
			xml.Add(new XElement("ИНН", organization.INN));
			return xml;
		}
	}
}

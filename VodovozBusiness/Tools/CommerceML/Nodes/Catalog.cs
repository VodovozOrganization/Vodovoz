using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Vodovoz.Tools.CommerceML.Nodes
{
	public class Catalog: IGuidNode, IXmlConvertable 
	{

		public Catalog(Export export, Classifier classifier)
		{
			myExport = export;
			Classifier = classifier;
			goods = new Goods(export);
		}

		Export myExport;
		Goods goods;

		public Guid Guid => Guid.Parse("79ecd59a-403b-4bac-809d-738d4e146b84");
		public string Name = "Основной каталог товаров";
		private Classifier Classifier;

		public XElement ToXml()
		{
			var xml = new XElement("Каталог");
			xml.Add(new XAttribute("СодержитТолькоИзменения", false));
			xml.Add(new XElement("Ид", Guid));
			xml.Add(new XElement("ИдКлассификатора", Classifier.Guid));
			xml.Add(new XElement("Наименование", Name));
			xml.Add(myExport.DefaultOwner.ToXml());
			xml.Add(goods.ToXml());
			return xml;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Vodovoz.Tools.CommerceML.Nodes
{
	public class OffersPackage: IGuidNode, IXmlConvertable 
	{

		public OffersPackage(Export export, Classifier classifier, Catalog catalog)
		{
			myExport = export;
			Catalog = catalog;
			Classifier = classifier;
			offers = new Offers(export);
		}

		Export myExport;
		Offers offers;

		public Guid Guid => Guid.Parse("32848d9e-18c5-4995-95cb-dca685577de1");
		public string Name = "Пакет предложений (Основной каталог товаров)";
		private Classifier Classifier;
		private Catalog Catalog;

		public XElement ToXml()
		{
			var xml = new XElement("ПакетПредложений");
			xml.Add(new XAttribute("СодержитТолькоИзменения", false));
			xml.Add(new XElement("Ид", Guid));
			xml.Add(new XElement("Наименование", Name));
			xml.Add(new XElement("ИдКаталога", Catalog.Guid));
			xml.Add(new XElement("ИдКлассификатора", Classifier.Guid));
			xml.Add(myExport.DefaultOwner.ToXml());
			xml.Add(new XElement("ТипыЦен",
			                     new XElement("ТипЦены",
			                                  new XElement("Ид", myExport.DefaultPriceGuid),
			                                  new XElement("Наименование", "Основная Веселый водовоз"),
			                                  new XElement("Валюта", "руб"),
			                                  new XElement("Налог", 
			                                               new XElement("Наименование", "НДС"),
			                                               new XElement("УчтеноВСумме", true)
			                                 )
			                                 )
			                    )
			       );

			xml.Add(offers.ToXml());
			return xml;
		}
	}
}

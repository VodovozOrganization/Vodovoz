using System;
using System.Xml;
using System.Xml.Linq;

namespace Vodovoz.Tools.CommerceML.Nodes
{
	public class Root: IXmlConvertable
	{
		Export myExport;

		public string Version { get; private set; }
		public DateTime ExportDate { get; private set; }
		public Classifier Classifier { get; private set; }
		public Catalog Catalog { get; private set; }

		public Root(Export export)
		{
			myExport = export;
			Version = "2.04";
			ExportDate = DateTime.Now;
			Classifier = new Classifier(myExport);
			Catalog = new Catalog(myExport, Classifier);
		}

		public XElement ToXml()
		{
			var xml = new XElement("КоммерческаяИнформация",
			                       new XAttribute("ВерсияСхемы", Version),
			                       new XAttribute("ДатаФормирования", ExportDate)
					  );
			xml.Add(Classifier.ToXml());
			xml.Add(Catalog.ToXml());
			return xml;
		}
	}
}

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
		public OffersPackage Offers { get; private set; }

		private RootContents Contents;

		public Root(Export export, RootContents contents)
		{
			myExport = export;
			Version = "2.04";
			ExportDate = DateTime.Now;
			Contents = contents;

			switch(contents) {
				case RootContents.Catalog:
					myExport.Classifier = Classifier = new Classifier(myExport);
					myExport.Catalog = Catalog = new Catalog(myExport, Classifier);
					break;

				case RootContents.Offers:
					Offers = new OffersPackage(myExport, myExport.Classifier, myExport.Catalog);
					break;
				default:
					break;
			}

		}

		public XElement ToXml()
		{
			var xml = new XElement("КоммерческаяИнформация",
			                       new XAttribute("ВерсияСхемы", Version),
			                       new XAttribute("ДатаФормирования", ExportDate)
					  );

			switch(Contents) {
				case RootContents.Catalog:
					xml.Add(Classifier.ToXml());
					xml.Add(Catalog.ToXml());
					break;
				case RootContents.Offers:
					xml.Add(Offers.ToXml());
					break;
				default:
					break;
			}

			return xml;
		}

		public XDocument GetXDocument()
        {
            return new XDocument(
                        new XDeclaration("1.0", "UTF-8", ""),
                          ToXml()
                    );
        }

        public void WriteToStream(System.IO.Stream stream)
		{
			using(XmlWriter writer = XmlWriter.Create(stream, Export.WriterSettings)) {
                GetXDocument().WriteTo(writer);
			}
		}
	}

	public enum RootContents
	{
		Catalog,
		Offers
	}
}

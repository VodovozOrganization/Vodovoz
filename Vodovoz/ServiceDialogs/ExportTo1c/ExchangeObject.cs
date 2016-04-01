using System;
using System.Xml.Serialization;
using System.Xml.Linq;

namespace Vodovoz
{
	[Serializable]
	[XmlInclude(typeof(ExchangeCatalogueObject))]
	[XmlInclude(typeof(ExchangeDocumentSale))]
	public abstract class ExchangeObject : IXmlConvertable
	{
		[XmlAttribute("Нпп")]
		public int Id{get;set;}
		[XmlAttribute("Тип")]
		public virtual string Type{get;}
		[XmlAttribute("ИмяПравила")]
		public virtual string RuleName{get;set;}

		public abstract XElement ToXml();
	}
}


using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class Values
	{
		private string[] valueField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("Value")]
		public string[] Value
		{
			get => valueField;
			set => valueField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}
using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class CustomProperties
	{
		private CustomProperty[] customPropertyField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("CustomProperty")]
		public CustomProperty[] CustomProperty
		{
			get => customPropertyField;
			set => customPropertyField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}
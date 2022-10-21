using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class Fields
	{
		private Field[] fieldField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("Field")]
		public Field[] Field
		{
			get => fieldField;
			set => fieldField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}
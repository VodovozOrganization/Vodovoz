using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class StaticColumns
	{
		private StaticColumn[] staticColumnField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("StaticColumn")]
		public StaticColumn[] StaticColumn
		{
			get => staticColumnField;
			set => staticColumnField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}
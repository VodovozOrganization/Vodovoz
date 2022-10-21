using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class StaticRows
	{
		private StaticRow[] staticRowField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("StaticRow")]
		public StaticRow[] StaticRow
		{
			get => staticRowField;
			set => staticRowField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}
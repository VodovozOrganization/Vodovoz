using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class StaticSeries
	{
		private StaticMember[] staticMemberField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("StaticMember")]
		public StaticMember[] StaticMember
		{
			get => staticMemberField;
			set => staticMemberField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}
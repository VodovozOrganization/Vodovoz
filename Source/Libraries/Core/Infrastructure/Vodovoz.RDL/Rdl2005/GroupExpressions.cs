using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class GroupExpressions
	{
		private string[] groupExpressionField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("GroupExpression")]
		public string[] GroupExpression
		{
			get => groupExpressionField;
			set => groupExpressionField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}
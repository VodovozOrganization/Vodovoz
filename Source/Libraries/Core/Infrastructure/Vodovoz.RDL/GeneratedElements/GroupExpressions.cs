using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class GroupExpressions
	{
		private List<string> groupExpressionField = new List<string>();
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlElement("GroupExpression")]
		public List<string> GroupExpression
		{
			get => groupExpressionField;
			set => groupExpressionField = value;
		}

		[XmlIgnore()]
		public List<XmlAttribute> AnyAttrList
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => AnyAttrList.ToArray();
			set => AnyAttrList = value == null ? new List<XmlAttribute>() : value.ToList();
		}
	}
}

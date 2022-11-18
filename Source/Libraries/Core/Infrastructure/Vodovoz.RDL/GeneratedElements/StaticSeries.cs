using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class StaticSeries
	{
		private List<StaticMember> staticMemberField = new List<StaticMember>();
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlElement("StaticMember")]
		public List<StaticMember> StaticMember
		{
			get => staticMemberField;
			set => staticMemberField = value;
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

using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class CategoryGroupings
	{
		private List<CategoryGrouping> categoryGroupingField = new List<CategoryGrouping>();
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlElement("CategoryGrouping")]
		public List<CategoryGrouping> CategoryGrouping
		{
			get => categoryGroupingField;
			set => categoryGroupingField = value;
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

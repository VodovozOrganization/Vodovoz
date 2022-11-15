using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class ParameterValues
	{
		private List<ParameterValue> parameterValueField = new List<ParameterValue>();
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlElement("ParameterValue")]
		public List<ParameterValue> ParameterValue
		{
			get => parameterValueField;
			set => parameterValueField = value;
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

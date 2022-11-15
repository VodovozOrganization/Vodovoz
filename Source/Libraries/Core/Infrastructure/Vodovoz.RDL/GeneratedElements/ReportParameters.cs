using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class ReportParameters
	{
		private List<ReportParameter> reportParameterField = new List<ReportParameter>();
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlElement("ReportParameter")]
		public List<ReportParameter> ReportParameter
		{
			get => reportParameterField;
			set => reportParameterField = value;
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

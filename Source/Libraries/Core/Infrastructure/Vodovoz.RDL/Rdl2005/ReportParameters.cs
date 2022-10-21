using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class ReportParameters
	{
		private ReportParameter[] reportParameterField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("ReportParameter")]
		public ReportParameter[] ReportParameter
		{
			get => reportParameterField;
			set => reportParameterField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}
using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class ReportItems
	{
		private object[] itemsField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("Chart", typeof(Chart))]
		[XmlElement("CustomReportItem", typeof(CustomReportItem))]
		[XmlElement("Image", typeof(Image))]
		[XmlElement("Line", typeof(Line))]
		[XmlElement("List", typeof(List))]
		[XmlElement("Matrix", typeof(Matrix))]
		[XmlElement("Rectangle", typeof(Rectangle))]
		[XmlElement("Subreport", typeof(Subreport))]
		[XmlElement("Table", typeof(Table))]
		[XmlElement("Textbox", typeof(Textbox))]
		public object[] Items
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}
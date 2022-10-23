using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType(AnonymousType = true, Namespace = "http://schemas.microsoft.com/sqlserver/reporting/2005/01/reportdefinition")]
	[XmlRoot(IsNullable = false, Namespace = "http://schemas.microsoft.com/sqlserver/reporting/2005/01/reportdefinition")]
	public partial class Report
	{
		private object[] itemsField;
		private ItemsChoiceType37[] itemsElementNameField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("Author", typeof(string))]
		[XmlElement("AutoRefresh", typeof(uint))]
		[XmlElement("Body", typeof(Body))]
		[XmlElement("BottomMargin", typeof(string), DataType = "normalizedString")]
		[XmlElement("Classes", typeof(Classes))]
		[XmlElement("Code", typeof(string))]
		[XmlElement("CodeModules", typeof(CodeModules))]
		[XmlElement("CustomProperties", typeof(CustomProperties))]
		[XmlElement("DataElementName", typeof(string))]
		[XmlElement("DataElementStyle", typeof(ReportDataElementStyle))]
		[XmlElement("DataSchema", typeof(string))]
		[XmlElement("DataSets", typeof(DataSets))]
		[XmlElement("DataSources", typeof(DataSources))]
		[XmlElement("DataTransform", typeof(string))]
		[XmlElement("Description", typeof(string))]
		[XmlElement("EmbeddedImages", typeof(EmbeddedImages))]
		[XmlElement("InteractiveHeight", typeof(string), DataType = "normalizedString")]
		[XmlElement("InteractiveWidth", typeof(string), DataType = "normalizedString")]
		[XmlElement("Language", typeof(string))]
		[XmlElement("LeftMargin", typeof(string), DataType = "normalizedString")]
		[XmlElement("PageFooter", typeof(PageHeaderFooter))]
		[XmlElement("PageHeader", typeof(PageHeaderFooter))]
		[XmlElement("PageHeight", typeof(string), DataType = "normalizedString")]
		[XmlElement("PageWidth", typeof(string), DataType = "normalizedString")]
		[XmlElement("ReportParameters", typeof(ReportParameters))]
		[XmlElement("RightMargin", typeof(string), DataType = "normalizedString")]
		[XmlElement("TopMargin", typeof(string), DataType = "normalizedString")]
		[XmlElement("Width", typeof(string), DataType = "normalizedString")]
		[XmlChoiceIdentifier("ItemsElementName")]
		public object[] Items
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType37[] ItemsElementName
		{
			get => itemsElementNameField;
			set => itemsElementNameField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum ReportDataElementStyle
	{
		AttributeNormal,
		ElementNormal,
	}

	[Serializable()]
	[XmlType(IncludeInSchema = false)]
	public enum ItemsChoiceType37
	{
		[XmlEnum("##any:")]
		Item,
		Author,
		AutoRefresh,
		Body,
		BottomMargin,
		Classes,
		Code,
		CodeModules,
		CustomProperties,
		DataElementName,
		DataElementStyle,
		DataSchema,
		DataSets,
		DataSources,
		DataTransform,
		Description,
		EmbeddedImages,
		InteractiveHeight,
		InteractiveWidth,
		Language,
		LeftMargin,
		PageFooter,
		PageHeader,
		PageHeight,
		PageWidth,
		ReportParameters,
		RightMargin,
		TopMargin,
		Width,
	}
}
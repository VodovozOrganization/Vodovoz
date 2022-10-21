using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class CustomReportItem
	{
		private object[] itemsField;
		private ItemsChoiceType29[] itemsElementNameField;
		private string nameField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("AltReportItem", typeof(ReportItems))]
		[XmlElement("Bookmark", typeof(string))]
		[XmlElement("CustomData", typeof(CustomData))]
		[XmlElement("CustomProperties", typeof(CustomProperties))]
		[XmlElement("DataElementName", typeof(string))]
		[XmlElement("DataElementOutput", typeof(CustomReportItemTypeDataElementOutput))]
		[XmlElement("Height", typeof(string), DataType = "normalizedString")]
		[XmlElement("Label", typeof(string))]
		[XmlElement("Left", typeof(string), DataType = "normalizedString")]
		[XmlElement("RepeatWith", typeof(string))]
		[XmlElement("Style", typeof(Style))]
		[XmlElement("Top", typeof(string), DataType = "normalizedString")]
		[XmlElement("Type", typeof(string))]
		[XmlElement("Visibility", typeof(Visibility))]
		[XmlElement("Width", typeof(string), DataType = "normalizedString")]
		[XmlElement("ZIndex", typeof(uint))]
		[XmlChoiceIdentifier("ItemsElementName")]
		public object[] Items
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType29[] ItemsElementName
		{
			get => itemsElementNameField;
			set => itemsElementNameField = value;
		}

		[XmlAttribute(DataType = "normalizedString")]
		public string Name
		{
			get => nameField;
			set => nameField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}

	[Serializable()]
	[XmlType(IncludeInSchema = false)]
	public enum ItemsChoiceType29
	{
		[XmlEnum("##any:")]
		Item,
		AltReportItem,
		Bookmark,
		CustomData,
		CustomProperties,
		DataElementName,
		DataElementOutput,
		Height,
		Label,
		Left,
		RepeatWith,
		Style,
		Top,
		Type,
		Visibility,
		Width,
		ZIndex,
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum CustomReportItemTypeDataElementOutput
	{
		Output,
		NoOutput,
		ContentsOnly,
		Auto,
	}
}
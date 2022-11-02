using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class CustomReportItem
	{
		private List<object> itemsField = new List<object>();
		private List<ItemsChoiceType29> itemsElementNameField = new List<ItemsChoiceType29>();
		private string nameField;
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlIgnore()]
		public List<object> ItemsList
		{
			get => itemsField;
			set => itemsField = value;
		}

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
			get => ItemsList.ToArray();
			set => ItemsList = value == null ? new List<object>() : value.ToList();
		}

		[XmlIgnore()]
		public List<ItemsChoiceType29> ItemsElementNameList
		{
			get => itemsElementNameField;
			set => itemsElementNameField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType29[] ItemsElementName
		{
			get => ItemsElementNameList.ToArray();
			set => ItemsElementNameList = value == null ? new List<ItemsChoiceType29>() : value.ToList();
		}

		[XmlAttribute(DataType = "normalizedString")]
		public string Name
		{
			get => nameField;
			set => nameField = value;
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

using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class Axis
	{
		private List<object> itemsField = new List<object>();
		private List<ItemsChoiceType25> itemsElementNameField = new List<ItemsChoiceType25>();
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlIgnore()]
		public List<object> ItemsList
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlAnyElement()]
		[XmlElement("CrossAt", typeof(string))]
		[XmlElement("Interlaced", typeof(bool))]
		[XmlElement("LogScale", typeof(bool))]
		[XmlElement("MajorGridLines", typeof(MajorGridLines))]
		[XmlElement("MajorInterval", typeof(string))]
		[XmlElement("MajorTickMarks", typeof(AxisTypeMajorTickMarks))]
		[XmlElement("Margin", typeof(bool))]
		[XmlElement("Max", typeof(string))]
		[XmlElement("Min", typeof(string))]
		[XmlElement("MinorGridLines", typeof(MinorGridLines))]
		[XmlElement("MinorInterval", typeof(string))]
		[XmlElement("MinorTickMarks", typeof(AxisTypeMinorTickMarks))]
		[XmlElement("Reverse", typeof(bool))]
		[XmlElement("Scalar", typeof(bool))]
		[XmlElement("Style", typeof(Style))]
		[XmlElement("Title", typeof(Title))]
		[XmlElement("Visible", typeof(bool))]
		[XmlChoiceIdentifier("ItemsElementName")]
		public object[] Items
		{
			get => ItemsList.ToArray();
			set => ItemsList = value == null ? new List<object>() : value.ToList();
		}

		[XmlIgnore()]
		public List<ItemsChoiceType25> ItemsElementNameList
		{
			get => itemsElementNameField;
			set => itemsElementNameField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType25[] ItemsElementName
		{
			get => ItemsElementNameList.ToArray();
			set => ItemsElementNameList = value == null ? new List<ItemsChoiceType25>() : value.ToList();
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
	[XmlType(AnonymousType = true)]
	public enum AxisTypeMajorTickMarks
	{
		None,
		Inside,
		Outside,
		Cross,
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum AxisTypeMinorTickMarks
	{
		None,
		Inside,
		Outside,
		Cross,
	}

	[Serializable()]
	[XmlType(IncludeInSchema = false)]
	public enum ItemsChoiceType25
	{
		[XmlEnum("##any:")]
		Item,
		CrossAt,
		Interlaced,
		LogScale,
		MajorGridLines,
		MajorInterval,
		MajorTickMarks,
		Margin,
		Max,
		Min,
		MinorGridLines,
		MinorInterval,
		MinorTickMarks,
		Reverse,
		Scalar,
		Style,
		Title,
		Visible,
	}
}

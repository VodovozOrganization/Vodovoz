using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class Axis
	{
		private object[] itemsField;
		private ItemsChoiceType25[] itemsElementNameField;
		private XmlAttribute[] anyAttrField;

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
			get => itemsField;
			set => itemsField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType25[] ItemsElementName
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
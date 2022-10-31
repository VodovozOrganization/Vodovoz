using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class Chart
	{
		private object[] itemsField;
		private ItemsChoiceType27[] itemsElementNameField;
		private string nameField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("Action", typeof(Action))]
		[XmlElement("Bookmark", typeof(string))]
		[XmlElement("CategoryAxis", typeof(CategoryAxis))]
		[XmlElement("CategoryGroupings", typeof(CategoryGroupings))]
		[XmlElement("ChartData", typeof(ChartData))]
		[XmlElement("ChartElementOutput", typeof(ChartTypeChartElementOutput))]
		[XmlElement("CustomProperties", typeof(CustomProperties))]
		[XmlElement("DataElementName", typeof(string))]
		[XmlElement("DataElementOutput", typeof(ChartTypeDataElementOutput))]
		[XmlElement("DataSetName", typeof(string))]
		[XmlElement("Filters", typeof(Filters))]
		[XmlElement("Height", typeof(string), DataType = "normalizedString")]
		[XmlElement("KeepTogether", typeof(bool))]
		[XmlElement("Label", typeof(string))]
		[XmlElement("Left", typeof(string), DataType = "normalizedString")]
		[XmlElement("Legend", typeof(Legend))]
		[XmlElement("LinkToChild", typeof(string))]
		[XmlElement("NoRows", typeof(string))]
		[XmlElement("PageBreakAtEnd", typeof(bool))]
		[XmlElement("PageBreakAtStart", typeof(bool))]
		[XmlElement("Palette", typeof(ChartTypePalette))]
		[XmlElement("PlotArea", typeof(PlotArea))]
		[XmlElement("PointWidth", typeof(uint))]
		[XmlElement("SeriesGroupings", typeof(SeriesGroupings))]
		[XmlElement("Style", typeof(Style))]
		[XmlElement("Subtype", typeof(ChartTypeSubtype))]
		[XmlElement("ThreeDProperties", typeof(ThreeDProperties))]
		[XmlElement("Title", typeof(Title))]
		[XmlElement("ToolTip", typeof(string))]
		[XmlElement("Top", typeof(string), DataType = "normalizedString")]
		[XmlElement("Type", typeof(ChartTypeType))]
		[XmlElement("ValueAxis", typeof(ValueAxis))]
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
		public ItemsChoiceType27[] ItemsElementName
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
	[XmlType(AnonymousType = true)]
	public enum ChartTypeChartElementOutput
	{
		Output,
		NoOutput,
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum ChartTypeDataElementOutput
	{
		Output,
		NoOutput,
		ContentsOnly,
		Auto,
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum ChartTypePalette
	{
		Default,
		EarthTones,
		Excel,
		GrayScale,
		Light,
		Pastel,
		SemiTransparent,
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum ChartTypeSubtype
	{
		Stacked,
		PercentStacked,
		Plain,
		Smooth,
		Exploded,
		Line,
		SmoothLine,
		HighLowClose,
		OpenHighLowClose,
		Candlestick,
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum ChartTypeType
	{
		Column,
		Bar,
		Line,
		Pie,
		Scatter,
		Bubble,
		Area,
		Doughnut,
		Stock,
	}

	[Serializable()]
	[XmlType(IncludeInSchema = false)]
	public enum ItemsChoiceType27
	{
		[XmlEnum("##any:")]
		Item,
		Action,
		Bookmark,
		CategoryAxis,
		CategoryGroupings,
		ChartData,
		ChartElementOutput,
		CustomProperties,
		DataElementName,
		DataElementOutput,
		DataSetName,
		Filters,
		Height,
		KeepTogether,
		Label,
		Left,
		Legend,
		LinkToChild,
		NoRows,
		PageBreakAtEnd,
		PageBreakAtStart,
		Palette,
		PlotArea,
		PointWidth,
		SeriesGroupings,
		Style,
		Subtype,
		ThreeDProperties,
		Title,
		ToolTip,
		Top,
		Type,
		ValueAxis,
		Visibility,
		Width,
		ZIndex,
	}
}
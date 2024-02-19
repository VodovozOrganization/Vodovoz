using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class Chart : IReportItemElement
	{
		private List<object> itemsField = new List<object>();
		private List<ItemsChoiceType27> itemsElementNameField = new List<ItemsChoiceType27>();
		private string nameField;
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlIgnore()]
		public List<object> ItemsList
		{
			get => itemsField;
			set => itemsField = value;
		}

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
		[XmlElement("KeepTogether", typeof(bool))]
		[XmlElement("Label", typeof(string))]
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
		[XmlElement("Type", typeof(ChartTypeType))]
		[XmlElement("ValueAxis", typeof(ValueAxis))]
		[XmlElement("Visibility", typeof(Visibility))]
		[XmlElement("ZIndex", typeof(uint))]
		[XmlChoiceIdentifier("ItemsElementName")]
		public object[] Items
		{
			get => ItemsList.ToArray();
			set => ItemsList = value == null ? new List<object>() : value.ToList();
		}

		[XmlIgnore()]
		public List<ItemsChoiceType27> ItemsElementNameList
		{
			get => itemsElementNameField;
			set => itemsElementNameField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType27[] ItemsElementName
		{
			get => ItemsElementNameList.ToArray();
			set => ItemsElementNameList = value == null ? new List<ItemsChoiceType27>() : value.ToList();
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

		private string _top;

		public string Top
		{
			get => _top;
			set
			{
				_top = value;
				ParseTopValue(_top);
			}
		}
		
		[XmlIgnore]
		public decimal TopSize { get; set; }
		[XmlIgnore]
		public string TopDimension { get; set; }
		
		private string _left;

		public string Left
		{
			get => _left;
			set
			{
				_left = value;
				ParseTopValue(_left);
			}
		}

		[XmlIgnore]
		public decimal LeftSize { get; set; }
		[XmlIgnore]
		public string LeftDimension { get; set; }

		public void ParseTopValue(string top)
		{
			TopDimension = top.Substring(top.Length - 2, 2);
			var size = top.Substring(0, top.Length - 2);
			TopSize = decimal.Parse(size, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
		}
		
		public void ParseLeftValue(string left)
		{
			LeftDimension = left.Substring(left.Length - 2, 2);
			var size = left.Substring(0, left.Length - 2);
			LeftSize = decimal.Parse(size, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
		}
		
		private string _height;

		public string Height
		{
			get => _height;
			set
			{
				_height = value;
				ParseHeightValue(_height);
			}
		}
		
		[XmlIgnore]
		public decimal HeightSize { get; set; }
		[XmlIgnore]
		public string HeightDimension { get; set; }

		private string _width;

		public string Width
		{
			get => _width;
			set
			{
				_width = value;
				ParseWidthValue(_width);
			}
		}
		
		[XmlIgnore]
		public decimal WidthSize { get; set; }
		[XmlIgnore]
		public string WidthDimension { get; set; }

		public void ParseHeightValue(string height)
		{
			HeightDimension = height.Substring(height.Length - 2, 2);
			var size = height.Substring(0, height.Length - 2);
			HeightSize = decimal.Parse(size, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
		}
		
		public void ParseWidthValue(string width)
		{
			WidthDimension = width.Substring(width.Length - 2, 2);
			var size = width.Substring(0, width.Length - 2);
			WidthSize = decimal.Parse(size, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
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
		KeepTogether,
		Label,
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
		Type,
		ValueAxis,
		Visibility,
		ZIndex,
	}
}

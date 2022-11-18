using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class MatrixCells
	{
		private List<MatrixCell> matrixCellField = new List<MatrixCell>();
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlElement("MatrixCell")]
		public List<MatrixCell> MatrixCell
		{
			get => matrixCellField;
			set => matrixCellField = value;
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

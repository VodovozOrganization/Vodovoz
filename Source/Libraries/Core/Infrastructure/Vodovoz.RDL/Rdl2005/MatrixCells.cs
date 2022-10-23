using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class MatrixCells
	{
		private MatrixCell[] matrixCellField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("MatrixCell")]
		public MatrixCell[] MatrixCell
		{
			get => matrixCellField;
			set => matrixCellField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}
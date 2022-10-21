using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class CodeModules
	{
		private string[] codeModuleField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("CodeModule")]
		public string[] CodeModule
		{
			get => codeModuleField;
			set => codeModuleField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}
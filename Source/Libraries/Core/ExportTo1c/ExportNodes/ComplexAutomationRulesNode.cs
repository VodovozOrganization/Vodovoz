using System.Xml.Linq;
using Vodovoz.Tools;

namespace ExportTo1c.Library.ExportNodes
{
	public partial class ComplexAutomationRulesNode : IXmlConvertable, IRulesNode
	{
		#region IXmlConvertable implementation

		public XElement ToXml()
		{
			return XElement.Parse(ExportTo1c.ExportNodes.ComplexAutomationRulesNode.ComplexAutomationDefaultExchangeStrings
				.DefaultExchangeRules);
		}

		#endregion
	}
}

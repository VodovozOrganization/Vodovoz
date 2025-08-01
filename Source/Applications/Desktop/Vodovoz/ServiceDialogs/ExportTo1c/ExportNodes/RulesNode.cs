using System.Xml.Linq;
using Vodovoz.ExportTo1c;

namespace Vodovoz.ServiceDialogs.ExportTo1c.ExportNodes
{
	public partial class RulesNode : IXmlConvertable, IRulesNode
	{
		#region IXmlConvertable implementation

		public XElement ToXml()
		{
			return XElement.Parse(DefaultExchangeStrings.DefaultExchangeRules);
		}

		#endregion
	}
}


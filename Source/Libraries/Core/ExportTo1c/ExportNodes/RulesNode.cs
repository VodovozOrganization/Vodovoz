using System.Xml.Linq;
using Vodovoz.Tools;

namespace ExportTo1c.Library.ExportNodes
{
	/// <summary>
	/// Правило обмена 1С Бухгалтерия
	/// </summary>
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

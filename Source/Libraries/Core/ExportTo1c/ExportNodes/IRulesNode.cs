using System.Xml.Linq;

namespace ExportTo1c.Library.ExportNodes
{
	/// <summary>
	/// Правило обмена
	/// </summary>
	public interface IRulesNode
	{
		XElement ToXml();
	}
}

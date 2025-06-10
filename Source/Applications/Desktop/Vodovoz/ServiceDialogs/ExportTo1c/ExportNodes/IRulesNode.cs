using System.Xml.Linq;

namespace Vodovoz.ExportTo1c
{
	public interface IRulesNode
	{
		XElement ToXml();
	}
}
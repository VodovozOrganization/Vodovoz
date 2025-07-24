using System.Xml.Linq;

namespace ExportTo1c.Library.ExportNodes
{
	public interface IRulesNode
	{
		XElement ToXml();
	}
}

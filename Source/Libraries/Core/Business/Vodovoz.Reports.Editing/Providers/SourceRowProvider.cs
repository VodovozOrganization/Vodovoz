using System.Xml.Linq;
using Vodovoz.RDL.Elements;

namespace Vodovoz.Reports.Editing.Providers
{
	public abstract class SourceRowProvider
	{
		public abstract TableRow GetSourceRow(XDocument report, string tableName);
	}
}

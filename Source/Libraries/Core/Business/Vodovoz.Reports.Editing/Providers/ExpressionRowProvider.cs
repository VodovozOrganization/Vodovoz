using System.Xml.Linq;
using Vodovoz.RDL.Elements;

namespace Vodovoz.Reports.Editing.Providers
{
	public abstract class ExpressionRowProvider
	{
		public abstract TableRow GetExpressionRow(XDocument report, string tableName);
	}
}

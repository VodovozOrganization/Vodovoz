using System.Xml.Linq;

namespace Vodovoz.Reports.Editing
{
	public abstract class ModifierAction
	{
		public abstract void Modify(XDocument report);
	}
}

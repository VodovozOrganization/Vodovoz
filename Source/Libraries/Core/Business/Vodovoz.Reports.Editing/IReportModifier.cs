using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Vodovoz.Reports.Editing
{
	public interface IReportModifier
	{
		void ApplyChanges(XDocument document);
	}
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Vodovoz.Reports.Editing
{
	public abstract class ModifierAction
	{
		public abstract void Modify(XDocument report);

		public abstract IEnumerable<ValidationResult> Validate(XDocument report);
	}
}

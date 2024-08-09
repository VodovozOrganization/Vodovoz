using System.Collections.Generic;
using System.Xml.Linq;

namespace Vodovoz.Reports.Editing
{
	public abstract class ReportModifierBase : IReportModifier
	{
		protected List<ModifierAction> ModifierActions = new List<ModifierAction>();

		public virtual void AddAction(ModifierAction modifierAction)
		{
			if(ModifierActions.Contains(modifierAction))
			{
				return;
			}
			ModifierActions.Add(modifierAction);
		}

		public virtual void AddActions(IEnumerable<ModifierAction> modifierActions)
		{
			foreach(var modifierAction in modifierActions)
			{
				if(ModifierActions.Contains(modifierAction))
				{
					continue;
				}
				ModifierActions.Add(modifierAction);
			}
		}

		public virtual void ApplyChanges(XDocument reportDocument)
		{
			foreach(var modifierAction in ModifierActions)
			{
				modifierAction.Modify(reportDocument);
			}
		}
	}
}

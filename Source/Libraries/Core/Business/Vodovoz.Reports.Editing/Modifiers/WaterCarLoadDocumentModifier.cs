using Vodovoz.Reports.Editing.ModifierActions;

namespace Vodovoz.Reports.Editing.Modifiers
{
	public class WaterCarLoadDocumentModifier : ReportModifierBase
	{
		private const string _dataTableName = "TableData";

		private static ModifierAction SetTablePositionAction(string identifier, double left, double top)
		{
			return new SetTablePosition($"Table{identifier}", left, top);
		}

		private static ModifierAction RemoveTableAction(string identifier)
		{
			return new RemoveTable($"Table{identifier}");
		}
	}
}

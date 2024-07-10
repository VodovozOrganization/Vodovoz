using System.Collections.Generic;
using Vodovoz.Reports.Editing.ModifierActions;

namespace Vodovoz.Reports.Editing.Modifiers
{
	public class WaterCarLoadDocumentModifier : ReportModifierBase
	{
		private const string _dataTableName = "TableData";

		public void Setup()
		{
			AddActions(CopyTableAndPlaceToPositionActions(_dataTableName, "TableDataNew", 0, 670));
		}

		private static IEnumerable<ModifierAction> CopyTableAndPlaceToPositionActions(string sourceTableName, string newTableName, double newTableLeft, double newTableTop)
		{
			var actions = new List<ModifierAction>
			{
				CopyTableAction(sourceTableName, newTableName),
				SetTablePositionAction(sourceTableName, newTableLeft, newTableTop)
			};

			return actions;
		}

		private static ModifierAction CopyTableAction(string sourceTableName, string newTableName)
		{
			return new CopyElement(sourceTableName, newTableName);
		}

		private static ModifierAction SetTablePositionAction(string tableName, double left, double top)
		{
			return new SetTablePosition(tableName, left, top);
		}

		private static ModifierAction RemoveTableAction(string tableName)
		{
			return new RemoveTable(tableName);
		}
	}
}

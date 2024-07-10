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

		private static IEnumerable<ModifierAction> CopyTableAndPlaceToPositionActions(string sourceTableName, string newTableName, decimal newTableLeft, decimal newTableTop)
		{
			var actions = new List<ModifierAction>
			{
				CopyTableAction(sourceTableName, newTableName),
				SetElementPosition(sourceTableName, newTableLeft, newTableTop),
				AddOrderEqualTableFilter(sourceTableName, "150")
			};

			return actions;
		}

		private static ModifierAction CopyTableAction(string sourceTableName, string newTableName)
		{
			return new CopyElement(sourceTableName, newTableName);
		}

		private static ModifierAction RemoveTableAction(string tableName)
		{
			return new RemoveTable(tableName);
		}

		private static ModifierAction SetElementPosition(string elementName, decimal leftPositionInPt, decimal topPositionInPt)
		{
			return new SetElementPosition(elementName, leftPositionInPt, topPositionInPt);
		}

		private static ModifierAction MoveElementDown(string elementName, decimal offsetInPt)
		{
			return new MoveElementDown(elementName, offsetInPt);
		}

		private static ModifierAction RenameElement(string elementOldName, string elementNewName)
		{
			return new RenameElement(elementOldName, elementNewName);
		}

		private static ModifierAction AddOrderEqualTableFilter(string tableName, string value)
		{
			return AddTableFilter(tableName, "={amount}", "Equal", value);
		}

		private static ModifierAction AddOrderNotEqualTableFilter(string tableName, string value)
		{
			return AddTableFilter(tableName, "={amount}", "NotEqual", value);
		}

		private static ModifierAction AddTableFilter(string tableName, string expression, string @operator, string value)
		{
			return new AddTableFilter(tableName, expression, @operator, value);
		}
	}
}

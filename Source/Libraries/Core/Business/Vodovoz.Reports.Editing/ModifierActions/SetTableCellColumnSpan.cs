using System;
using System.Linq;
using System.Xml.Linq;
using Vodovoz.RDL.Elements;
using Vodovoz.RDL.Utilities;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.ModifierActions
{
	public class SetTableCellColumnSpan : ModifierAction
	{
		private readonly string _table;
		private readonly string _innerTextBoxName;
		private readonly uint _columnSpan;

		public SetTableCellColumnSpan(string table, string innerTextBoxName, uint columnSpan)
		{
			if(string.IsNullOrWhiteSpace(table))
			{
				throw new ArgumentException($"'{nameof(table)}' cannot be null or whitespace.", nameof(table));
			}

			if(string.IsNullOrWhiteSpace(innerTextBoxName))
			{
				throw new ArgumentException($"'{nameof(innerTextBoxName)}' cannot be null or whitespace.", nameof(innerTextBoxName));
			}

			if(columnSpan < 1)
			{
				throw new InvalidOperationException($"{columnSpan} не может быть меньше 1.");
			}

			_table = table;
			_innerTextBoxName = innerTextBoxName;
			_columnSpan = columnSpan;
		}

		public override void Modify(XDocument report)
		{
			var @namespace = report.Root.Attribute("xmlns").Value;
			var textBoxElement = report.GetTable(_table, @namespace)
				.GetTextbox(_innerTextBoxName, @namespace);
			var tableCellElement = textBoxElement.Parent.Parent;
			var cellsAfter = tableCellElement.ElementsAfterSelf().ToList();

			var tableCell = tableCellElement.FromXElement<TableCell>();
			tableCell.ColSpan = _columnSpan;
			var modifiedTableCellElement = tableCell.ToXElement<TableCell>(@namespace);
			modifiedTableCellElement.RemoveAttributes();
			tableCellElement.ReplaceWith(modifiedTableCellElement);

			//Удаление закрытых ячеек
			var cellsForDeleting = cellsAfter.Take((int)_columnSpan - 1);
			foreach(var deletingCell in cellsForDeleting)
			{
				deletingCell.Remove();
			}
		}
	}
}

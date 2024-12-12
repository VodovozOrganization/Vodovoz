using System;
using System.Linq;
using System.Xml.Linq;
using Vodovoz.Reports.Editing.Providers;

namespace Vodovoz.Reports.Editing.ModifierActions
{
	public class SetTablePosition : ModifierAction
	{
		private readonly string _tableName;
		private readonly double _leftValueInPt;
		private readonly double _topValueInPt;

		public SetTablePosition(string tableName, double leftValueInPt, double topValueInPt)
		{
			if(string.IsNullOrWhiteSpace(tableName))
			{
				throw new ArgumentException($"'{nameof(tableName)}' cannot be null or whitespace.", nameof(tableName));
			}

			_tableName = tableName;
			_leftValueInPt = leftValueInPt;
			_topValueInPt = topValueInPt;
		}

		public override void Modify(XDocument report)
		{
			var @namespace = report.Root.Attribute("xmlns").Value;
			report.SetTablePosition(_tableName, @namespace, _leftValueInPt, _topValueInPt);
		}
	}
}

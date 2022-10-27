using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;
using Vodovoz.RDL.Elements;

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

		public virtual void ApplyChanges(XDocument reportDocument)
		{
			var report = DeserializeReport(reportDocument);
			if(report == null)
			{
				throw new ArgumentException($"Ошибка во время десериализации отчета. {report} is null.");
			}

			foreach(var modifierAction in ModifierActions)
			{
				modifierAction.Modify(reportDocument);
			}
		}

		protected virtual Report DeserializeReport(XDocument report)
		{
			using(var reader = report.CreateReader())
			{
				reader.MoveToContent();
				var serializer = new XmlSerializer(typeof(Report));
				var result = (Report)serializer.Deserialize(reader);
				return result;
			}
		}
	}
}

using System;
using DiffPlex;
using DiffPlex.DiffBuilder;
using QSHistoryLog;

namespace ServiceDialogs.LoadFrom1c
{
	public class FieldChange
	{
		public virtual int Id { get; set; }

		public virtual string Title { get; set; }
		public virtual string OldValue { get; set; }
		public virtual string NewValue { get; set; }

		string oldPangoText;
		public virtual string OldPangoText {
			get { if (!isPangoMade)
					MakeDiffPangoMarkup ();
				return oldPangoText;
			}
			private set {
				oldPangoText = value;
			}
		}

		string newPangoText;
		public virtual string NewPangoText {
			get {
				if (!isPangoMade)
					MakeDiffPangoMarkup ();
				return newPangoText;
			}
			private set {
				newPangoText = value;
			}
		}

		private bool isPangoMade = false;

		public FieldChange (string title, string oldvalue, string newvalue)
		{
			Title = title;
			OldValue = oldvalue;
			NewValue = newvalue;
		}

		private void MakeDiffPangoMarkup()
		{
			var d = new Differ ();
			var differ = new SideBySideFullDiffBuilder(d);
			var diffRes = differ.BuildDiffModel(OldValue, NewValue);
			OldPangoText = PangoRender.RenderDiffLines (diffRes.OldText);
			NewPangoText = PangoRender.RenderDiffLines (diffRes.NewText);
			isPangoMade = true;
		}
	}
}


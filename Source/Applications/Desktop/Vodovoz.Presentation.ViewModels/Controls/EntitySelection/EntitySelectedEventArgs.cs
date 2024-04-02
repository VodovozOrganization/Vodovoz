using System;

namespace Vodovoz.Presentation.ViewModels.Controls.EntitySelection
{
	public class EntitySelectedEventArgs : EventArgs
	{
		public EntitySelectedEventArgs(object selectedObject)
		{
			SelectedObject = selectedObject ?? throw new ArgumentNullException(nameof(selectedObject));
		}

		public object SelectedObject { get; set; }
	}
}

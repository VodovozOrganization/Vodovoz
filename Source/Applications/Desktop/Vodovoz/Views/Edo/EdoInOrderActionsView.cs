using Gamma.GtkWidgets;
using QS.Views.GtkUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Vodovoz.ViewModels.Edo;
namespace Vodovoz.Views.Edo
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EdoInOrderActionsView : WidgetViewBase<EdoInOrderDocumentActionsViewModel>
	{
		private List<yButton> _yButtons = new List<yButton>();

		public EdoInOrderActionsView()
		{
			this.Build();
		}

		public override EdoInOrderDocumentActionsViewModel ViewModel
		{
			get => base.ViewModel;
			set
			{
				if(base.ViewModel != null)
				{
					base.ViewModel.PropertyChanged -= ViewModelPropertyChanged;
					DeleteButtons();
				}
				base.ViewModel = value;
				if(base.ViewModel != null)
				{
					base.ViewModel.PropertyChanged += ViewModelPropertyChanged;
					CreateButtons();
				}
			}
		}

		private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(EdoInOrderDocumentActionsViewModel.Actions))
			{
				CreateButtons();
			}
		}

		private void CreateButtons()
		{
			DeleteButtons();

			if(!ViewModel.Actions.Any())
			{
				ylabelNotSelected.Visible = true;
				return;
			}

			foreach(var action in ViewModel.Actions.Reverse())
			{
				var button = new yButton();
				button.Label = action.Name;
				button.BindCommand(action);
				yhboxButtons.PackStart(button, false, false, 1);
				button.ShowAll();

				_yButtons.Add(button);
			}

			ylabelNotSelected.Visible = false;
		}

		private void DeleteButtons()
		{
			foreach(var button in _yButtons)
			{
				yhboxButtons.Remove(button);
				button.Destroy();
			}
		}
	}
}

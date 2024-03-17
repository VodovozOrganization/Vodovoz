using Gamma.Binding.Core;
using Gtk;
using System.ComponentModel;
using Vodovoz.Presentation.ViewModels.Controls.EntitySelection;

namespace Vodovoz.ViewWidgets.GtkUI
{
	[ToolboxItem(true)]
	public partial class EntitySelection : Gtk.Bin
	{
		private IEntitySelectionViewModel _viewModel;

		public EntitySelection()
		{
			Build();

			Binding = new BindingControler<EntitySelection>(this);

			ybuttonSelectEntity.Clicked += (s, e) => OnButtonSelectEntityClicked();
			ybuttonClear.Clicked += (s, e) => OnButtonClearClicked();
		}

		public IEntitySelectionViewModel ViewModel
		{
			get => _viewModel;
			set
			{
				if (_viewModel == value)
				{
					return;
				}

				_viewModel = value;

				if(_viewModel != null)
				{
					ViewModel.PropertyChanged += ViewModel_PropertyChanged;
				}

				ybuttonSelectEntity.Sensitive = ViewModel.CanSelectEntity;
				ybuttonClear.Sensitive = ViewModel.CanClearEntity;
				SetEntryText(ViewModel.EntityTitle);
			}
		}

		public BindingControler<EntitySelection> Binding { get; private set; }

		void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(IEntitySelectionViewModel.CanSelectEntity):
					ybuttonSelectEntity.Sensitive = ViewModel.CanSelectEntity;
					break;
				case nameof(IEntitySelectionViewModel.CanClearEntity):
					ybuttonClear.Sensitive = ViewModel.CanClearEntity;
					break;
				case nameof(IEntitySelectionViewModel.EntityTitle):
					SetEntryText(ViewModel.EntityTitle);
					break;
				default:
					break;
			}
		}

		protected void OnButtonSelectEntityClicked()
		{
			ViewModel.ClearEntityCommand?.Execute();
		}

		protected void OnButtonClearClicked()
		{
			ViewModel.ClearEntityCommand?.Execute();
		}

		private void SetEntryText(string text)
		{
			yentryObject.Text = text ?? string.Empty;
			yentryObject.ModifyText(StateType.Normal);
		}
	}
}

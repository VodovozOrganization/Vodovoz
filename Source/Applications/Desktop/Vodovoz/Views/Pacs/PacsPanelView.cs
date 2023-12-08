using Gtk;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.Presentation.ViewModels.Pacs;

namespace Vodovoz.Views.Pacs
{
	[ToolboxItem(true)]
	public partial class PacsPanelView : WidgetViewBase<PacsPanelViewModel>
	{
		private IconSize _iconSize = IconSize.Dnd;

		public PacsPanelView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();
			if(ViewModel == null)
			{
				return;
			}

			buttonBreak.BindCommand(ViewModel.LongBreakCommand);
			buttonBreak.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.PacsEnabled, w => w.Visible)
				.InitializeFromSource();

			buttonRefresh.BindCommand(ViewModel.ShortBreakCommand);
			buttonRefresh.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.PacsEnabled, w => w.Visible)
				.InitializeFromSource();

			buttonPacs.BindCommand(ViewModel.OpenPacsDialogCommand);
			buttonPacs.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.PacsEnabled, w => w.Visible)
				.InitializeFromSource();

			labelPacs.Visible = false;

			
			/*labelPacs.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.PacsEnabled, w => w.Visible)
				.InitializeFromSource();*/

			buttonMango.BindCommand(ViewModel.OpenMangoDialogCommand);

			/*labelMango.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.MangoState, w => w.LabelProp)
				.InitializeFromSource();*/
			ViewModel.PropertyChanged += ViewModelPropertyChanged;
		}

		public IconSize IconSize
		{
			get => _iconSize; 
			set
			{
				if(_iconSize == value)
				{
					return;
				}

				_iconSize = value;
				UpdateLongBreakButtonImage();
				UpdateShortBreakButtonImage();
				UpdateRefreshButtonImage();
				UpdatePacsButtonImage();
				UpdateMangoButtonImage();
			}
		}

		private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(PacsPanelViewModel.BreakState):
					UpdateLongBreakButtonImage();
					UpdateShortBreakButtonImage();
					break;
				case nameof(PacsPanelViewModel.PacsState):
					UpdatePacsButtonImage();
					break;
				case nameof(PacsPanelViewModel.MangoState):
					UpdateMangoButtonImage();
					break;
			}
		}

		private void UpdateLongBreakButtonImage()
		{
			var image = new Image();

			switch(ViewModel.BreakState)
			{
				case BreakState.BreakDenied:
					image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "coffee-break-denied", GetSmallButtonsSize());
					break;
				case BreakState.CanStartBreak:
					image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "coffee-break-allowed", GetSmallButtonsSize());
					break;
				case BreakState.CanEndBreak:
					image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "coffee-break-default", GetSmallButtonsSize());
					break;
				default:
					throw new InvalidOperationException("Неизвестное состояние кнопки перерыва");
			}

			this.buttonBreak.Image.Destroy();
			this.buttonBreak.Image = image;
		}

		private void UpdateShortBreakButtonImage()
		{
			var image = new Image();

			switch(ViewModel.BreakState)
			{
				case BreakState.BreakDenied:
					image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "coffee-break-denied", GetSmallButtonsSize());
					break;
				case BreakState.CanStartBreak:
					image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "coffee-break-allowed", GetSmallButtonsSize());
					break;
				case BreakState.CanEndBreak:
					image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "coffee-break-default", GetSmallButtonsSize());
					break;
				default:
					throw new InvalidOperationException("Неизвестное состояние кнопки перерыва");
			}

			this.buttonRefresh.Image.Destroy();
			this.buttonRefresh.Image = image;
		}

		private void UpdateRefreshButtonImage()
		{
			var image = new Image();
			image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-refresh", GetSmallButtonsSize());
			
			this.buttonRefresh.Image.Destroy();
			this.buttonRefresh.Image = image;
		}

		private IconSize GetSmallButtonsSize()
		{
			switch(_iconSize)
			{
				case IconSize.Dialog:
					return IconSize.LargeToolbar;
				case IconSize.Dnd:
				case IconSize.LargeToolbar:
					return IconSize.SmallToolbar;
				case IconSize.SmallToolbar:
				case IconSize.Menu:
				case IconSize.Button:
				case IconSize.Invalid:
				default:
					return IconSize.Menu;
			}
		}

		private void UpdatePacsButtonImage()
		{
			var image = new Image();

			switch(ViewModel.PacsState)
			{
				case PacsState.Disconnected:
					image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "pacs-disabled", _iconSize);
					break;
				case PacsState.Break:
					image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "coffee-break-active", _iconSize);
					break;
				case PacsState.Talk:
				case PacsState.Connected:
				case PacsState.WorkShift:
					image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "pacs-active", _iconSize);
					break;
				default:
					throw new InvalidOperationException("Неизвестное состояние кнопки скуд");
			}

			this.buttonPacs.Image.Destroy();
			this.buttonPacs.Image = image;
		}

		private void UpdateMangoButtonImage()
		{
			var image = new Image();

			switch(ViewModel.MangoState)
			{
				case MangoState.Disable:
					image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "phone-disable", _iconSize);
					break;
				case MangoState.Disconnected:
					image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "phone-disconnected", _iconSize);
					break;
				case MangoState.Connected:
					image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "phone-connected", _iconSize);
					break;
				case MangoState.Ring:
					image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "phone-ring", _iconSize);
					break;
				case MangoState.Talk:
					image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "phone-talk", _iconSize);
					break;
				default:
					throw new InvalidOperationException("Неизвестное состояние кнопки манго");
			}

			this.buttonMango.Image.Destroy();
			this.buttonMango.Image = image;
		}
	}
}

using Gtk;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.Application.Pacs;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.Infrastructure.Converters;
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

			buttonLongBreak.HasTooltip = true;
			buttonLongBreak.TooltipText = "Большой перерыв";
			buttonLongBreak.BindCommand(ViewModel.LongBreakCommand);
			buttonLongBreak.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.IsOperator, w => w.Visible)
				.InitializeFromSource();

			buttonShortBreak.HasTooltip = true;
			buttonShortBreak.TooltipText = "Малый перерыв";
			buttonShortBreak.BindCommand(ViewModel.ShortBreakCommand);
			buttonShortBreak.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.IsOperator, w => w.Visible)
				.InitializeFromSource();

			buttonPacs.HasTooltip = true;
			buttonPacs.TooltipText = "Открыть диалог СКУД";
			buttonPacs.BindCommand(ViewModel.OpenPacsDialogCommand);
			buttonPacs.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.PacsEnabled, w => w.Visible)
				.InitializeFromSource();

			labelPacs.UseMarkup = true;
			labelPacs.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.PacsInfo, w => w.LabelProp, new TextToColoredTextConverter(GetPacsLabelColor))
				.InitializeFromSource();

			buttonMango.HasTooltip = true;
			buttonMango.TooltipText = "Открыть диалог звонков Манго";
			buttonMango.BindCommand(ViewModel.OpenMangoDialogCommand);
			labelMango.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.MangoPhone, w => w.LabelProp)
				.InitializeFromSource();
			ViewModel.PropertyChanged += ViewModelPropertyChanged;

			UpdateLongBreakButtonImage();
			UpdateShortBreakButtonImage();
			UpdatePacsButtonImage();
			UpdateMangoButtonImage();
		}

		private string GetPacsLabelColor()
		{
			return ViewModel.BreakTimeGone ? GdkColors.DangerText.ToHtmlColor() : GdkColors.PrimaryText.ToHtmlColor();
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
				UpdatePacsButtonImage();
				UpdateMangoButtonImage();
			}
		}

		private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(PacsPanelViewModel.LongBreakState):
					UpdateLongBreakButtonImage();
					break;
				case nameof(PacsPanelViewModel.ShortBreakState):
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

			switch(ViewModel.LongBreakState)
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

			this.buttonLongBreak.Image.Destroy();
			this.buttonLongBreak.Image = image;
		}

		private void UpdateShortBreakButtonImage()
		{
			var image = new Image();

			switch(ViewModel.ShortBreakState)
			{
				case BreakState.BreakDenied:
					image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "short-break-denied", GetSmallButtonsSize());
					break;
				case BreakState.CanStartBreak:
					image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "short-break-allowed", GetSmallButtonsSize());
					break;
				case BreakState.CanEndBreak:
					image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "short-break-default", GetSmallButtonsSize());
					break;
				default:
					throw new InvalidOperationException("Неизвестное состояние кнопки перерыва");
			}

			this.buttonShortBreak.Image.Destroy();
			this.buttonShortBreak.Image = image;
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

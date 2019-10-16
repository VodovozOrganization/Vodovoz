using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using Gamma.GtkWidgets;
using Gdk;
using Gtk;
using Image = Gtk.Image;

namespace Vodovoz.ViewWidgets
{
	[ToolboxItem(true)]
	[Category("QsProjectsLib")]
	public class NullableCheckButton: yButton, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public BindingControler<NullableCheckButton> Binding { get; private set; }

		public RenderMode RenderMode { get; set; } = RenderMode.Symbol;

		private Pixbuf noIcon;
		public virtual Pixbuf NoIcon {
			get {
				if(noIcon == null)
					noIcon = new Pixbuf("Vodovoz.icons.buttons.NullableCheckBox.close-button.png").ScaleSimple(50, 50, InterpType.Nearest);
				return noIcon;
			}
			set => noIcon = value;
		}

		private Pixbuf okIcon;
		public virtual Pixbuf OkIcon {
			get {
				if(okIcon == null)
					okIcon = new Pixbuf("Vodovoz.icons.buttons.NullableCheckBox.check-symbol.png").ScaleSimple(50, 50, InterpType.Nearest);
				return okIcon;
			}
			set => okIcon = value;
		}

		private Pixbuf nullIcon;
		public virtual Pixbuf NullIcon {
			get {
				if(nullIcon == null)
					nullIcon = new Pixbuf("Vodovoz.icons.buttons.NullableCheckBox.blank-square.png").ScaleSimple(50, 50, InterpType.Nearest);
				return nullIcon;
			}
			set => nullIcon = value;
		}


		private bool? active;
		public virtual bool? Active {
			get { return active; }
			set 
			{ 
				active = value;

				if(active == null)
					NullValueConfigure();
				else if(active.Value)
					OkValueConfigure();
				else 
					NoValueConfigure();

				Binding.FireChange(x => x.Active);
			}
		}

		public NullableCheckButton()
		{
			Binding = new BindingControler<NullableCheckButton>(this, new Expression<Func<NullableCheckButton, object>>[] {
				(w => w.Active)
			});
			BorderWidth = 0;
			this.Relief = ReliefStyle.None;
		}

		#region valueConfig

		private void NoValueConfigure()
		{
			if(RenderMode == RenderMode.Icon) {
				Image = new Image(NoIcon);
				Label = null;
			} else if(RenderMode == RenderMode.Symbol) {
				Image = null;
				Label = "☒";
			}
		}

		private void OkValueConfigure()
		{
			if(RenderMode == RenderMode.Icon) {
				Image = new Image(OkIcon);
				Label = null;
			} else if(RenderMode == RenderMode.Symbol) {
				Image = null;
				Label = "☑";
			}
		}

		private void NullValueConfigure()
		{
			if(RenderMode == RenderMode.Icon) {
				Image = new Image(NullIcon);
				Label = null;
			} else if(RenderMode == RenderMode.Symbol) {
				Image = null;
				Label = "☐";
			}
		}

		#endregion valueConfig

		protected override void OnClicked()
		{
			Active = Active == null ? false : (Active.Value ? null : (bool?)true);
			base.OnClicked();
		}
	}

	public enum RenderMode
	{
		Symbol,
		Icon
	}
}

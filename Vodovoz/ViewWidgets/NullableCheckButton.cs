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
	public class NullableCheckButton : yButton
	{
		public BindingControler<NullableCheckButton> Binding { get; private set; }


		private RenderMode renderMode = RenderMode.Symbol;
		public RenderMode RenderMode { get => renderMode; set { renderMode = value; ConfigureValue(); } }

		private Pixbuf noIcon;
		public virtual Pixbuf NoIcon {
			get {
				if(noIcon == null)
					noIcon = new Pixbuf(System.Reflection.Assembly.GetExecutingAssembly(), "Vodovoz.icons.buttons.NullableCheckBox.close-button.png").ScaleSimple(13, 13, InterpType.Bilinear);
				return noIcon;
			}
			set => noIcon = value;
		}

		private Pixbuf okIcon;
		public virtual Pixbuf OkIcon {
			get {
				if(okIcon == null)
					okIcon = new Pixbuf(System.Reflection.Assembly.GetExecutingAssembly(), "Vodovoz.icons.buttons.NullableCheckBox.check-symbol.png").ScaleSimple(13, 13, InterpType.Bilinear);
				return okIcon;
			}
			set => okIcon = value;
		}

		private Pixbuf nullIcon;
		public virtual Pixbuf NullIcon {
			get {
				if(nullIcon == null)
					nullIcon = new Pixbuf(System.Reflection.Assembly.GetExecutingAssembly(), "Vodovoz.icons.buttons.NullableCheckBox.blank-square.png").ScaleSimple(13, 13, InterpType.Bilinear);
				return nullIcon;
			}
			set => nullIcon = value;
		}


		private bool? active;

		public virtual bool? Active {
			get { return active; }
			set {
				active = value;
				ConfigureValue();

				Binding.FireChange(x => x.Active);
			}
		}

		private void ConfigureValue()
		{
			if(active == null)
				NullValueConfigure();
			else if(active.Value)
				OkValueConfigure();
			else
				NoValueConfigure();
		}

		public NullableCheckButton()
		{
			Binding = new BindingControler<NullableCheckButton>(this, new Expression<Func<NullableCheckButton, object>>[] {
				(w => w.Active)
			});
			BorderWidth = 0;
			this.Relief = ReliefStyle.None;
			ConfigureValue();
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

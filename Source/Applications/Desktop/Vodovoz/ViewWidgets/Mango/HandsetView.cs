using System;
using Vodovoz.Domain.Contacts;
using System.Linq.Expressions;
using Gamma.Binding.Core;

namespace Vodovoz.ViewWidgets.Mango
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class HandsetView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		private Phone _phone;
		public BindingControler<HandsetView> Binding { get; private set; }

		public HandsetView(string number)
		{
			this.Build();

			Binding = new BindingControler<HandsetView>(this, new Expression<Func<HandsetView, object>>[] { w => w.Sensitive });
			SetPhone(number);
		}

		public void SetPhone(string number)
		{
			_phone = new Phone
			{
				Number = number
			};
		}

		protected void Clicked_buttonMakeCall(object sender, EventArgs e)
		{
			if(_phone.DigitsNumber.Length == 10)
				Startup.MainWin.MangoManager.MakeCall("7"+ _phone.DigitsNumber);
		}

		protected virtual void OnChanged()
		{
			Binding.FireChange(w => w.Sensitive);
		}
	}
}

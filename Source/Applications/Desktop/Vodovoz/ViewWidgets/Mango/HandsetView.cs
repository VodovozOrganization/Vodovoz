using System;
using Vodovoz.Domain.Contacts;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using Vodovoz.ViewModels.Dialogs.Mango;
using System.ComponentModel;
using Vodovoz.Application.Mango;

namespace Vodovoz.ViewWidgets.Mango
{
	[ToolboxItem(true)]
	public partial class HandsetView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		private MangoManager _mangoManager;
		private Phone _phone;
		public BindingControler<HandsetView> Binding { get; private set; }

		public HandsetView()
		{
			Build();

			Binding = new BindingControler<HandsetView>(this, new Expression<Func<HandsetView, object>>[] { w => w.Sensitive });
			_mangoManager = Startup.MainWin.MangoManager;
			if(_mangoManager != null)
			{
				_mangoManager.PropertyChanged += MangoManagerPropertyChanged;
			}
		}

		public HandsetView(string number) : this()
		{
			SetPhone(number);
		}

		private void MangoManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(MangoManager.ConnectionState):
					Gtk.Application.Invoke(delegate {
						buttonMakeCall.Sensitive = _mangoManager.IsActive;
					});
					break;
				default:
					break;
			}
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
			if(_mangoManager == null
				|| _phone is null
				|| _mangoManager.ConnectionState == ConnectionState.Disconnected)
			{
				return;
			}

			if(_phone.DigitsNumber.Length == 10)
			{
				_mangoManager.MakeCall("7" + _phone.DigitsNumber);
			}
		}

		protected virtual void OnChanged()
		{
			Binding.FireChange(w => w.Sensitive);
		}
	}
}

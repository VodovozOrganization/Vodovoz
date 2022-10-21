using System;
using System.Linq;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using QS.Widgets;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Infrastructure.Mango;

namespace Vodovoz.ViewWidgets.Mango
{
	public class EmployeePhone : MenuButton
	{
		private Employee _employee;

		public BindingControler<EmployeePhone> Binding { get; private set; }

		public EmployeePhone()
		{
			global::Gtk.Image image = new global::Gtk.Image();
			image.Pixbuf = global::Gdk.Pixbuf.LoadFromResource("Vodovoz.icons.phone.make-call-16.png");
			Image = image;
			Sensitive = false;

			Binding = new BindingControler<EmployeePhone>(this, new Expression<Func<EmployeePhone, object>>[] {});
		}

		public MangoManager MangoManager;

		public Employee Employee {
			get => _employee; set {
				_employee = value;

				if(Employee != null && Employee.Phones.Any()) {
					Sensitive = MangoManager != null && MangoManager.ConnectionState != ConnectionState.Disable;
					Menu = new Gtk.Menu();
					foreach(var phone in Employee.Phones) {
						var item = new QSWidgetLib.MenuItemId<Phone>(phone.LongText);
						item.ID = phone;
						item.Activated += Item_Activated;
						Menu.Add(item);
					}
					Menu.ShowAll();
				} else
					Sensitive = false;
			}
		}

		void Item_Activated(object sender, EventArgs e)
		{
			var item = sender as QSWidgetLib.MenuItemId<Phone>;
			MangoManager.MakeCall("+7" + item.ID.DigitsNumber);
		}
	}
}

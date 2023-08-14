using Gtk;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Validation;
using Vodovoz.Domain.Client;

namespace Vodovoz.Dialogs.Client
{
	public partial class CounterpartyActivityKindDlg : EntityDialogBase<CounterpartyActivityKind>
	{
		public CounterpartyActivityKindDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<CounterpartyActivityKind>();
			ConfigureDlg();
		}

		public CounterpartyActivityKindDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<CounterpartyActivityKind>(id);
			ConfigureDlg();
		}

		public CounterpartyActivityKindDlg(CounterpartyActivityKind entity) : this(entity.Id) { }

		public override bool Save()
		{
			var validator = new ObjectValidator(new GtkValidationViewFactory());
			if(!validator.Validate(Entity))
			{
				return false;
			}
			UoWGeneric.Save();
			return true;
		}

		void ConfigureDlg()
		{
			entName.Binding.AddBinding(Entity, x => x.Name, x => x.Text).InitializeFromSource();
			txtSubstrings.Binding.AddBinding(Entity, e => e.Substrings, w => w.Buffer.Text).InitializeFromSource();
		}
	}
}

using System;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Store;
using Vodovoz.Domain.Goods;
using Vodovoz.Repository.Store;

namespace Vodovoz
{
	public partial class IncomingWaterDlg : OrmGtkDialogBase<IncomingWater>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public IncomingWaterDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<IncomingWater>();
			Entity.Author = Repository.EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			if(Entity.Author == null) {
				MessageDialogWorks.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}

			Warehouse productionWarehouse = WarehouseRepository.DefaultWarehouseForProduction(UoWGeneric);

			if(QSMain.User.Permissions["production"] && productionWarehouse != null) {
				Entity.WriteOffWarehouse = Entity.IncomingWarehouse = productionWarehouse;
			}

			ConfigureDlg();
		}

		public IncomingWaterDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<IncomingWater>(id);
			ConfigureDlg();
		}

		public IncomingWaterDlg(IncomingWater sub) : this(sub.Id)
		{
		}

		void ConfigureDlg()
		{
			labelTimeStamp.Binding.AddBinding(Entity, e => e.DateString, w => w.LabelProp).InitializeFromSource();
			spinAmount.Binding.AddBinding(Entity, e => e.Amount, w => w.ValueAsInt).InitializeFromSource();

			referenceProduct.SubjectType = typeof(Nomenclature);
			referenceProduct.Binding.AddBinding(Entity, e => e.Product, w => w.Subject).InitializeFromSource();
			referenceSrcWarehouse.SubjectType = typeof(Warehouse);
			referenceSrcWarehouse.Binding.AddBinding(Entity, e => e.WriteOffWarehouse, w => w.Subject).InitializeFromSource();
			referenceDstWarehouse.SubjectType = typeof(Warehouse);
			referenceDstWarehouse.Binding.AddBinding(Entity, e => e.IncomingWarehouse, w => w.Subject).InitializeFromSource();

			if(QSMain.User.Permissions["production"] && WarehouseRepository.DefaultWarehouseForProduction(UoWGeneric) != null) {
				referenceSrcWarehouse.Sensitive = false;
				referenceDstWarehouse.Sensitive = false;
			}


			incomingwatermaterialview1.DocumentUoW = UoWGeneric;
		}

		public override bool Save ()
		{
			var valid = new QSValidator<IncomingWater> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			Entity.LastEditor = Repository.EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditor == null)
			{
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			logger.Info ("Сохраняем документ производства...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			return true;
		}

		protected void OnButtonFillClicked (object sender, EventArgs e)
		{
			OrmReference SelectDialog = new OrmReference (typeof(ProductSpecification), UoWGeneric);
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ObjectSelected += SelectDialog_ObjectSelected;

			TabParent.AddSlaveTab (this, SelectDialog);
		}

		void SelectDialog_ObjectSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var spec = e.Subject as ProductSpecification;
			UoWGeneric.Root.Product = spec.Product;
			UoWGeneric.Root.ObservableMaterials.Clear ();
			foreach (var material in spec.Materials) {
				UoWGeneric.Root.AddMaterial (material);
			}
		}
	}
}


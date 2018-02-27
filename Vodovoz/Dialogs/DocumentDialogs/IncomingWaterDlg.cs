using System;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Additions.Store;
using Vodovoz.Core.Permissions;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;

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

			Entity.IncomingWarehouse = StoreDocumentHelper.GetDefaultWarehouse(UoW, WarehousePermissions.IncomingWaterEdit);
			Entity.WriteOffWarehouse = StoreDocumentHelper.GetDefaultWarehouse(UoW, WarehousePermissions.IncomingWaterEdit);

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
			if(StoreDocumentHelper.CheckAllPermissions(UoW.IsNew, WarehousePermissions.IncomingWaterEdit, Entity.IncomingWarehouse, Entity.WriteOffWarehouse)) {
				FailInitialize = true;
				return;
			}

			var editing = StoreDocumentHelper.CanEditDocument(WarehousePermissions.IncomingWaterEdit, Entity.IncomingWarehouse, Entity.WriteOffWarehouse);
			buttonFill.Sensitive = referenceProduct.IsEditable = spinAmount.Sensitive
				= referenceSrcWarehouse.IsEditable = referenceDstWarehouse.IsEditable = editing;
			incomingwatermaterialview1.Sensitive = editing;

			labelTimeStamp.Binding.AddBinding(Entity, e => e.DateString, w => w.LabelProp).InitializeFromSource();
			spinAmount.Binding.AddBinding(Entity, e => e.Amount, w => w.ValueAsInt).InitializeFromSource();

			referenceProduct.SubjectType = typeof(Nomenclature);
			referenceProduct.Binding.AddBinding(Entity, e => e.Product, w => w.Subject).InitializeFromSource();
			referenceSrcWarehouse.SubjectType = typeof(Warehouse);
			referenceSrcWarehouse.Binding.AddBinding(Entity, e => e.WriteOffWarehouse, w => w.Subject).InitializeFromSource();
			referenceDstWarehouse.SubjectType = typeof(Warehouse);
			referenceDstWarehouse.Binding.AddBinding(Entity, e => e.IncomingWarehouse, w => w.Subject).InitializeFromSource();

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


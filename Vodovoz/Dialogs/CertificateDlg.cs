using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.JournalFilters;
using Vodovoz.ViewModel;

namespace Vodovoz.Dialogs
{
	public partial class CertificateDlg : QS.Dialog.Gtk.EntityDialogBase<Certificate>
	{
		Nomenclature selectedNomenclature;
		GenericObservableList<Nomenclature> ObservableList { get; set; }

		public CertificateDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Certificate>();
			TabName = "Новый сертификат";
			ConfigureDlg();
		}

		public CertificateDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Certificate>(id);
			ConfigureDlg();
		}

		public CertificateDlg(Certificate sub) : this(sub.Id) { }

		void ConfigureDlg()
		{
			yEntryName.Binding.AddBinding(Entity, s => s.Name, w => w.Text).InitializeFromSource();
			yEnumCmbType.ItemsEnum = typeof(CertificateType);
			yEnumCmbType.Binding.AddBinding(Entity, s => s.TypeOfCertificate, w => w.SelectedItemOrNull).InitializeFromSource();
			yEnumCmbType.EnumItemSelected += YEnumCmbType_EnumItemSelected;
			photoViewCertificate.Binding.AddBinding(Entity, e => e.ImageFile, w => w.ImageFile).InitializeFromSource();
			photoViewCertificate.GetSaveFileName = () => Entity.Name;
			photoViewCertificate.CanPrint = true;
			yStartDate.Binding.AddBinding(Entity, s => s.StartDate, w => w.DateOrNull).InitializeFromSource();
			yDateOfExpiration.Binding.AddBinding(Entity, s => s.ExpirationDate, w => w.DateOrNull).InitializeFromSource();
			yChkIsArchive.Binding.AddBinding(Entity, s => s.IsArchive, w => w.Active).InitializeFromSource();
			YEnumCmbType_EnumItemSelected(this, new Gamma.Widgets.ItemSelectedEventArgs(yEnumCmbType.SelectedItem));
			ObservableList = Entity.ObservableNomenclatures;
			yTreeNomenclatures.Selection.Changed += (sender, e) => {
				selectedNomenclature = yTreeNomenclatures.GetSelectedObject<Nomenclature>();
				SetControlsAcessibility();
			};
			yTreeNomenclatures.ColumnsConfig = new FluentColumnsConfig<Nomenclature>()
				.AddColumn("Имя")
					.AddTextRenderer(n => n.Name)
				.AddColumn("Код")
					.AddTextRenderer(n => n.Id.ToString())
				.Finish();
			yTreeNomenclatures.ItemsDataSource = ObservableList;
		}

		public override bool Save()
		{
			var valid = new QSValidator<Certificate>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;
			UoWGeneric.Save();
			return true;
		}

		protected void OnBtnDeleteActivated(object sender, System.EventArgs e)
		{
			if(selectedNomenclature != null)
				ObservableList.Remove(selectedNomenclature);
		}

		protected void OnBtnAddNomenclatureClicked(object sender, System.EventArgs e)
		{
			var nomenclatureFilter = new NomenclatureRepFilter(UoWGeneric);
			nomenclatureFilter.SetAndRefilterAtOnce(
				x => x.AvailableCategories = Nomenclature.GetCategoriesForSaleToOrder(),
				x => x.DefaultSelectedCategory = NomenclatureCategory.water,
				x => x.DefaultSelectedSubCategory = SubtypeOfEquipmentCategory.forSale
			);
			ReferenceRepresentation SelectDialog = new ReferenceRepresentation(new ViewModel.NomenclatureForSaleVM(nomenclatureFilter)) {
				Mode = OrmReferenceMode.MultiSelect,
				TabName = "Номенклатура на продажу",
				ShowFilter = true
			};
			SelectDialog.ObjectSelected += (s, ea) => {
				var nomenclaturesForSaleVMNode = ea.Selected.Select(o => o.VMNode as NomenclatureForSaleVMNode);
				foreach(var nomenclatureForSaleVMNode in nomenclaturesForSaleVMNode) {
					Nomenclature n = UoWGeneric.GetById<Nomenclature>(nomenclatureForSaleVMNode.Id);
					if(n != null && !Entity.ObservableNomenclatures.Any(x => x == n))
						Entity.ObservableNomenclatures.Add(n);
				}
			};
			TabParent.AddSlaveTab(this, SelectDialog);
		}

		void SetControlsAcessibility()
		{
			btnDelete.Sensitive = selectedNomenclature != null;
		}

		void YEnumCmbType_EnumItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			bool isCertificateForNomenclatures = Entity.TypeOfCertificate.HasValue && Entity.TypeOfCertificate.Value == CertificateType.Nomenclature;
			if(!isCertificateForNomenclatures)
				Entity.ObservableNomenclatures.Clear();
			lblNomenclatures.Markup = string.Format("<span foreground='{0}'><b>Номенклатуры</b></span>", isCertificateForNomenclatures ? "black" : "grey");
			vbxNomenclatures.Sensitive = isCertificateForNomenclatures;
		}
	}
}
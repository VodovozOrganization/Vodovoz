﻿using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using QS.DomainModel.UoW;
using QS.Project.Dialogs;
using QS.Project.Dialogs.GtkUI;
using QSOrmProject;
using QS.Validation;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.JournalFilters;
using Vodovoz.ViewModel;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;

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

		protected void OnBtnDeleteClicked(object sender, System.EventArgs e)
		{
			if(selectedNomenclature != null)
				ObservableList.Remove(selectedNomenclature);
		}

		protected void OnBtnAddNomenclatureClicked(object sender, System.EventArgs e)
		{
			var filter = new NomenclatureFilterViewModel();
			filter.SetAndRefilterAtOnce(
				x => x.AvailableCategories = Nomenclature.GetCategoriesForSaleToOrder(),
				x => x.SelectCategory = NomenclatureCategory.water,
				x => x.SelectSaleCategory = SaleCategory.forSale
			);

			var nomenclatureJournalFactory = new NomenclatureJournalFactory();
			var journal = nomenclatureJournalFactory.CreateNomenclaturesJournalViewModel(filter, true);
			journal.OnEntitySelectedResult += JournalOnEntitySelectedResult;
			journal.Title = "Номенклатура на продажу";
			TabParent.AddSlaveTab(this, journal);
		}

		private void JournalOnEntitySelectedResult(object sender, QS.Project.Journal.JournalSelectedNodesEventArgs e)
		{
			if(!e.SelectedNodes.Any())
			{
				return;
			}

			var nomenclatures = UoWGeneric.GetById<Nomenclature>(e.SelectedNodes.Select(x => x.Id));
			foreach(var nomenclature in nomenclatures)
			{
				if(!Entity.ObservableNomenclatures.Any(x => x == nomenclature))
				{
					Entity.ObservableNomenclatures.Add(nomenclature);
				}
			}
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

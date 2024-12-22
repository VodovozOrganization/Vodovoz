using System;
using Gamma.ColumnConfig;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Navigation;
using QS.Project.Journal;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using QS.Project.Services;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;

namespace Vodovoz.Dialogs
{
	public partial class CertificateDlg : QS.Dialog.Gtk.EntityDialogBase<Certificate>
	{
		Nomenclature selectedNomenclature;
		GenericObservableList<Nomenclature> ObservableList { get; set; }

		private readonly string _primaryTextHtmlColor = GdkColors.PrimaryText.ToHtmlColor();
		private readonly string _insensitiveTextHtmlColor = GdkColors.InsensitiveText.ToHtmlColor();

		public CertificateDlg()
		{
			this.Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<Certificate>();
			TabName = "Новый сертификат";
			ConfigureDlg();
		}

		public CertificateDlg(int id)
		{
			this.Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<Certificate>(id);
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
			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(Entity))
			{
				return false;
			}
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
			var journal =
				Startup.MainWin.NavigationManager.OpenViewModelOnTdi<NomenclaturesJournalViewModel, Action<NomenclatureFilterViewModel>>(
					this,
					filter =>
					{
						filter.AvailableCategories = Nomenclature.GetCategoriesForSaleToOrder();
						filter.SelectCategory = NomenclatureCategory.water;
						filter.SelectSaleCategory = SaleCategory.forSale;
					},
					OpenPageOptions.AsSlave,
					vm =>
					{
						vm.SelectionMode = JournalSelectionMode.Multiple;
						vm.Title = "Номенклатура на продажу";
						vm.OnSelectResult += JournalOnEntitySelectedResult;
					}
				).ViewModel;
		}

		private void JournalOnEntitySelectedResult(object sender, JournalSelectedEventArgs e)
		{
			var selectedNodes = e.SelectedObjects.Cast<NomenclatureJournalNode>();

			if(!selectedNodes.Any())
			{
				return;
			}

			var nomenclatures = UoWGeneric.GetById<Nomenclature>(selectedNodes.Select(x => x.Id));
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
			{
				Entity.ObservableNomenclatures.Clear();
			}

			lblNomenclatures.Markup = string.Format("<span foreground='{0}'><b>Номенклатуры</b></span>", isCertificateForNomenclatures ? _primaryTextHtmlColor : _insensitiveTextHtmlColor);
			vbxNomenclatures.Sensitive = isCertificateForNomenclatures;
		}
	}
}

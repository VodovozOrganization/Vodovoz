using Gamma.GtkWidgets;
using NHibernate.Transform;
using QS.Views.GtkUI;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Retail;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.ViewModels.Dialogs.Counterparty;
using System.Linq;

namespace Vodovoz.Views.Client
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CloseSupplyToCounterpartyView : TabViewBase<CloseSupplyToCounterpartyViewModel>
	{
		public CloseSupplyToCounterpartyView(CloseSupplyToCounterpartyViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		protected void ConfigureDlg()
		{
			if(ViewModel == null)
			{
				return;
			}

			enumPersonType.ItemsEnum = typeof(PersonType);
			enumPersonType.Binding.AddBinding(ViewModel.Entity, s => s.PersonType, w => w.SelectedItemOrNull).InitializeFromSource();

			yEnumCounterpartyType.ItemsEnum = typeof(CounterpartyType);
			yEnumCounterpartyType.Binding
				.AddBinding(ViewModel.Entity, c => c.CounterpartyType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			ycheckIsArchived.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.InitializeFromSource();

			lblVodovozNumber.LabelProp = ViewModel.Entity.VodovozInternalId.ToString();

			hboxCameFrom.Visible = (ViewModel.Entity.Id != 0 && ViewModel.Entity.CameFrom != null) || ViewModel.Entity.Id == 0;

			ySpecCmbCameFrom.SetRenderTextFunc<ClientCameFrom>(f => f.Name);

			ySpecCmbCameFrom.ItemsList = (new CounterpartyRepository()).GetPlacesClientCameFrom(
				ViewModel.UoW,
				ViewModel.Entity.CameFrom == null || !ViewModel.Entity.CameFrom.IsArchive
			);

			ySpecCmbCameFrom.Binding
				.AddBinding(ViewModel.Entity, f => f.CameFrom, w => w.SelectedItem)
				.InitializeFromSource();

			ycheckIsForRetail.Binding
				.AddBinding(ViewModel.Entity, e => e.IsForRetail, w => w.Active)
				.InitializeFromSource();

			ycheckIsForSalesDepartment.Binding
				.AddBinding(ViewModel.Entity, e => e.IsForSalesDepartment, w => w.Active)
				.InitializeFromSource();

			ycheckNoPhoneCall.Binding
				.AddBinding(ViewModel.Entity, e => e.NoPhoneCall, w => w.Active)
				.InitializeFromSource();
			ycheckNoPhoneCall.Visible = ViewModel.Entity.IsForRetail;

			DelayDaysForBuyerValue.Binding
				.AddBinding(ViewModel.Entity, e => e.DelayDaysForBuyers, w => w.ValueAsInt)
				.InitializeFromSource();

			yspinDelayDaysForTechProcessing.Binding
				.AddBinding(ViewModel.Entity, e => e.TechnicalProcessingDelay, w => w.ValueAsInt)
				.InitializeFromSource();

			entryFIO.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();

			yhboxPersonFullName.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.TypeOfOwnership == "ИП" || e.PersonType == PersonType.natural, w => w.Visible)
				.InitializeFromSource();

			ylabelPersonFullName.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.TypeOfOwnership == "ИП" || e.PersonType == PersonType.natural, w => w.Visible)
				.InitializeFromSource();

			yentrySurname.Binding
				.AddBinding(ViewModel.Entity, e => e.Surname, w => w.Text)
				.InitializeFromSource();

			yentryFirstName.Binding
				.AddBinding(ViewModel.Entity, e => e.FirstName, w => w.Text)
				.InitializeFromSource();

			yentryPatronymic.Binding
				.AddBinding(ViewModel.Entity, e => e.Patronymic, w => w.Text)
				.InitializeFromSource();

			labelFIO.Visible = entryFIO.Visible = ViewModel.Entity.PersonType == PersonType.natural;
			labelShort.Visible = labelShort1.Visible = /*comboboxOpf.Visible =*/ yentryOrganizationName.Visible =
				labelFullName.Visible = entryFullName.Visible =
				entryMainCounterparty.Visible = labelMainCounterparty.Visible =
				lblPaymentType.Visible = enumPayment.Visible = (ViewModel.Entity.PersonType == PersonType.legal);

			//FillComboboxOpf();
			//comboboxOpf. = ViewModel.UoW.GetAll<OrganizationOwnershipType>();
			//var currentOwnershipType = Entity.TypeOfOwnership;

			//while(GetAllComboboxOpfValues().Count() > 0)
			//{
			//	comboboxOpf.RemoveText(0);
			//}

			//comboboxOpf.AppendText("");

			//foreach(var ownershipType in availableOrganizationOwnershipTypes)
			//{
			//	comboboxOpf.AppendText(ownershipType);
			//}

			//Entity.TypeOfOwnership = SetActiveComboboxOpfValue(currentOwnershipType) ? currentOwnershipType : String.Empty;
			//GetAllOrganizationOwnershipTypes()
			//		.Where(t => !t.IsArchive || (Entity.TypeOfOwnership != null && t.Abbreviation == Entity.TypeOfOwnership))
			//		.Select(t => t.Abbreviation)
			//		.ToList<string>();

			//yentryOrganizationName.Sensitive = _currentUserCanEditCounterpartyDetails && CanEdit;
			yentryOrganizationName.Binding.AddBinding(ViewModel.Entity, s => s.Name, t => t.Text).InitializeFromSource();
			yentryOrganizationName.Binding.AddFuncBinding(ViewModel.Entity, s => s.TypeOfOwnership != "ИП", t => t.Sensitive).InitializeFromSource();

			entryFullName.Binding
				.AddBinding(ViewModel.Entity, e => e.FullName, w => w.Text)
				.AddFuncBinding(s => s.TypeOfOwnership != "ИП", w => w.Sensitive)
				.InitializeFromSource();

			//entryMainCounterparty
			//	.SetEntityAutocompleteSelectorFactory(CounterpartySelectorFactory.CreateCounterpartyAutocompleteSelectorFactory());
			entryMainCounterparty.Binding
				.AddBinding(ViewModel.Entity, e => e.MainCounterparty, w => w.Subject)
				.InitializeFromSource();

			//entryPreviousCounterparty
			//	.SetEntityAutocompleteSelectorFactory(CounterpartySelectorFactory.CreateCounterpartyAutocompleteSelectorFactory());
			entryPreviousCounterparty.Binding
				.AddBinding(ViewModel.Entity, e => e.PreviousCounterparty, w => w.Subject)
				.InitializeFromSource();

			enumPayment.ItemsEnum = typeof(PaymentType);
			enumPayment.Binding
				.AddBinding(ViewModel.Entity, s => s.PaymentMethod, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			enumDefaultDocumentType.ItemsEnum = typeof(DefaultDocumentType);
			enumDefaultDocumentType.Binding
				.AddBinding(ViewModel.Entity, s => s.DefaultDocumentType, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			enumDefaultDocumentType.Visible = ViewModel.Entity.PaymentMethod == PaymentType.cashless;
			labelDefaultDocumentType.Visible = ViewModel.Entity.PaymentMethod == PaymentType.cashless;

			lblTax.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.PersonType == PersonType.legal, w => w.Visible)
				.InitializeFromSource();

			specialListCmbWorksThroughOrganization.ItemsList = ViewModel.UoW.GetAll<Domain.Organizations.Organization>();
			specialListCmbWorksThroughOrganization.Binding
				.AddBinding(ViewModel.Entity, e => e.WorksThroughOrganization, w => w.SelectedItem)
				.InitializeFromSource();

			enumTax.ItemsEnum = typeof(TaxType);

			if(ViewModel.Entity.CreateDate != null)
			{
				Enum[] hideEnums = { TaxType.None };
				enumTax.AddEnumToHideList(hideEnums);
			}

			enumTax.Binding.AddSource(ViewModel.Entity)
				.AddBinding(e => e.TaxType, w => w.SelectedItem)
				.AddFuncBinding(e => e.PersonType == PersonType.legal, w => w.Visible)
				.InitializeFromSource();

			spinMaxCredit.Binding
				.AddBinding(ViewModel.Entity, e => e.MaxCredit, w => w.ValueAsDecimal)
				.InitializeFromSource();

			dataComment.Binding
				.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text)
				.InitializeFromSource();

			// Прикрепляемые документы

			//var filesViewModel =
			//	new CounterpartyFilesViewModel(ViewModel.Entity, UoW, new FileDialogService(), ServicesConfig.CommonServices, _userRepository)
			//	{
			//		ReadOnly = !CanEdit
			//	};
			//counterpartyfilesview1.ViewModel = filesViewModel;

			chkNeedNewBottles.Binding
				.AddBinding(ViewModel.Entity, e => e.NewBottlesNeeded, w => w.Active)
				.InitializeFromSource();

			ycheckSpecialDocuments.Binding
				.AddBinding(ViewModel.Entity, e => e.UseSpecialDocFields, w => w.Active)
				.InitializeFromSource();

			ycheckAlwaysPrintInvoice.Binding
				.AddBinding(ViewModel.Entity, e => e.AlwaysPrintInvoice, w => w.Active)
				.InitializeFromSource();

			ycheckAlwaysSendReceitps.Binding
				.AddBinding(ViewModel.Entity, e => e.AlwaysSendReceipts, w => w.Active)
				.InitializeFromSource();
			//ycheckAlwaysSendReceitps.Visible =
			//	ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_manage_cash_receipts");
			//ycheckAlwaysSendReceitps.Sensitive = CanEdit;

			ycheckExpirationDateControl.Binding
				.AddBinding(ViewModel.Entity, e => e.SpecialExpireDatePercentCheck, w => w.Active)
				.InitializeFromSource();
			yspinExpirationDatePercent.Binding.AddSource(ViewModel.Entity)
				.AddBinding(e => e.SpecialExpireDatePercentCheck, w => w.Visible)
				.AddBinding(e => e.SpecialExpireDatePercent, w => w.ValueAsDecimal)
				.InitializeFromSource();

			ycheckRoboatsExclude.Binding.AddBinding(ViewModel.Entity, e => e.RoboatsExclude, w => w.Active).InitializeFromSource();

			//Настройка каналов сбыта
			if(ViewModel.Entity.IsForRetail)
			{
				ytreeviewSalesChannels.ColumnsConfig = ColumnsConfigFactory.Create<SalesChannelSelectableNode>()
					.AddColumn("Название").AddTextRenderer(node => node.Name)
					.AddColumn("").AddToggleRenderer(x => x.Selected)
					.Finish();

				SalesChannel salesChannelAlias = null;
				SalesChannelSelectableNode salesChannelSelectableNodeAlias = null;

				var salesChannels = ViewModel.UoW.Session.QueryOver(() => salesChannelAlias)
					.SelectList(scList => scList
						.SelectGroup(() => salesChannelAlias.Id).WithAlias(() => salesChannelSelectableNodeAlias.Id)
						.Select(() => salesChannelAlias.Name).WithAlias(() => salesChannelSelectableNodeAlias.Name)
					).TransformUsing(Transformers.AliasToBean<SalesChannelSelectableNode>()).List<SalesChannelSelectableNode>().ToList();

				foreach(var selectableChannel in salesChannels.Where(x => ViewModel.Entity.SalesChannels.Any(sc => sc.Id == x.Id)))
				{
					selectableChannel.Selected = true;
				}

				ytreeviewSalesChannels.ItemsDataSource = salesChannels;
			}
			else
			{
				yspinDelayDaysForTechProcessing.Visible = false;
				lblDelayDaysForTechProcessing.Visible = false;
				frame2.Visible = false;
				frame3.Visible = false;
				//label46.Visible = false;
				//label47.Visible = false;
				//label48.Visible = false;
				//label49.Visible = false;
			}
		}
	}
}

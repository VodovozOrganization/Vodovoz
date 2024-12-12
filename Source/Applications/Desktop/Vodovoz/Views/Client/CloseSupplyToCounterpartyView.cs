using Gamma.GtkWidgets;
using QS.Views.GtkUI;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.ViewModels.Dialogs.Counterparties;
using QS.Project.Services;
using QS.Navigation;
using QS.Dialog.GtkUI.FileDialog;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Views.Client
{
	public partial class CloseSupplyToCounterpartyView : TabViewBase<CloseSupplyToCounterpartyViewModel>
	{
		public CloseSupplyToCounterpartyView(CloseSupplyToCounterpartyViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		protected void ConfigureDlg()
		{
			if(ViewModel == null)
			{
				return;
			}

			yvboxMain.Sensitive = ViewModel.CanCloseDelivery;

			ConfigureCloseSupplyControls();
			ConfigureNotSensitiveControls();

			buttonSave.Clicked += (sender, args) => ViewModel.Save(true);
			buttonSave.Binding
				.AddFuncBinding(ViewModel, vm => vm.CanCloseDelivery, w => w.Sensitive)
				.InitializeFromSource();

			buttonCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);
			buttonCancel.Binding
				.AddFuncBinding(ViewModel, vm => true, w => w.Sensitive)
				.InitializeFromSource();
		}

		private void ConfigureCloseSupplyControls()
		{
			ylabelDebtType.Binding
				.AddBinding(ViewModel.Entity, e => e.IsDeliveriesClosed, l => l.Visible)
				.InitializeFromSource();

			yenumcomboboxDebtType.ItemsEnum = typeof(DebtType);
			yenumcomboboxDebtType.Binding
				.AddSource(ViewModel.Entity)
				.AddBinding(e => e.CloseDeliveryDebtType, w => w.SelectedItemOrNull)
				.AddBinding(e => e.IsDeliveriesClosed, l => l.Visible)
				.InitializeFromSource();

			labelCloseDelivery.Binding
				.AddBinding(ViewModel, vm => vm.CloseDeliveryLabelInfo, l => l.LabelProp)
				.AddBinding(ViewModel.Entity, e => e.IsDeliveriesClosed, l => l.Visible)
				.InitializeFromSource();

			yhboxComment.Binding
				.AddBinding(ViewModel.Entity, e => e.IsDeliveriesClosed, s => s.Visible)
				.InitializeFromSource();

			ytextviewCloseComment.Binding
				.AddFuncBinding(ViewModel.Entity, e => string.IsNullOrWhiteSpace(e.CloseDeliveryComment), t => t.Sensitive)
				.AddBinding(ViewModel, vm => vm.CloseDeliveryComment, w => w.Buffer.Text)
				.InitializeFromSource();

			buttonSaveCloseComment.Binding
				.AddSource(ViewModel.Entity)
				.AddFuncBinding(e => string.IsNullOrWhiteSpace(e.CloseDeliveryComment), b => b.Sensitive)
				.AddBinding(e => e.IsDeliveriesClosed, b => b.Visible)
				.InitializeFromSource();
			buttonSaveCloseComment.Clicked += (s, e) => ViewModel.SaveCloseCommentCommand.Execute();

			buttonEditCloseDeliveryComment.Binding
				.AddSource(ViewModel.Entity)
				.AddFuncBinding(e => !string.IsNullOrWhiteSpace(e.CloseDeliveryComment), b => b.Sensitive)
				.AddBinding(e => e.IsDeliveriesClosed, b => b.Visible)
				.InitializeFromSource();
			buttonEditCloseDeliveryComment.Clicked += (s, e) => ViewModel.EditCloseCommentCommand.Execute();

			buttonCloseDelivery.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.IsDeliveriesClosed ? "Открыть поставки" : "Закрыть поставки", l => l.Label)
				.InitializeFromSource();
			buttonCloseDelivery.Clicked += (s, e) => ViewModel.CloseDeliveryCommand.Execute();
		}

		private void ConfigureNotSensitiveControls()
		{
			enumPersonType.ItemsEnum = typeof(PersonType);
			enumPersonType.Binding.AddBinding(ViewModel.Entity, s => s.PersonType, w => w.SelectedItemOrNull).InitializeFromSource();

			yEnumCounterpartyType.ItemsEnum = typeof(CounterpartyType);
			yEnumCounterpartyType.Binding
				.AddBinding(ViewModel.Entity, c => c.CounterpartyType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			ycheckIsArchived.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.InitializeFromSource();

			lblVodovozNumber.Visible = false;

			hboxCameFrom.Visible = (ViewModel.Entity.Id != 0 && ViewModel.Entity.CameFrom != null) || ViewModel.Entity.Id == 0;

			ySpecCmbCameFrom.SetRenderTextFunc<ClientCameFrom>(f => f.Name);
			ySpecCmbCameFrom.ItemsList = ViewModel.ClientCameFromPlaces;
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
				.AddSource(ViewModel.Entity)
				.AddBinding(e => e.TechnicalProcessingDelay, w => w.ValueAsInt)
				.AddBinding(e => e.IsForRetail, w => w.Sensitive)
				.InitializeFromSource();

			lblDelayDaysForTechProcessing.Visible = ViewModel.Entity.IsForRetail;

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
			labelShort.Visible = labelShort1.Visible = ySpecCmbOpf.Visible = yentryOrganizationName.Visible =
				labelFullName.Visible = entryFullName.Visible =
				entryMainCounterparty.Visible = labelMainCounterparty.Visible =
				lblPaymentType.Visible = enumPayment.Visible = (ViewModel.Entity.PersonType == PersonType.legal);

			ySpecCmbOpf.ItemsList = ViewModel.AllOrganizationOwnershipTypesAbbreviations;
			ySpecCmbOpf.Binding
				.AddBinding(ViewModel.Entity, f => f.TypeOfOwnership, w => w.SelectedItem)
				.InitializeFromSource();

			yentryOrganizationName.Binding.AddBinding(ViewModel.Entity, s => s.Name, t => t.Text).InitializeFromSource();
			yentryOrganizationName.Binding.AddFuncBinding(ViewModel.Entity, s => s.TypeOfOwnership != "ИП", t => t.Sensitive).InitializeFromSource();

			entryFullName.Binding
				.AddBinding(ViewModel.Entity, e => e.FullName, w => w.Text)
				.AddFuncBinding(s => s.TypeOfOwnership != "ИП", w => w.Sensitive)
				.InitializeFromSource();

			entryMainCounterparty.Binding
				.AddBinding(ViewModel.Entity, e => e.MainCounterparty, w => w.Subject)
				.InitializeFromSource();

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
			enumDefaultDocumentType.Visible = ViewModel.Entity.PaymentMethod == PaymentType.Cashless;
			labelDefaultDocumentType.Visible = ViewModel.Entity.PaymentMethod == PaymentType.Cashless;

			lblTax.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.PersonType == PersonType.legal, w => w.Visible)
				.InitializeFromSource();

			specialListCmbWorksThroughOrganization.ItemsList = ViewModel.AllOrganizations;
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

			smallfileinformationsview.ViewModel = ViewModel.AttachedFileInformationsViewModel;
			frame3.Visible = ViewModel.Entity.IsForRetail;

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
			ycheckAlwaysSendReceitps.Visible = ViewModel.CanManageCachReceipts;

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
				ytreeviewSalesChannels.ColumnsConfig = ColumnsConfigFactory.Create<SalesChannelNode>()
					.AddColumn("Название").AddTextRenderer(node => node.Name)
					.AddColumn("").AddToggleRenderer(x => x.Selected)
					.Finish();

				ytreeviewSalesChannels.ItemsDataSource = ViewModel.SalesChannels;
			}
			frame2.Visible = ViewModel.Entity.IsForRetail;
		}

		public override void Dispose()
		{
			ytreeviewSalesChannels?.Destroy();
			smallfileinformationsview?.Destroy();
			base.Dispose();
		}
	}
}

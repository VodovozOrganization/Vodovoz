using System;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;

namespace Vodovoz
{
	[OrmDefaultIsFiltered(true)]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ClientBalanceFilter : RepresentationFilterBase<ClientBalanceFilter>
	{
		protected override void ConfigureFilter()
		{
			entryreferenceNomenclature.SubjectType = typeof(Nomenclature);
			entryreferenceClient.RepresentationModel = new ViewModel.CounterpartyVM();
		}

		public ClientBalanceFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public ClientBalanceFilter()
		{
			this.Build();
		}

		public Counterparty RestrictCounterparty {
			get { return entryreferenceClient.Subject as Counterparty; }
			set {
				entryreferenceClient.Subject = value;
				entryreferenceClient.Sensitive = false;
			}
		}

		public Nomenclature RestrictNomenclature {
			get { return entryreferenceNomenclature.Subject as Nomenclature; }
			set {
				entryreferenceNomenclature.Subject = value;
				entryreferenceNomenclature.Sensitive = false;
			}
		}

		public DeliveryPoint RestrictDeliveryPoint {
			get { return entryreferencePoint.Subject as DeliveryPoint; }
			set {
				entryreferencePoint.Subject = value;
				entryreferencePoint.Sensitive = false;
			}
		}

		public bool RestrictIncludeSold {
			get { return checkIncludeSold.Active; }
			set {
				checkIncludeSold.Active = value;
				checkIncludeSold.Sensitive = false;
			}
		}

		protected void OnSpeccomboStockItemSelected(object sender, EnumItemClickedEventArgs e)
		{
			OnRefiltered();
		}

		protected void OnEntryreferenceClientChanged(object sender, EventArgs e)
		{
			entryreferencePoint.Sensitive = RestrictCounterparty != null;
			if(RestrictCounterparty == null)
				entryreferencePoint.Subject = null;
			else {
				entryreferencePoint.Subject = null;
				entryreferencePoint.RepresentationModel = new ViewModel.ClientDeliveryPointsVM(UoW, RestrictCounterparty);
			}
			OnRefiltered();
		}

		protected void OnEntryreferencePointChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnCheckIncludeSoldToggled(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnEntryreferenceNomenclatureChangedByUser(object sender, EventArgs e)
		{
			OnRefiltered();
		}
	}
}


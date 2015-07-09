using System;
using System.Collections.Generic;
using NHibernate.Transform;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain;
using Gtk.DataBindings;

namespace Vodovoz.ViewModel
{
	public class AdditionalAgreementsVM : RepresentationModelBase<AdditionalAgreement, AdditionalAgreementVMNode>
	{
		IUnitOfWorkGeneric<CounterpartyContract> uow;

		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			AdditionalAgreement additionalAgreementAlias = null;
			CounterpartyContract counterpartyContractAlias = null;
			AdditionalAgreementVMNode resultAlias = null;

			var additionalAgreementsList = uow.Session.QueryOver<AdditionalAgreement> (() => additionalAgreementAlias)
				.JoinAlias (c => c.Contract, () => counterpartyContractAlias)
				.Where (() => counterpartyContractAlias.Id == uow.Root.Id)
				.SelectList (list => list
					.Select (() => additionalAgreementAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => additionalAgreementAlias.AgreementNumber).WithAlias (() => resultAlias.Number)
					//.Select (() => additionalAgreementAlias.AgreementTypeTitle).WithAlias (() => resultAlias.Type)
					//.Select (() => additionalAgreementAlias.IsActive).WithAlias (() => resultAlias.IsActive)
					//.Select (() => additionalAgreementAlias.Name).WithAlias (() => resultAlias.Name)
					//.Select (() => additionalAgreementAlias.Street).WithAlias (() => resultAlias.Street)
					//.Select (() => additionalAgreementAlias.Room).WithAlias (() => resultAlias.Room)
			                               )
				.TransformUsing (Transformers.AliasToBean<AdditionalAgreementVMNode> ())
				.List<AdditionalAgreementVMNode> ();

			SetItemsSource (additionalAgreementsList);
		}

		IMappingConfig treeViewConfig = FluentMappingConfig<AdditionalAgreementVMNode>.Create ()
			.AddColumn ("Номер").SetDataProperty (node => node.Number)
		                                //.AddColumn ("Тип").SetDataProperty (node => node.Type)
			.Finish ();

		public override IMappingConfig TreeViewConfig {
			get { return treeViewConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc (AdditionalAgreement updatedSubject)
		{
			return uow.Root.Id == updatedSubject.Contract.Id;
		}

		protected override bool NeedUpdateFunc (object updatedSubject)
		{
			return (updatedSubject as AdditionalAgreement).Contract.Id == uow.Root.Id;
		}

		#endregion

		public AdditionalAgreementsVM (IUnitOfWorkGeneric<CounterpartyContract> uow) : base (
				typeof(AdditionalAgreementDailyRent), 
				typeof(AdditionalAgreementFreeRent), 
				typeof(AdditionalAgreementNonFreeRent),
				typeof(AdditionalAgreementRepair),
				typeof(AdditionalAgreementWater))
		{
			this.uow = uow;
		}
	}

	public class AdditionalAgreementVMNode
	{

		public int Id { get; set; }

		public string Number { get; set; }

		//public string Type { get; set; }

		//public string DeliveryPoint { get; set; }

		//public string Date { get; set; }
	}
}


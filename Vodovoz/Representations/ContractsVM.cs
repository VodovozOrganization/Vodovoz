using System;
using System.Collections.Generic;
using Gtk;
using NHibernate.Transform;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain;
using Gtk.DataBindings;

namespace Vodovoz.ViewModel
{
	public class ContractsVM : RepresentationModelBase<CounterpartyContract, ContractsVMNode>
	{
		IUnitOfWorkGeneric<Counterparty> uow;

		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{

			CounterpartyContract contractAlias = null;
			Counterparty counterpartyAlias = null;
			Organization organizationAlias = null;
			ContractsVMNode resultAlias = null;
			AdditionalAgreement agreementAlias = null;

			var subquery = NHibernate.Criterion.QueryOver.Of<AdditionalAgreement>(() => agreementAlias)
				.Where(() => agreementAlias.Contract.Id == contractAlias.Id)
				.ToRowCountQuery();

			var contractslist = uow.Session.QueryOver<CounterpartyContract> (() => contractAlias)
				.JoinAlias (c => c.Counterparty, () => counterpartyAlias)
				.JoinAlias (c => c.Organization, () => organizationAlias)
				.Where (() => counterpartyAlias.Id == uow.Root.Id)
				.SelectList(list => list
					.Select(() => contractAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => contractAlias.IssueDate).WithAlias(() => resultAlias.IssueDate)
					.Select(() => contractAlias.IsArchive).WithAlias(() => resultAlias.IsArchive)
					.Select(() => contractAlias.OnCancellation).WithAlias(() => resultAlias.OnCancellation)
					.Select(() => organizationAlias.Name).WithAlias(() => resultAlias.Organization)
					.SelectSubQuery(subquery).WithAlias(() => resultAlias.AdditionalAgreements)
				)
				.TransformUsing(Transformers.AliasToBean<ContractsVMNode>())
				.List<ContractsVMNode>();

			SetItemsSource (contractslist);
		}

		IMappingConfig treeViewConfig = FluentMappingConfig<ContractsVMNode>.Create ()
			.AddColumn("Номер").SetDataProperty (node => node.Title)
			.AddColumn ("Организация").SetDataProperty (node => node.Organization)
			.AddColumn ("Кол-во доп. соглашений").SetDataProperty (node => node.AdditionalAgreements)
			.RowCells ().AddSetter<CellRendererText> ((c, n) => c.Foreground = n.RowColor)
			.Finish ();

		public override IMappingConfig TreeViewConfig {
			get { return treeViewConfig;}
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc (CounterpartyContract updatedSubject)
		{
			return uow.Root.Id == updatedSubject.Counterparty.Id;
		}
			
		protected override bool NeedUpdateFunc (object updatedSubject)
		{
			throw new NotImplementedException ();
		}

		#endregion

		public ContractsVM (IUnitOfWorkGeneric<Counterparty> uow)
		{
			this.uow = uow;
		}
	}
		
	public class ContractsVMNode : TreeNode
	{

		public int Id{ get; set;}

		public DateTime IssueDate{ get; set;}

		public bool IsArchive{ get; set;}

		public bool OnCancellation{ get; set;}

		public string Title {
			get { return String.Format ("{0} от {1:d}", Id, IssueDate); }
		}
			
		public string Organization { get; set;}

		public int AdditionalAgreements { get; set;}

		public string RowColor {
			get {
				if (IsArchive)
					return "grey";
				if (OnCancellation)
					return "blue";
				return "black";

			}
		}
	}
}


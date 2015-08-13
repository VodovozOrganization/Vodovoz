using System;
using System.Collections.Generic;
using NHibernate.Transform;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain;
using Gtk.DataBindings;
using Vodovoz.Domain.Service;
using System.Data.Bindings;

namespace Vodovoz.ViewModel
{
	public class ServiceClaimVM : RepresentationModelBase<ServiceClaim, ServiceClaimVMNode>
	{
		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			ServiceClaimVMNode resultAlias = null;
			ServiceClaim serviceClaimAlias = null;
			Counterparty counterpartyAlias = null;
			Nomenclature nomenclatureAlias = null;

			var query = UoW.Session.QueryOver<ServiceClaim> (() => serviceClaimAlias);

			var result = query
				.JoinAlias (sc => sc.Counterparty, () => counterpartyAlias)
				.JoinAlias (sc => sc.Nomenclature, () => nomenclatureAlias)

				.SelectList (list => list
					.Select (() => serviceClaimAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => serviceClaimAlias.Status).WithAlias (() => resultAlias.Status)
					.Select (() => serviceClaimAlias.ServiceStartDate).WithAlias (() => resultAlias.StartDate)
					.Select (() => counterpartyAlias.FullName).WithAlias (() => resultAlias.Counterparty)
					.Select (() => nomenclatureAlias.Name).WithAlias (() => resultAlias.Nomenclature)
			             )
				.TransformUsing (Transformers.AliasToBean<ServiceClaimVMNode> ())
				.List<ServiceClaimVMNode> ();

			SetItemsSource (result);
		}

		IMappingConfig treeViewConfig = FluentMappingConfig<ServiceClaimVMNode>.Create ()
			.AddColumn ("Номер").SetDataProperty (node => node.Id.ToString ())
			.AddColumn ("Дата").SetDataProperty (node => node.StartDate.ToString ("d"))
			.AddColumn ("Статус").SetDataProperty (node => node.Status.GetEnumTitle ())
			.AddColumn ("Клиент").SetDataProperty (node => node.Counterparty)
			.AddColumn ("Оборудование").SetDataProperty (node => node.Nomenclature)
			.Finish ();

		public override IMappingConfig TreeViewConfig {
			get { return treeViewConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc (ServiceClaim updatedSubject)
		{
			return true;
		}

		protected override bool NeedUpdateFunc (object updatedSubject)
		{
			throw new InvalidOperationException ();
		}

		#endregion

		public ServiceClaimVM () : this (UnitOfWorkFactory.CreateWithoutRoot ())
		{
		}

		public ServiceClaimVM (IUnitOfWork uow) : base ()
		{
			this.UoW = uow;
		}
	}

	public class ServiceClaimVMNode
	{
		public int Id{ get; set; }

		public string Counterparty { get; set; }

		public string Nomenclature { get; set; }

		public DateTime StartDate { get; set; }

		public ServiceClaimStatus Status { get; set; }
	}
}


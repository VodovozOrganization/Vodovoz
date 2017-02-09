using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using NHibernate.Transform;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Service;
using Vodovoz.Domain.Goods;

namespace Vodovoz.ViewModel
{
	public class ServiceClaimVM : RepresentationModelEntityBase<ServiceClaim, ServiceClaimVMNode>
	{
		public ServiceClaimFilter Filter {
			get {
				return RepresentationFilter as ServiceClaimFilter;
			}
			set {
				RepresentationFilter = value as IRepresentationFilter;
			}
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			ServiceClaimVMNode resultAlias = null;
			ServiceClaim serviceClaimAlias = null;
			Counterparty counterpartyAlias = null;
			Nomenclature nomenclatureAlias = null;

			var query = UoW.Session.QueryOver<ServiceClaim> (() => serviceClaimAlias);

			if (Filter.RestrictServiceClaimStatus != null) {
				query.Where (c => c.Status == Filter.RestrictServiceClaimStatus);
			}

			if (Filter.RestrictServiceClaimType != null) {
				query.Where (c => c.ServiceClaimType == Filter.RestrictServiceClaimType);
			}
				
			var result = query
				.JoinAlias (sc => sc.Counterparty, () => counterpartyAlias)
				.JoinAlias (sc => sc.Nomenclature, () => nomenclatureAlias)

				.SelectList (list => list
					.Select (() => serviceClaimAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => serviceClaimAlias.Status).WithAlias (() => resultAlias.Status)
					.Select (() => serviceClaimAlias.ServiceStartDate).WithAlias (() => resultAlias.StartDate)
					.Select (() => serviceClaimAlias.ServiceClaimType).WithAlias (() => resultAlias.Type)
					.Select (() => counterpartyAlias.FullName).WithAlias (() => resultAlias.Counterparty)
					.Select (() => nomenclatureAlias.Name).WithAlias (() => resultAlias.Nomenclature)
			             )
				.OrderBy(x => x.ServiceStartDate).Desc
				.TransformUsing (Transformers.AliasToBean<ServiceClaimVMNode> ())
				.List<ServiceClaimVMNode> ();

			SetItemsSource (result);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<ServiceClaimVMNode>.Create ()
			.AddColumn ("Номер").SetDataProperty (node => node.Id.ToString ())
			.AddColumn ("Дата").SetDataProperty (node => node.StartDate.ToString ("d"))
			.AddColumn ("Тип заявки").SetDataProperty (node => node.Type.GetEnumTitle ())
			.AddColumn ("Статус").SetDataProperty (node => node.Status.GetEnumTitle ())
			.AddColumn ("Клиент").SetDataProperty (node => node.Counterparty)
			.AddColumn ("Оборудование").SetDataProperty (node => node.Nomenclature)
			.Finish ();

		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc (ServiceClaim updatedSubject)
		{
			return true;
		}

		#endregion

		public ServiceClaimVM () : this (UnitOfWorkFactory.CreateWithoutRoot ())
		{
			CreateRepresentationFilter = () => new ServiceClaimFilter (UoW);
		}

		public ServiceClaimVM (IUnitOfWork uow) : base ()
		{
			this.UoW = uow;
		}

		public ServiceClaimVM (ServiceClaimFilter filter) : this (filter.UoW)
		{
			Filter = filter;
		}
	}

	public class ServiceClaimVMNode
	{
		public int Id{ get; set; }

		public string Counterparty { get; set; }

		public string Nomenclature { get; set; }

		public DateTime StartDate { get; set; }

		public ServiceClaimStatus Status { get; set; }

		public ServiceClaimType Type { get; set; }
	}
}


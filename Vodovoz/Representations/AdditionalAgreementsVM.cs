using System;
using System.Collections.Generic;
using NHibernate.Transform;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain;
using Gtk.DataBindings;
using NHibernate.Criterion;

namespace Vodovoz.ViewModel
{
	public class AdditionalAgreementsVM : RepresentationModelBase<AdditionalAgreement, AdditionalAgreementVMNode>
	{
		public IUnitOfWorkGeneric<CounterpartyContract> CounterpartyUoW {
			get {
				return UoW as IUnitOfWorkGeneric<CounterpartyContract>;
			}
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			AdditionalAgreement additionalAgreementAlias = null;
			CounterpartyContract counterpartyContractAlias = null;
			AdditionalAgreementVMNode resultAlias = null;
			DeliveryPoint deliveryPointAlias = null;

			var additionalAgreementsList = UoW.Session.QueryOver<AdditionalAgreement> (() => additionalAgreementAlias)
				.JoinAlias (c => c.Contract, () => counterpartyContractAlias)
				.JoinAlias (c => c.DeliveryPoint, () => deliveryPointAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Where (() => counterpartyContractAlias.Id == CounterpartyUoW.Root.Id)
				.SelectList (list => list
					.Select (() => additionalAgreementAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => additionalAgreementAlias.AgreementNumber).WithAlias (() => resultAlias.Number)
					.Select (() => additionalAgreementAlias.IssueDate).WithAlias (() => resultAlias.IssueDate)
					.Select (Projections.Property ("additionalAgreementAlias.class")).WithAlias (() => resultAlias.Type)
//TODO FIXME Найти способ написать это лямбдой а не строкой
					.Select (() => deliveryPointAlias.Building).WithAlias (() => resultAlias.Building)
					.Select (() => deliveryPointAlias.City).WithAlias (() => resultAlias.City)
					.Select (() => deliveryPointAlias.IsActive).WithAlias (() => resultAlias.IsActive)
					.Select (() => deliveryPointAlias.Name).WithAlias (() => resultAlias.Name)
					.Select (() => deliveryPointAlias.Street).WithAlias (() => resultAlias.Street)
					.Select (() => deliveryPointAlias.Room).WithAlias (() => resultAlias.Room))
				.TransformUsing (Transformers.AliasToBean<AdditionalAgreementVMNode> ())
				.List<AdditionalAgreementVMNode> ();
			SetItemsSource (additionalAgreementsList);
		}

		IMappingConfig treeViewConfig = FluentMappingConfig<AdditionalAgreementVMNode>.Create ()
			.AddColumn ("Номер").SetDataProperty (node => node.NumberString)
			.AddColumn ("Дата").SetDataProperty (node => node.IssueDateString)
			.AddColumn ("Тип").SetDataProperty (node => node.TypeString)
			.AddColumn ("Точка доставки").SetDataProperty (node => node.Point)
			.Finish ();

		public override IMappingConfig TreeViewConfig {
			get { return treeViewConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc (AdditionalAgreement updatedSubject)
		{
			return CounterpartyUoW.Root.Id == updatedSubject.Contract.Id;
		}

		protected override bool NeedUpdateFunc (object updatedSubject)
		{
			return (updatedSubject as AdditionalAgreement).Contract.Id == CounterpartyUoW.Root.Id;
		}

		#endregion

		public AdditionalAgreementsVM (IUnitOfWorkGeneric<CounterpartyContract> uow) : base (
				typeof(AdditionalAgreementDailyRent), 
				typeof(AdditionalAgreementFreeRent), 
				typeof(AdditionalAgreementNonFreeRent),
				typeof(AdditionalAgreementRepair),
				typeof(AdditionalAgreementWater))
		{
			this.UoW = uow;
		}
	}

	public class AdditionalAgreementVMNode
	{

		public int Id { get; set; }

		public string Number { get; set; }

		public string NumberString { 
			get {
				switch (Type) {
				case "DailyRent":
					return String.Format ("{0} - А", Number);
				case "NonfreeRent":
					return String.Format ("{0} - А", Number);
				case "FreeRent":
					return String.Format ("{0} - Б", Number);
				case "WaterSales":
					return String.Format ("{0} - В", Number);
				case "Repair":
					return String.Format ("{0} - Т", Number);
				default:
					return Number;
				}
			}
		}

		public DateTime IssueDate { get; set; }

		public string IssueDateString { get { return String.Format ("От {0}", IssueDate.ToShortDateString ()); } }

		public string Type { get; set; }

		public string TypeString {
			get {
				switch (Type) {
				case "DailyRent":
					return "Посуточная аренда";
				case "NonfreeRent":
					return "Долгосрочная аренда";
				case "FreeRent":
					return "Бесплатная аренда";
				case "WaterSales":
					return "Продажа воды";
				case "Repair":
					return "Сервис";
				default:
					return "Тип не определен";
				}
			}
		}

		public string Name { get; set; }

		public string City { get; set; }

		public string Street { get; set; }

		public string Building { get; set; }

		public string Room { get; set; }

		public bool IsActive { get; set; }

		public string RowColor { get { return IsActive ? "black" : "grey"; } }

		public string Point { 
			get { if (String.IsNullOrWhiteSpace (Name) && String.IsNullOrWhiteSpace (City) && String.IsNullOrWhiteSpace (Street))
					return String.Empty;
				else
					return String.Format ("{0}г. {1}, ул. {2}, д.{3}, квартира/офис {4}",
						(Name == String.Empty ? "" : "\"" + Name + "\": "), City, Street, Building, Room); }
		}
	}
}


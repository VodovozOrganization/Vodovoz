using System;
using Gamma.ColumnConfig;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Representations
{
	public class TraineeVM : RepresentationModelEntityBase<Trainee, TraineeVMNode>
	{
		#region IRepresentationModel implementation

		public override void UpdateNodes()
		{
			TraineeVMNode resultAlias = null;
			Trainee traineeAlias = null;

			var query = UoW.Session.QueryOver<Trainee>(() => traineeAlias);

			var result = query
				.SelectList(list => list
				   .Select(() => traineeAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => traineeAlias.Name).WithAlias(() => resultAlias.FirstName)
				   .Select(() => traineeAlias.LastName).WithAlias(() => resultAlias.LastName)
				   .Select(() => traineeAlias.Patronymic).WithAlias(() => resultAlias.Patronymic)
				   )
				.OrderBy(x => x.LastName).Asc
				.OrderBy(x => x.Name).Asc
				.OrderBy(x => x.Patronymic).Asc
				.TransformUsing(NHibernate.Transform.Transformers.AliasToBean<TraineeVMNode>())
				.List<TraineeVMNode>();

			SetItemsSource(result);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<TraineeVMNode>.Create()
			.AddColumn("Код").SetDataProperty(node => node.Id.ToString())
			.AddColumn("Ф.И.О.").SetDataProperty(node => node.FullName)
			.Finish();

		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
		}

		public override bool PopupMenuExist {
			get {
				return false;
			}
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc(Trainee updatedSubject)
		{
			return true;
		}

		#endregion

		private void AdditionalUpdateSubscribe()
		{
			OrmMain.GetObjectDescription<Employee>().ObjectUpdatedGeneric += Handle_ObjectUpdatedGeneric;
		}

		void Handle_ObjectUpdatedGeneric(object sender, QSOrmProject.UpdateNotification.OrmObjectUpdatedGenericEventArgs<Employee> e)
		{
			UpdateNodes();
		}

		public TraineeVM() : this(UnitOfWorkFactory.CreateWithoutRoot())
		{
		}

		public TraineeVM(IUnitOfWork uow)
		{
			this.UoW = uow;
			AdditionalUpdateSubscribe();
		}
	}

	public class TraineeVMNode : INodeWithEntryFastSelect
	{
		[UseForSearch]
		[SearchHighlight]
		public int Id { get; set; }

		public string LastName { get; set; }
		public string FirstName { get; set; }
		public string Patronymic { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string FullName { get { return String.Format("{0} {1} {2}", LastName, FirstName, Patronymic); } }

		public string EntityTitle => FullName;
	}
}

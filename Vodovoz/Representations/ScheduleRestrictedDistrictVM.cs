using Gamma.ColumnConfig;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Representations
{
	public class ScheduleRestrictedDistrictVM : RepresentationModelEntityBase<ScheduleRestrictedDistrict, ScheduleRestrictedDistrict>
	{
		#region IRepresentationModel implementation

		public override void UpdateNodes()
		{
			var ScheduleRestrictedDistricts = UoW.Session.QueryOver<ScheduleRestrictedDistrict>().List();

			SetItemsSource(ScheduleRestrictedDistricts);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<ScheduleRestrictedDistrict>.Create()
				.AddColumn("Код").AddTextRenderer(x => x.Id.ToString())
				.AddColumn("Название").AddTextRenderer(x => x.DistrictName)
				.Finish();

		public override IColumnsConfig ColumnsConfig => columnsConfig;

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc(ScheduleRestrictedDistrict updatedSubject) => true;

		#endregion

		public ScheduleRestrictedDistrictVM() : this(UnitOfWorkFactory.CreateWithoutRoot()) { }

		public ScheduleRestrictedDistrictVM(IUnitOfWork uow) => this.UoW = uow;
	}
}

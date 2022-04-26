using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Utilities.Text;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Orders;
using Vodovoz.Filters.ViewModels;

namespace Vodovoz.ViewModel
{
	public class WorkingDriversVM : RepresentationModelEntityBase<RouteList, WorkingDriverVMNode>
	{
		private bool _isFastDeliveryOnly;

		#region IRepresentationModel implementation

		public override void UpdateNodes()
		{
			WorkingDriverVMNode resultAlias = null;
			Employee driverAlias = null;
			RouteList routeListAlias = null;
			Car carAlias = null;
			CarVersion carVersionAlias = null;

			Domain.Orders.Order orderAlias = null;
			OrderItem ordItemsAlias = null;
			Nomenclature nomenclatureAlias = null;

			var completedSubquery = QueryOver.Of<RouteListItem>()
				.Where(i => i.RouteList.Id == routeListAlias.Id)
				.Where(i => i.Status != RouteListItemStatus.EnRoute)
				.Select(Projections.RowCount());

			var addressesSubquery = QueryOver.Of<RouteListItem>()
				.Where(i => i.RouteList.Id == routeListAlias.Id)
				.Select(Projections.RowCount());

			var uncompletedBottlesSubquery = QueryOver.Of<RouteListItem>()  // Запрашивает количество ещё не доставленных бутылей.
				.Where(i => i.RouteList.Id == routeListAlias.Id)
				.Where(i => i.Status == RouteListItemStatus.EnRoute)
				   .JoinAlias(rli => rli.Order, () => orderAlias)
				   .JoinAlias(() => orderAlias.OrderItems, () => ordItemsAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				   .JoinAlias(() => ordItemsAlias.Nomenclature, () => nomenclatureAlias)
			   	.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.Select(Projections.Sum(() => ordItemsAlias.Count));

			var trackSubquery = QueryOver.Of<Track>()
				.Where(x => x.RouteList.Id == routeListAlias.Id)
				.Select(x => x.Id);

			var isCompanyHavingProjection = Projections.Conditional(
				Restrictions.Eq(Projections.Property(() => carVersionAlias.CarOwnType), CarOwnType.Company),
				Projections.Constant(true),
				Projections.Constant(false));

			var query = UoW.Session.QueryOver<RouteList>(() => routeListAlias);

			if(Filter.IsFastDeliveryOnly)
			{
				query.Where(() => routeListAlias.AdditionalLoadingDocument != null);
			}

			var result = query
				.JoinAlias(rl => rl.Driver, () => driverAlias)
				.JoinAlias(rl => rl.Car, () => carAlias)
				.JoinEntityAlias(() => carVersionAlias,
					() => carVersionAlias.Car.Id == carAlias.Id
						&& carVersionAlias.StartDate <= routeListAlias.Date &&
						(carVersionAlias.EndDate == null || carVersionAlias.EndDate >= routeListAlias.Date))
				.Where(rl => rl.Status == RouteListStatus.EnRoute)
				.Where(rl => rl.Driver != null)
				.Where(rl => rl.Car != null)
				.SelectList(list => list
					.Select(() => driverAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => driverAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => driverAlias.LastName).WithAlias(() => resultAlias.LastName)
					.Select(() => driverAlias.Patronymic).WithAlias(() => resultAlias.Patronymic)
					.Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.CarNumber)
					.Select(isCompanyHavingProjection).WithAlias(() => resultAlias.IsVodovozAuto)
					.Select(() => routeListAlias.Id).WithAlias(() => resultAlias.RouteListNumber)
					.SelectSubQuery(addressesSubquery).WithAlias(() => resultAlias.AddressesAll)
					.SelectSubQuery(completedSubquery).WithAlias(() => resultAlias.AddressesCompleted)
					.SelectSubQuery(trackSubquery).WithAlias(() => resultAlias.TrackId)
					.SelectSubQuery(uncompletedBottlesSubquery).WithAlias(() => resultAlias.BottlesLeft)
					)
				.TransformUsing(Transformers.AliasToBean<WorkingDriverVMNode>())
				.List<WorkingDriverVMNode>();

			var summaryResult = new List<WorkingDriverVMNode>();
			foreach(var driver in result.GroupBy(x => x.Id)) {
				var savedRow = driver.First();
				savedRow.RouteListsText = String.Join("; ", driver.Select(x => x.TrackId != null ? String.Format("<span foreground=\"green\"><b>{0}</b></span>", x.RouteListNumber) : x.RouteListNumber.ToString()));
				savedRow.RouteListsIds = driver.ToDictionary(x => x.RouteListNumber, x => x.TrackId);
				savedRow.AddressesAll = driver.Sum(x => x.AddressesAll);
				savedRow.AddressesCompleted = driver.Sum(x => x.AddressesCompleted);
				summaryResult.Add(savedRow);
			}

			SetItemsSource(summaryResult.OrderBy(x => x.ShortName).ToList());
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<WorkingDriverVMNode>.Create()
			.AddColumn("Имя").SetDataProperty(node => node.ShortName)
			.AddColumn("Машина").AddTextRenderer().AddSetter((c, node) => c.Markup = node.CarText)
			.AddColumn("МЛ").AddTextRenderer().AddSetter((c, node) => c.Markup = node.RouteListsText)
			.AddColumn("Выполнено").AddProgressRenderer(x => x.CompletedPercent)
			.AddSetter((c, n) => c.Text = n.CompletedText)
			.AddColumn("Остаток бут.").AddTextRenderer().AddSetter((c, node) => c.Markup = $"{node.BottlesLeft:N0}")
			.AddColumn("Остаток запаса").AddTextRenderer().AddSetter((c, node) => c.Markup = $"{node.AdditionalWater19LLeft:N0}")
			.Finish();

		public override IColumnsConfig ColumnsConfig => columnsConfig;

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc(RouteList updatedSubject) => true;

		#endregion

		public WorkingDriversVM() : this(UnitOfWorkFactory.CreateWithoutRoot(), new RouteListTrackFilterViewModel()) { }

		public WorkingDriversVM(IUnitOfWork uow, RouteListTrackFilterViewModel routeListTrackFilterViewModel) : base()
		{
			this.UoW = uow;
			Filter = routeListTrackFilterViewModel;
		}

		private RouteListTrackFilterViewModel _filter;
		public RouteListTrackFilterViewModel Filter
		{
			get => _filter;
			set
			{
				if(_filter != value)
				{
					_filter = value;
					_filter.OnFiltered += (sender, e) => UpdateNodes();
				}
			}
		}
	}

	public class WorkingDriverVMNode
	{
		public int Id { get; set; }

		public string Name { get; set; }
		public string LastName { get; set; }
		public string Patronymic { get; set; }
		public string CarNumber { get; set; }
		public string RouteListsText { get; set; }
		public bool IsVodovozAuto { get; set; }

		//RouteListId, TrackId
		public Dictionary<int, int?> RouteListsIds;

		public int AddressesCompleted { get; set; }
		public int AddressesAll { get; set; }

		public decimal BottlesLeft { get; set; } // @Дима

		public int AdditionalWater19LLeft { get; set; }

		public int CompletedPercent {
			get {
				if(AddressesAll == 0)
					return 100;
				return (int)(((double)AddressesCompleted / AddressesAll) * 100);
			}
		}

		public string CompletedText => string.Format("{0}/{1}", AddressesCompleted, AddressesAll);

		public string CarText => IsVodovozAuto ? string.Format("<b>{0}</b>", CarNumber) : CarNumber;

		private int routeListNumber;

		public int? TrackId;

		public int RouteListNumber {
			get => routeListNumber;
			set {
				routeListNumber = value;
				this.RouteListsText = value.ToString();
			}
		}

		public string ShortName => PersonHelper.PersonNameWithInitials(LastName, Name, Patronymic);
	}
}

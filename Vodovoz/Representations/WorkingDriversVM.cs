using System;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using QSOrmProject;
using NHibernate.Transform;
using Gamma.ColumnConfig;
using System.Linq;
using QSProjectsLib;
using Vodovoz.Domain.Chat;
using ChatClass = Vodovoz.Domain.Chat.Chat;
using Vodovoz.Repository.Chat;
using Vodovoz.Repository;
using NHibernate.Criterion;
using System.Collections.Generic;

namespace Vodovoz.ViewModel
{
	public class WorkingDriversVM : RepresentationModelEntityBase<RouteList, WorkingDriverVMNode>
	{
		#region IRepresentationModel implementation

		public override void UpdateNodes()
		{
			WorkingDriverVMNode resultAlias = null;
			Employee driverAlias = null;
			RouteList routeListAlias = null;
			Car carAlias = null;

			var completedSubquery = QueryOver.Of<RouteListItem>()
				.Where(i => i.RouteList.Id == routeListAlias.Id)
				.Where(i => i.Status != RouteListItemStatus.EnRoute)
				.Select(Projections.RowCount());

			var addressesSubquery = QueryOver.Of<RouteListItem>()
				.Where(i => i.RouteList.Id == routeListAlias.Id)
				.Select(Projections.RowCount());

			var trackSubquery = QueryOver.Of<Track>()
				.Where(x => x.RouteList.Id == routeListAlias.Id)
				.Select(x => x.Id);

			var query = UoW.Session.QueryOver<RouteList>(() => routeListAlias);

			var result = query
				.JoinAlias(rl => rl.Driver, () => driverAlias)
				.JoinAlias(rl => rl.Car, () => carAlias)

				.Where (rl => rl.Status == RouteListStatus.EnRoute)
				.Where (rl => rl.Driver != null)
				.Where (rl => rl.Car != null)

				.SelectList(list => list
					.Select(() => driverAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => driverAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => driverAlias.LastName).WithAlias(() => resultAlias.LastName)
					.Select(() => driverAlias.Patronymic).WithAlias(() => resultAlias.Patronymic)
					.Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.CarNumber)
					.Select(() => routeListAlias.Id).WithAlias(() => resultAlias.RouteListNumber)
					.SelectSubQuery(addressesSubquery).WithAlias(() => resultAlias.AddressesAll)
					.SelectSubQuery(completedSubquery).WithAlias(() => resultAlias.AddressesCompleted)
					.SelectSubQuery(trackSubquery).WithAlias(() => resultAlias.TrackId)
			             )
				.TransformUsing(Transformers.AliasToBean<WorkingDriverVMNode>())
				.List<WorkingDriverVMNode>();

			var summaryResult = new List<WorkingDriverVMNode>();
			foreach(var driver in result.GroupBy(x => x.Id))
			{
				var savedRow = driver.First();
				savedRow.RouteListsText = String.Join("; ", driver.Select(x => x.TrackId != null ? String.Format("<span foreground=\"green\"><b>{0}</b></span>", x.RouteListNumber) : x.RouteListNumber.ToString()));
				savedRow.RouteListsIds = driver.ToDictionary(x => x.RouteListNumber, x => x.TrackId);
				savedRow.AddressesAll = driver.Sum(x => x.AddressesAll);
				savedRow.AddressesCompleted = driver.Sum(x => x.AddressesCompleted);
				summaryResult.Add(savedRow);
			}

			var currentEmploee = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			if (currentEmploee != null)
			{
				var chats = ChatRepository.GetCurrentUserChats(UoW, null);
				var unreaded = ChatMessageRepository.GetUnreadedChatMessages(UoW, currentEmploee, true);
				foreach (var item in summaryResult)
				{
					var chat = chats.FirstOrDefault(x => x.Driver.Id == item.Id);
					if (chat != null && unreaded.ContainsKey(chat.Id))
					{
						item.Unreaded = unreaded[chat.Id];
					}
				}
			}
			SetItemsSource(summaryResult.OrderBy(x => x.ShortName).ToList());
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<WorkingDriverVMNode>.Create()
			.AddColumn("Имя").SetDataProperty(node => node.ShortName)
			.AddColumn("Машина").SetDataProperty(node => node.CarNumber)
			.AddColumn("Маршрутные листы").AddTextRenderer().AddSetter( (c, node) => c.Markup = node.RouteListsText)
			.AddColumn("Чат").AddTextRenderer().AddSetter((w, n) => w.Markup = (n.Unreaded > 0 ? String.Format("<b><span foreground=\"red\">{0}</span></b>", n.Unreaded) : String.Empty))
			.AddColumn("Выполнено").AddProgressRenderer(x => x.CompletedPercent)
			.AddSetter((c, n) => c.Text = n.CompletedText)
			.Finish();

		public override IColumnsConfig ColumnsConfig
		{
			get { return columnsConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc(RouteList updatedSubject)
		{
			return true;
		}

		#endregion

		public WorkingDriversVM()
			: this(UnitOfWorkFactory.CreateWithoutRoot())
		{
		}

		public WorkingDriversVM(IUnitOfWork uow)
			: base()
		{
			this.UoW = uow;
		}
	}

	public class WorkingDriverVMNode
	{
		public int Id{ get; set; }

		public string Name { get; set; }
		public string LastName { get; set; }
		public string Patronymic { get; set; }
		public string CarNumber { get; set; }
		public string RouteListsText { get; set; }
		public int Unreaded { get; set; }

		//RouteListId, TrackId
		public Dictionary<int, int?> RouteListsIds;

		public int AddressesCompleted { get; set; }
		public int AddressesAll { get; set; }

		public int CompletedPercent{
			get{
				if (AddressesAll == 0)
					return 100;
				return (int)(((double)AddressesCompleted / AddressesAll) * 100);
			}
		}

		public string CompletedText{
			get{
				return String.Format("{0}/{1}", AddressesCompleted, AddressesAll);
			}
		}

		private int routeListNumber;

		public int? TrackId;

		public int RouteListNumber
		{ 
			get { return routeListNumber; } 
			set
			{ 
				routeListNumber = value;
				this.RouteListsText = value.ToString();
			}
		}

		public string ShortName
		{ 
			get { return StringWorks.PersonNameWithInitials (LastName, Name, Patronymic);}
		}

	}
}
using System;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class SectorNodeViewModel : PropertyChangedBase
	{

		public SectorNodeViewModel(int id, DateTime createDate, string name = null)
		{
			Id = id;
			Name = name ?? "";
			CreateDate = createDate;
		}

		public SectorNodeViewModel(Sector sector, string name = null)
		{
			Id = sector.Id;
			Name = name ?? "";
			CreateDate = sector.DateCreated;
		}
		
		private int _id;

		public int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		private string _name;

		public string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		private DateTime _createDate;

		public DateTime CreateDate
		{
			get => _createDate;
			set => SetField(ref _createDate, value);
		}
	}
}
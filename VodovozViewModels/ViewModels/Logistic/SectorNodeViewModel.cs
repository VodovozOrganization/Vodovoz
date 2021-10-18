using System;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class SectorNodeViewModel : PropertyChangedBase
	{

		public SectorNodeViewModel(Sector sector, DateTime createDate, string name = null)
		{
			Sector = sector;
			Name = name ?? "";
			CreateDate = createDate;
		}

		public SectorNodeViewModel(Sector sector, string name = null)
		{
			Sector = sector;
			Name = name ?? "";
			CreateDate = sector.DateCreated;
		}
		
		private Sector _sector;

		public Sector Sector
		{
			get => _sector;
			set => SetField(ref _sector, value);
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

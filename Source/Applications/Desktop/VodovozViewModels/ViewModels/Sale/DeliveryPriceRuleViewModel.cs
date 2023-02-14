using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.EntityRepositories;
using Vodovoz.Infrastructure.Print;
using Vodovoz.Infrastructure.Services;
using Vodovoz.PermissionExtensions;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.Factories;
using System.ComponentModel.DataAnnotations;
using QS.Project.Services;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.ViewModels.ViewModels.Sale
{
	public class DeliveryPriceRuleViewModel : EntityTabViewModelBase<DeliveryPriceRule>
	{
		private readonly IDistrictRuleRepository districtRuleRepository;

		public DeliveryPriceRuleViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IDistrictRuleRepository districtRuleRepository)
		: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			this.districtRuleRepository = districtRuleRepository ?? throw new ArgumentNullException(nameof(districtRuleRepository));

			ValidationContext.ServiceContainer.AddService(typeof(IDistrictRuleRepository), districtRuleRepository);
		}

		#region Свойства
		private int water19LCount;
		public int Water19LCount
		{
			get
			{
				water19LCount = Entity.Water19LCount;
				return water19LCount;
			}
			set
			{
				if(SetField(ref water19LCount, value))
				{
					Entity.Water19LCount = value;
				}
			}
		}

		private int water6LCount;
		public int Water6LCount
		{
			get
			{
				water6LCount = Entity.Water6LCount;
				return water6LCount;
			}
			set
			{
				if(SetField(ref water6LCount, value))
				{
					Entity.Water6LCount = value;
				}
			}
		}

		private int water1500mlCount;
		public int Water1500mlCount
		{
			get
			{
				water1500mlCount = Entity.Water1500mlCount;
				return water1500mlCount;
			}
			set
			{
				if(SetField(ref water1500mlCount, value))
				{
					Entity.Water1500mlCount = value;
				}
			}
		}

		private int water600mlCount;
		public int Water600mlCount
		{
			get
			{
				water600mlCount = Entity.Water600mlCount;
				return water600mlCount;
			}
			set
			{
				if(SetField(ref water600mlCount, value))
				{
					Entity.Water600mlCount = value;
				}
			}
		}

		private int water500mlCount;
		public int Water500mlCount
		{
			get
			{
				water500mlCount = Entity.Water500mlCount;
				return water500mlCount;
			}
			set
			{
				if(SetField(ref water500mlCount, value))
				{
					Entity.Water500mlCount = value;
				}
			}
		}

		private string ruleName;
		public string RuleName
		{
			get
			{
				ruleName = Entity.RuleName;
				return ruleName;
			}
			set
			{
				if(SetField(ref ruleName, value))
				{
					Entity.RuleName = value;
				}
			}
		}

		private decimal orderMinSumEShopGoods;
		public decimal OrderMinSumEShopGoods
		{
			get
			{
				orderMinSumEShopGoods = Entity.OrderMinSumEShopGoods;
				return orderMinSumEShopGoods;
			}
			set
			{
				if(SetField(ref orderMinSumEShopGoods, value))
				{
					Entity.OrderMinSumEShopGoods = value;
				}
			}
		}
		#endregion



		public override bool Save(bool close)
		{
			ValidationContext = ConfigureValidationContext(UoW, districtRuleRepository);
			return base.Save(close);
		}

		private ValidationContext ConfigureValidationContext(IUnitOfWork uow, IDistrictRuleRepository districtRuleRepository)
		{
			if(uow == null)
			{
				throw new ArgumentNullException(nameof(uow));
			}
			if(districtRuleRepository == null)
			{
				throw new ArgumentNullException(nameof(districtRuleRepository));
			}

			ValidationContext context = new ValidationContext(this, new Dictionary<object, object> {
				{"Reason", nameof(ConfigureValidationContext)}
			});
			context.ServiceContainer.AddService(typeof(IUnitOfWork), uow);
			context.ServiceContainer.AddService(typeof(IDistrictRuleRepository), districtRuleRepository);
			return context;
		}

		#region Permissions

		public bool CanCreate => PermissionResult.CanCreate;
		public bool CanRead => PermissionResult.CanRead;
		public bool CanUpdate => PermissionResult.CanUpdate;
		public bool CanDelete => PermissionResult.CanDelete;

		public bool CanCreateOrUpdate => Entity.Id == 0 ? CanCreate : CanUpdate;

		#endregion
	}
}

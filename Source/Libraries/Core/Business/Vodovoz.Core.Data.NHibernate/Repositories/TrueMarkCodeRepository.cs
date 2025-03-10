using QS.DomainModel.UoW;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Data.NHibernate.Repositories
{
	internal sealed class TrueMarkCodeRepository : ITrueMarkCodeRepository
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IGenericRepository<TrueMarkWaterGroupCode> _waterGroupCodeRepository;
		private readonly IGenericRepository<TrueMarkTransportCode> _transportCodeRepository;

		public TrueMarkCodeRepository(
			IUnitOfWorkFactory uowFactory,
			IGenericRepository<TrueMarkWaterGroupCode> waterGroupCodeRepository,
			IGenericRepository<TrueMarkTransportCode> transportCodeRepository
			)
		{
			_uowFactory = uowFactory ?? throw new System.ArgumentNullException(nameof(uowFactory));
			_waterGroupCodeRepository = waterGroupCodeRepository ?? throw new System.ArgumentNullException(nameof(waterGroupCodeRepository));
			_transportCodeRepository = transportCodeRepository ?? throw new System.ArgumentNullException(nameof(transportCodeRepository));
		}

		public TrueMarkWaterGroupCode GetParentGroupCode(int id)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				return GetParentGroupCode(uow, id);
			}
		}

		public TrueMarkWaterGroupCode GetParentGroupCode(IUnitOfWork uow, int id)
		{
			var groupCode = _waterGroupCodeRepository
				.Get(uow, x => x.Id == id, 1)
				.FirstOrDefault();

			if(groupCode.ParentWaterGroupCodeId != null)
			{
				return GetParentGroupCode(uow, groupCode.ParentWaterGroupCodeId.Value);
			}

			return groupCode;
		}


		public TrueMarkTransportCode FindParentTransportCode(TrueMarkWaterIdentificationCode code)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				return FindParentTransportCode(uow, code);
			}
		}

		public TrueMarkTransportCode FindParentTransportCode(IUnitOfWork uow, TrueMarkWaterIdentificationCode code)
		{
			if(code.ParentTransportCodeId != null)
			{
				var transportCode = _transportCodeRepository
					.Get(uow, x => x.Id == code.ParentTransportCodeId.Value, 1)
					.FirstOrDefault();

				return transportCode;
			}

			if(code.ParentWaterGroupCodeId == null)
			{
				return null;
			}

			int nextGroupCodeId = code.ParentWaterGroupCodeId.Value;
			while(true)
			{
				var groupCode = _waterGroupCodeRepository
					.Get(uow, x => x.Id == nextGroupCodeId, 1)
					.FirstOrDefault();

				if(groupCode.ParentTransportCodeId != null)
				{
					var transportCode = _transportCodeRepository
						.Get(uow, x => x.Id == groupCode.ParentTransportCodeId.Value, 1)
						.FirstOrDefault();

					return transportCode;
				}

				if(groupCode.ParentWaterGroupCodeId == null)
				{
					return null;
				}

				nextGroupCodeId = groupCode.ParentWaterGroupCodeId.Value;
			}
		}

		public TrueMarkTransportCode FindParentTransportCode(TrueMarkWaterGroupCode code)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				return FindParentTransportCode(uow, code);
			}
		}

		public TrueMarkTransportCode FindParentTransportCode(IUnitOfWork uow, TrueMarkWaterGroupCode code)
		{
			if(code.ParentTransportCodeId != null)
			{
				var transportCode = _transportCodeRepository
					.Get(uow, x => x.Id == code.ParentTransportCodeId.Value, 1)
					.FirstOrDefault();

				return transportCode;
			}

			if(code.ParentWaterGroupCodeId == null)
			{
				return null;
			}

			int nextGroupCodeId = code.ParentWaterGroupCodeId.Value;
			while(true)
			{
				var groupCode = _waterGroupCodeRepository
					.Get(uow, x => x.Id == nextGroupCodeId, 1)
					.FirstOrDefault();

				if(groupCode.ParentTransportCodeId != null)
				{
					var transportCode = _transportCodeRepository
						.Get(uow, x => x.Id == groupCode.ParentTransportCodeId.Value, 1)
						.FirstOrDefault();

					return transportCode;
				}

				if(groupCode.ParentWaterGroupCodeId == null)
				{
					return null;
				}

				nextGroupCodeId = groupCode.ParentWaterGroupCodeId.Value;
			}
		}
	}
}

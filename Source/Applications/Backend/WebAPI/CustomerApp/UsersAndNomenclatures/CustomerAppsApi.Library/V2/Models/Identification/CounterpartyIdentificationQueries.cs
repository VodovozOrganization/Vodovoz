using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Orders;

namespace CustomerAppsApi.Library.V2.Models.Identification
{
	/// <summary>
	/// Запросы к БД для идентификации контрагента
	/// </summary>
	public class CounterpartyIdentificationQueries : ICounterpartyIdentificationQueries
	{
		private readonly IUnitOfWork _unitOfWork;

		public CounterpartyIdentificationQueries(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		/// <inheritdoc />
		public async Task<List<int>> GetCounterpartyIdsByCounterpartyPhoneAsync(
			string phone,
			bool useSuffix,
			CancellationToken cancellationToken)
		{
			var query = _unitOfWork.GetAll<Phone>()
				.Where(p => p.Counterparty != null && p.DigitsNumber != null);

			if(useSuffix)
			{
				var suffixPattern = $"%{phone}";
				query = query.Where(p => SqlMethods.Like(p.DigitsNumber, suffixPattern));
			}
			else
			{
				query = query.Where(p => p.DigitsNumber == phone);
			}

			return await query
				.Select(p => p.Counterparty.Id)
				.Take(CounterpartyIdentificationOptions.MaxSearchResults)
				.ToListAsync(cancellationToken);
		}

		/// <inheritdoc />
		public async Task<List<int>> GetCounterpartyIdsByDeliveryPointPhoneAsync(
			string phone,
			bool useSuffix,
			CancellationToken cancellationToken)
		{
			var query = _unitOfWork.GetAll<Phone>()
				.Where(p => p.DeliveryPoint != null
					&& p.DeliveryPoint.Counterparty != null
					&& p.DigitsNumber != null);

			if(useSuffix)
			{
				var suffixPattern = $"%{phone}";
				query = query.Where(p => SqlMethods.Like(p.DigitsNumber, suffixPattern));
			}
			else
			{
				query = query.Where(p => p.DigitsNumber == phone);
			}

			return await query
				.Select(p => p.DeliveryPoint.Counterparty.Id)
				.Take(CounterpartyIdentificationOptions.MaxSearchResults)
				.ToListAsync(cancellationToken);
		}

		/// <inheritdoc />
		public async Task<List<int>> GetCounterpartyIdsByPersonNameAsync(
			string surname,
			string firstName,
			string patronymic,
			CancellationToken cancellationToken)
		{
			// ИП может иметь PersonType = legal, но заполненные фамилию/имя/отчество,
			// поэтому поиск по ФИО не ограничиваем типом контрагента
			var query = _unitOfWork.GetAll<Counterparty>();

			if(!string.IsNullOrWhiteSpace(surname))
			{
				query = query
					.Where(c => c.Surname != null && c.Surname.Trim().ToLower() == surname);
			}

			if(!string.IsNullOrWhiteSpace(firstName))
			{
				query = query
					.Where(c => c.FirstName != null && c.FirstName.Trim().ToLower() == firstName);
			}

			if(!string.IsNullOrWhiteSpace(patronymic))
			{
				query = query
					.Where(c => c.Patronymic == null
						|| c.Patronymic.Trim() == string.Empty
						|| c.Patronymic.Trim().ToLower() == patronymic);
			}

			return await query
				.Select(c => c.Id)
				.Take(CounterpartyIdentificationOptions.MaxSearchResults)
				.ToListAsync(cancellationToken);
		}

		/// <inheritdoc />
		public async Task<List<int>> GetCounterpartyIdsByLegalNameAsync(
			string legalName,
			CancellationToken cancellationToken)
		{
			var legalNamePattern = $"%{legalName}%";

			return await _unitOfWork.GetAll<Counterparty>()
				.Where(c => (c.Name != null && SqlMethods.Like(c.Name.ToLower(), legalNamePattern))
					|| (c.FullName != null && SqlMethods.Like(c.FullName.ToLower(), legalNamePattern)))
				.Select(c => c.Id)
				.Take(CounterpartyIdentificationOptions.MaxSearchResults)
				.ToListAsync(cancellationToken);
		}

		/// <inheritdoc />
		public async Task<List<CounterpartyAddressCandidate>> GetAddressCandidatesAsync(
			string street,
			string city,
			string buildingNumber,
			CancellationToken cancellationToken)
		{
			var streetPattern = $"%{street}%";

			var query = _unitOfWork.GetAll<DeliveryPoint>()
				.Where(dp => dp.Counterparty != null
					&& dp.Street != null
					&& SqlMethods.Like(dp.Street.ToLower(), streetPattern));

			if(!string.IsNullOrWhiteSpace(buildingNumber))
			{
				var buildingPattern = $"{buildingNumber}%";
				query = query
					.Where(dp => dp.Building != null && SqlMethods.Like(dp.Building, buildingPattern));
			}

			if(!string.IsNullOrWhiteSpace(city))
			{
				var cityPattern = $"%{city}%";
				query = query
					.Where(dp => dp.City != null && SqlMethods.Like(dp.City.ToLower(), cityPattern));
			}

			var rows = await query
				.Select(dp => new
				{
					dp.Id,
					CounterpartyId = dp.Counterparty.Id,
					dp.City,
					dp.Street,
					dp.StreetType,
					dp.Building,
					dp.Letter,
					dp.Room,
					dp.RoomType,
					dp.Placement,
					dp.CompiledAddress,
					dp.IsActive,
					dp.Counterparty.Name,
					dp.Counterparty.FullName,
					dp.Counterparty.Surname,
					dp.Counterparty.FirstName,
					dp.Counterparty.Patronymic,
					dp.Counterparty.PersonType,
					dp.Counterparty.IsArchive,
					dp.Counterparty.IsDeliveriesClosed
				})
				.Take(CounterpartyIdentificationOptions.MaxAddressCandidates)
				.ToListAsync(cancellationToken);

			return rows
				.Select(x => new CounterpartyAddressCandidate
				{
					Id = x.Id,
					CounterpartyId = x.CounterpartyId,
					City = x.City,
					Street = x.Street,
					StreetType = x.StreetType,
					Building = x.Building,
					Letter = x.Letter,
					Room = x.Room,
					RoomType = x.RoomType,
					Placement = x.Placement,
					CompiledAddress = x.CompiledAddress,
					IsActive = x.IsActive,
					Name = x.Name,
					FullName = x.FullName,
					Surname = x.Surname,
					FirstName = x.FirstName,
					Patronymic = x.Patronymic,
					PersonType = x.PersonType,
					IsArchive = x.IsArchive,
					IsDeliveriesClosed = x.IsDeliveriesClosed
				})
				.ToList();
		}

		/// <inheritdoc />
		public async Task<List<CounterpartySummary>> GetCounterpartiesByIdsAsync(
			IReadOnlyCollection<int> counterpartyIds,
			CancellationToken cancellationToken)
		{
			var ids = counterpartyIds.ToList();

			if(ids.Count == 0)
			{
				return new List<CounterpartySummary>();
			}

			var rows = await _unitOfWork.GetAll<Counterparty>()
				.Where(c => ids.Contains(c.Id))
				.Select(c => new
				{
					c.Id,
					c.Name,
					c.FullName,
					c.Surname,
					c.FirstName,
					c.Patronymic,
					c.PersonType,
					c.IsArchive,
					c.IsDeliveriesClosed
				})
				.ToListAsync(cancellationToken);

			return rows
				.Select(x => new CounterpartySummary
				{
					Id = x.Id,
					Name = x.Name,
					FullName = x.FullName,
					Surname = x.Surname,
					FirstName = x.FirstName,
					Patronymic = x.Patronymic,
					PersonType = x.PersonType,
					IsArchive = x.IsArchive,
					IsDeliveriesClosed = x.IsDeliveriesClosed
				})
				.ToList();
		}

		/// <inheritdoc />
		public async Task<List<CounterpartyPhoneInfo>> GetPhonesAsync(
			IReadOnlyCollection<int> counterpartyIds,
			CancellationToken cancellationToken)
		{
			var ids = counterpartyIds.ToList();

			if(ids.Count == 0)
			{
				return new List<CounterpartyPhoneInfo>();
			}

			var counterpartyPhones = await _unitOfWork.GetAll<Phone>()
				.Where(p => p.Counterparty != null && ids.Contains(p.Counterparty.Id))
				.Select(p => new
				{
					CounterpartyId = p.Counterparty.Id,
					p.Number,
					p.DigitsNumber
				})
				.ToListAsync(cancellationToken);

			var deliveryPointPhones = await _unitOfWork.GetAll<Phone>()
				.Where(p => p.DeliveryPoint != null
					&& p.DeliveryPoint.Counterparty != null
					&& ids.Contains(p.DeliveryPoint.Counterparty.Id))
				.Select(p => new
				{
					CounterpartyId = p.DeliveryPoint.Counterparty.Id,
					p.Number,
					p.DigitsNumber
				})
				.ToListAsync(cancellationToken);

			return counterpartyPhones
				.Select(x => new CounterpartyPhoneInfo
				{
					CounterpartyId = x.CounterpartyId,
					Number = x.Number,
					DigitsNumber = x.DigitsNumber
				})
				.Concat(deliveryPointPhones
					.Select(x => new CounterpartyPhoneInfo
					{
						CounterpartyId = x.CounterpartyId,
						Number = x.Number,
						DigitsNumber = x.DigitsNumber
					}))
				.ToList();
		}

		/// <inheritdoc />
		public async Task<List<CounterpartyDeliveryPointInfo>> GetDeliveryPointsAsync(
			IReadOnlyCollection<int> counterpartyIds,
			CancellationToken cancellationToken)
		{
			var ids = counterpartyIds.ToList();

			if(ids.Count == 0)
			{
				return new List<CounterpartyDeliveryPointInfo>();
			}

			var rows = await _unitOfWork.GetAll<DeliveryPoint>()
				.Where(dp => dp.Counterparty != null && ids.Contains(dp.Counterparty.Id))
				.Select(dp => new
				{
					dp.Id,
					CounterpartyId = dp.Counterparty.Id,
					dp.CompiledAddress,
					dp.City,
					dp.StreetType,
					dp.Street,
					dp.Building,
					dp.Letter,
					dp.Room,
					dp.RoomType,
					dp.Placement,
					dp.IsActive
				})
				.Take(CounterpartyIdentificationOptions.MaxSearchResults
					* CounterpartyIdentificationOptions.MaxDeliveryPointsPerCounterparty)
				.ToListAsync(cancellationToken);

			return rows
				.Select(x => new CounterpartyDeliveryPointInfo
				{
					Id = x.Id,
					CounterpartyId = x.CounterpartyId,
					CompiledAddress = x.CompiledAddress,
					City = x.City,
					StreetType = x.StreetType,
					Street = x.Street,
					Building = x.Building,
					Letter = x.Letter,
					Room = x.Room,
					RoomType = x.RoomType,
					Placement = x.Placement,
					IsActive = x.IsActive
				})
				.ToList();
		}

		/// <inheritdoc />
		public async Task<List<CounterpartyOrderInfo>> GetOrdersAsync(
			int counterpartyId,
			CancellationToken cancellationToken)
		{
			var rows = await _unitOfWork.GetAll<Order>()
				.Where(o => o.Client.Id == counterpartyId)
				.OrderByDescending(o => o.DeliveryDate)
				.Take(CounterpartyIdentificationOptions.MaxOrders)
				.Select(o => new
				{
					o.Id,
					o.DeliveryDate,
					o.OrderStatus,
					o.OrderPaymentStatus
				})
				.ToListAsync(cancellationToken);

			return rows
				.Select(x => new CounterpartyOrderInfo
				{
					Id = x.Id,
					DeliveryDate = x.DeliveryDate,
					OrderStatus = x.OrderStatus,
					OrderPaymentStatus = x.OrderPaymentStatus
				})
				.ToList();
		}

		/// <inheritdoc />
		public async Task<bool> CounterpartyExistsAsync(
			int counterpartyId,
			CancellationToken cancellationToken)
		{
			return await _unitOfWork.GetAll<Counterparty>()
				.AnyAsync(c => c.Id == counterpartyId, cancellationToken);
		}
	}
}

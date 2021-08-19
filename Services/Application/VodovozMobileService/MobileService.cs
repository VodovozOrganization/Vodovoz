using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using NLog;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Payments;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Parameters;
using VodovozMobileService.DTO;

namespace VodovozMobileService
{
#if DEBUG
	[ServiceBehavior(IncludeExceptionDetailInFaults = true)]
#endif
	public class MobileService : IMobileService
	{
		static Logger logger = LogManager.GetCurrentClassLogger();
		private readonly INomenclatureRepository _nomenclatureRepository =
			new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
		public static string BaseUrl { get; set; }

		public MobileService()
		{
		}

		public List<NomenclatureDTO> GetGoods(CatalogType type)
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot($"[MB]Получение каталога товаров {type}")) {
				var types = Enum.GetValues(typeof(MobileCatalog))
								.Cast<MobileCatalog>()
								.Where(x => x.ToString().StartsWith(type.ToString()))
								.ToArray();

				var list = _nomenclatureRepository.GetNomenclatureWithPriceForMobileApp(uow, types);
				if(type == CatalogType.Water)
					list = list.OrderByDescending(n => n.Weight)
						.ThenBy(n => n.NomenclaturePrice.Any() ? n.NomenclaturePrice.Max(p => p.Price) : 0)
						.ToList();
				else
					list = list.OrderBy(n => (int)n.MobileCatalog)
						.ThenBy(n => n.NomenclaturePrice.Any() ? n.NomenclaturePrice.Max(p => p.Price) : 0)
						.ToList();

				var listDto = list.Select(n => new NomenclatureDTO(n)).ToList();

				var imageIds = _nomenclatureRepository.GetNomenclatureImagesIds(uow, list.Select(x => x.Id).ToArray());
				listDto.Where(dto => imageIds.ContainsKey(dto.Id))
					.ToList()
					.ForEach(dto => dto.imagesIds = imageIds[dto.Id]);
				return listDto;
			}
		}

		public Stream GetImage(string filename)
		{
			if(!filename.EndsWith(".jpg")) {
				WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
				return ReturnErrorInStream("Support only .jpg images.");
			}

			int id;
			string number = filename.Substring(0, filename.Length - 4);
			Console.WriteLine(number);
			if(!int.TryParse(number, out id)) {
				Console.WriteLine(id);
				WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
				return ReturnErrorInStream($"Can't parse {number} as image id.");
			}

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot($"[MB]Получение картинки {id}")) {
				var image = uow.GetById<NomenclatureImage>(id);
				if(image == null) {
					WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NotFound;
					return ReturnErrorInStream($"Nomenclature Image with id:{id} not found");
				}

				using(MemoryStream ms = new MemoryStream(image.Image)) {
					WebOperationContext.Current.OutgoingResponse.ContentType = "image/jpeg";
					WebOperationContext.Current.OutgoingResponse.Headers.Add("Content-disposition", $"inline; filename={id}.jpg");
					ms.Position = 0;
					return ms;
				}
			}
		}

		Stream ReturnErrorInStream(string error)
		{
			WebOperationContext.Current.OutgoingResponse.ContentType = "text/plain";
			return new MemoryStream(Encoding.UTF8.GetBytes(error));
		}

		public Func<MobileOrderDTO, int> SaveAndGetIdTestGap;
		int SaveAndGetId(MobileOrderDTO mobileOrder)
		{
			int resId = -1;

			if(mobileOrder == null) {
				logger.Error("[MB]Отсутсвует заказ");
				return resId;
			}

			if(mobileOrder.OrderId > 0) {
				//реализовать для изменения заказа
				logger.Error(string.Format("[MB]Запрос на измение мобильного заказа '{0}'. Пока не реализованно.", mobileOrder.OrderId));
				return resId;
			}

			if(!mobileOrder.IsOrderSumValid()) {
				logger.Error(string.Format("[MB]Неправильная сумма заказа: \"{0}\"", mobileOrder.OrderSum));
				return resId;
			}

			if(!mobileOrder.IsUuidValid()) {
				logger.Error(string.Format("[MB]Неправильный Uuid: \"{0}\"", mobileOrder.UuidRaw));
				return resId;
			}

			if(SaveAndGetIdTestGap != null)
				return SaveAndGetIdTestGap(mobileOrder);

			using(var uow = UnitOfWorkFactory.CreateWithNewRoot<OrderIdProviderForMobileApp>($"[MB]Регистрация заказа для '{mobileOrder.GetUuid()}' на сумму '{mobileOrder.OrderSum}'")) {
				uow.Root.Uuid = mobileOrder.GetUuid();
				uow.Root.OrderSum = mobileOrder.OrderSum;
				try {
					uow.Save();
					resId = uow.Root.Id;
				}
				catch(Exception ex) {
					logger.Error(string.Format("[MB]Ошибка при сохранении: {0}", ex.Message));
					throw ex;
				}
			}
			return resId;
		}

		public CreateOrderResponseDTO Order(MobileOrderDTO ord)
		{
			var id = SaveAndGetId(ord);
			if(WebOperationContext.Current?.OutgoingResponse != null)
				WebOperationContext.Current.OutgoingResponse.StatusCode = id > 0 ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;
			return new CreateOrderResponseDTO(id, ord.UuidRaw);
		}
	}
}
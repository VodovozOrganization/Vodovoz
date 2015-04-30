using QSOrmProject.Deletion;
using System.Collections.Generic;

namespace Vodovoz
{
	partial class MainClass
	{
		static void ConfigureDeletion ()
		{
			logger.Info ("Настройка параметров удаления...");

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(CounterpartyStatus),
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "{1}",
				ClearItems = new List<ClearDependenceInfo> {
					ClearDependenceInfo.Create<Counterparty> (item => item.Status)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(EquipmentColors),
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "{1}",
				ClearItems = new List<ClearDependenceInfo> {
					ClearDependenceInfo.Create<Nomenclature> (item => item.Color)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(EquipmentType),
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "{1}",
				DeleteItems = new List<DeleteDependenceInfo> {
					DeleteDependenceInfo.Create<FreeRentPackage> (item => item.EquipmentType),
					DeleteDependenceInfo.Create<Nomenclature> (item => item.Type),
					DeleteDependenceInfo.Create<PaidRentPackage> (item => item.EquipmentType)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(LogisticsArea),
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "{1}",
				ClearItems = new List<ClearDependenceInfo> {
					ClearDependenceInfo.Create<DeliveryPoint> (item => item.LogisticsArea)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(Manufacturer),
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "{1}",
				ClearItems = new List<ClearDependenceInfo> {
					ClearDependenceInfo.Create<Nomenclature> (item => item.Manufacturer)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(MeasurementUnits),
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "{1}",
				ClearItems = new List<ClearDependenceInfo> {
					ClearDependenceInfo.Create<Nomenclature> (item => item.Unit),
					ClearDependenceInfo.Create<OrderItem> (item => item.Units)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(Nationality),
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "{1}",
				ClearItems = new List<ClearDependenceInfo> {
					ClearDependenceInfo.Create<Employee> (item => item.Nationality)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(NomenclaturePrice),
				SqlSelect = "SELECT id, price, min_count FROM @tablename ",
				DisplayString = "{1:C} (от {2})"
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(Significance),
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "{1}",
				ClearItems = new List<ClearDependenceInfo> {
					ClearDependenceInfo.Create<Counterparty> (item => item.Significance)
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(Warehouse),
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "{1}",
				DeleteItems = new List<DeleteDependenceInfo> {
					DeleteDependenceInfo.Create<IncomingInvoice> (item => item.Warehouse)
					//FIXME добавить складские операции.
				}
			}.FillFromMetaInfo ()
			);

			//<!--Прочие справочники-->

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(AdditionalAgreement),
				SqlSelect = "SELECT id, number FROM @tablename ",
				DisplayString = "Доп. соглашение №{1}",
				ClearItems = new List<ClearDependenceInfo> {
					ClearDependenceInfo.Create<OrderItem> (item => item.AdditionalAgreement)
				}
			}.FillFromMetaInfo ()
			);


			//не закончено.
			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(Nomenclature),
				SqlSelect = "SELECT id, name FROM @tablename ",
				DisplayString = "{1}",
				DeleteItems = new List<DeleteDependenceInfo> {
					DeleteDependenceInfo.CreateFromBag<Nomenclature> (item => item.NomenclaturePrice),
				}
			}.FillFromMetaInfo ()
			);

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(CounterpartyContract),
				SqlSelect = "SELECT id, issue_date FROM @tablename ",
				DisplayString = "Договор №{0} от {1:d}",
				DeleteItems = new List<DeleteDependenceInfo> {
					DeleteDependenceInfo.Create<AdditionalAgreement> (item => item.Contract),
				}
			}.FillFromMetaInfo ()
			);


			/*	DeleteConfig.AddDeleteInfo (new DeleteInfo{
				ObjectClass = typeof(Contract),
				TableName = "contracts",
				ObjectsName = "Договора",
				ObjectName = "договор",
				SqlSelect = "SELECT number, sign_date, lessees.name as lessee, contracts.id as id FROM contracts " +
					"LEFT JOIN lessees ON lessees.id = lessee_id ",
				DisplayString = "Договор №{0} от {1:d} с арендатором {2}",
				DeleteItems = new List<DeleteDependenceInfo>{
					new DeleteDependenceInfo ("contract_docs", "WHERE contract_id = @id "),
					new DeleteDependenceInfo ("files", "WHERE item_group = 'contracts' AND item_id = @id")
				}
			});

			DeleteConfig.AddDeleteInfo (new DeleteInfo {
				ObjectClass = typeof(Lessee),
				TableName = "lessees",
				ObjectsName = "Арендаторы",
				ObjectName = "арендатора",
				SqlSelect = "SELECT name, id FROM lessees ",
				DisplayString = "Арендатор {0}",
				DeleteItems = new List<DeleteDependenceInfo> {
					new DeleteDependenceInfo (typeof(Contract), "WHERE lessee_id = @id ")
				}
			});

			DeleteConfig.AddDeleteInfo (new DeleteInfo{
				ObjectClass = typeof(DocPattern),
				TableName = "doc_patterns",
				ObjectsName = "Шаблоны документов",
				ObjectName = "шаблон",
				SqlSelect = "SELECT name, id FROM doc_patterns ",
				DisplayString = "Шаблон <{0}>",
				ClearItems = new List<ClearDependenceInfo>{
					new ClearDependenceInfo ("contract_docs", "WHERE pattern_id = @id", "pattern_id")
				}
			});

			DeleteConfig.AddDeleteInfo (new DeleteInfo{
				TableName = "files",
				ObjectsName = "Файлы",
				ObjectName = "файл",
				SqlSelect = "SELECT name, id FROM files ",
				DisplayString = "Фаил <{0}>",
			});

			DeleteConfig.AddDeleteInfo (new DeleteInfo{
				ObjectClass = typeof(ContractType),
				TableName = "contract_types",
				ObjectsName = "Типы договоров",
				ObjectName = "тип договора",
				SqlSelect = "SELECT name, id FROM contract_types ",
				DisplayString = "{0}",
				DeleteItems = new List<DeleteDependenceInfo>{
					new DeleteDependenceInfo (typeof(DocPattern), "WHERE contract_type_id = @id")
				},
				ClearItems = new List<ClearDependenceInfo>{
					new ClearDependenceInfo (typeof(Contract), "WHERE contract_type_id = @id", "contract_type_id")
				}
			});

			DeleteConfig.AddDeleteInfo (new DeleteInfo{
				ObjectClass = typeof(Stead),
				TableName = "stead",
				ObjectsName = "Земельные участки",
				ObjectName = "земельный участок",
				SqlSelect = "SELECT name, id, address FROM stead ",
				DisplayString = "{0} {2}",
				ClearItems = new List<ClearDependenceInfo>{
					new ClearDependenceInfo (typeof(Place), "WHERE stead_id = @id", "stead_id")
				}
			});

			DeleteConfig.AddDeleteInfo (new DeleteInfo{
				TableName = "contract_docs",
				ObjectsName = "Документы",
				ObjectName = "измененый документа",
				SqlSelect = "SELECT name, id FROM contract_docs ",
				DisplayString = "Документ <{0}>"
			});

			DeleteConfig.AddDeleteInfo (new DeleteInfo{
				ObjectClass = typeof(ContractCategory),
				TableName = "contract_category",
				ObjectsName = "Категории договоров",
				ObjectName = "категория",
				SqlSelect = "SELECT name, id FROM contract_category ",
				DisplayString = "{0}",
				ClearItems = new List<ClearDependenceInfo>{
					new ClearDependenceInfo (typeof(Contract), "WHERE category_id = @id", "category_id")
				}
			});

			DeleteConfig.AddDeleteInfo (new DeleteInfo{
				ObjectClass = typeof(Organization),
				TableName = "organizations",
				ObjectsName = "Организации",
				ObjectName = "организацию",
				SqlSelect = "SELECT name, id FROM organizations ",
				DisplayString = "{0}",
				DeleteItems = new List<DeleteDependenceInfo>{
					new DeleteDependenceInfo (typeof(Contract), "WHERE org_id = @id ")
				},
				ClearItems = new List<ClearDependenceInfo>{
					new ClearDependenceInfo (typeof(Place), "WHERE org_id = @id", "org_id")
				}
			});

			DeleteConfig.AddDeleteInfo (new DeleteInfo{
				ObjectClass = typeof(PlaceType),
				TableName = "place_types",
				ObjectsName = "Типы мест",
				ObjectName = "тип места",
				SqlSelect = "SELECT name, description, id FROM place_types ",
				DisplayString = "{0} - {1}",
				DeleteItems = new List<DeleteDependenceInfo>{
					new DeleteDependenceInfo (typeof(Place), "WHERE type_id = @id")
				}
			});

			DeleteConfig.AddDeleteInfo (new DeleteInfo{
				ObjectClass = typeof(User),
				TableName = "users",
				ObjectsName = "Пользователи",
				ObjectName = "пользователя",
				SqlSelect = "SELECT name, id FROM users ",
				DisplayString = "{0}",
				ClearItems = new List<ClearDependenceInfo>{
					new ClearDependenceInfo (typeof(Contract), "WHERE responsible_id = @id", "responsible_id"),
					new ClearDependenceInfo (typeof(QSHistoryLog.HistoryChangeSet), "WHERE user_id = @id", "user_id")
				}
			});

			DeleteConfig.AddDeleteInfo (new DeleteInfo{
				ObjectClass = typeof(QSHistoryLog.HistoryChangeSet),
				TableName = "history_changeset",
				ObjectsName = "Журнал действий",
				ObjectName = "действие пользователя",
				SqlSelect = "SELECT datetime, object_title, id FROM history_changeset ",
				DisplayString = "Изменено {1} в {0}"
			});
	*/
			logger.Info ("Ок");
		}
	}
}

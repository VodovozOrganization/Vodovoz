using System;
using System.Collections.Generic;
using QSProjectsLib;

namespace Vodovoz
{
	partial class MainClass
	{
		static void CreateProjectParam()
		{
			QSMain.AdminFieldName = "admin";
			QSMain.ProjectPermission = new Dictionary<string, UserPermission>();
			//QSMain.ProjectPermission.Add ("edit_slips", new UserPermission("edit_slips", "Изменение кассы задним числом",
			//                                                             "Пользователь может изменять или добавлять кассовые документы задним числом."));

			QSMain.User = new UserInfo();

			//Параметры удаления
			Dictionary<string, TableInfo> Tables = new Dictionary<string, TableInfo>();
			QSMain.ProjectTables = Tables;
			TableInfo PrepareTable;

			PrepareTable = new TableInfo();
			PrepareTable.ObjectsName = "Места";
			PrepareTable.ObjectName = "место";
			PrepareTable.SqlSelect = "SELECT place_types.name as type, place_no, area , type_id FROM places " +
				"LEFT JOIN place_types ON places.type_id = place_types.id ";
			PrepareTable.DisplayString = "Место {0}-{1} с площадью {2} кв.м.";
			PrepareTable.PrimaryKey = new  TableInfo.PrimaryKeys("type_id", "place_no");
			PrepareTable.DeleteItems.Add ("contracts", 
				new TableInfo.DeleteDependenceItem ("WHERE contracts.place_type_id = @type_id AND contracts.place_no = @place_no", "@place_no", "@type_id"));
			Tables.Add ("places", PrepareTable);

			PrepareTable = new TableInfo();
			PrepareTable.ObjectsName = "Договора"; 
			PrepareTable.ObjectName = "договор"; 
			PrepareTable.SqlSelect = "SELECT number, sign_date, lessees.name as lessee, contracts.id as id FROM contracts " +
				"LEFT JOIN lessees ON lessees.id = lessee_id ";
			PrepareTable.DisplayString = "Договор №{0} от {1:d} с арендатором {2}";
			PrepareTable.PrimaryKey = new TableInfo.PrimaryKeys("id");
			PrepareTable.DeleteItems.Add ("contract_docs", 
				new TableInfo.DeleteDependenceItem ("WHERE contract_id = @contract_id ", "", "@contract_id"));
			PrepareTable.DeleteItems.Add ("files", 
				new TableInfo.DeleteDependenceItem ("WHERE item_group = 'contracts' AND item_id = @contract_id ", "", "@contract_id"));
			Tables.Add ("contracts", PrepareTable);

			PrepareTable = new TableInfo();
			PrepareTable.ObjectsName = "Арендаторы";
			PrepareTable.ObjectName = "арендатора"; 
			PrepareTable.SqlSelect = "SELECT name, id FROM lessees ";
			PrepareTable.DisplayString = "Арендатор {0}";
			PrepareTable.PrimaryKey = new TableInfo.PrimaryKeys("id");
			PrepareTable.DeleteItems.Add ("contracts", 
				new TableInfo.DeleteDependenceItem ("WHERE lessee_id = @lessee_id ", "", "@lessee_id"));
			Tables.Add ("lessees", PrepareTable);

			PrepareTable = new TableInfo();
			PrepareTable.ObjectsName = "Шаблоны документов";
			PrepareTable.ObjectName = "шаблон"; 
			PrepareTable.SqlSelect = "SELECT name, id FROM doc_patterns ";
			PrepareTable.DisplayString = "шаблон <{0}>";
			PrepareTable.PrimaryKey = new TableInfo.PrimaryKeys("id");
			PrepareTable.ClearItems.Add ("contract_docs", 
				new TableInfo.ClearDependenceItem ("WHERE pattern_id = @id", "", "@id", "pattern_id"));
			Tables.Add ("doc_patterns", PrepareTable);

			PrepareTable = new TableInfo();
			PrepareTable.ObjectsName = "Файлы";
			PrepareTable.ObjectName = "файл"; 
			PrepareTable.SqlSelect = "SELECT name, id FROM files ";
			PrepareTable.DisplayString = "Фаил <{0}>";
			PrepareTable.PrimaryKey = new TableInfo.PrimaryKeys("id");
			Tables.Add ("files", PrepareTable);

			PrepareTable = new TableInfo();
			PrepareTable.ObjectsName = "Типы договоров";
			PrepareTable.ObjectName = "тип договора"; 
			PrepareTable.SqlSelect = "SELECT name, id FROM contract_types ";
			PrepareTable.DisplayString = "{0}";
			PrepareTable.PrimaryKey = new TableInfo.PrimaryKeys("id");
			PrepareTable.DeleteItems.Add ("doc_patterns", 
				new TableInfo.DeleteDependenceItem ("WHERE contract_type_id = @id", "", "@id"));
			PrepareTable.ClearItems.Add ("contracts", 
				new TableInfo.ClearDependenceItem ("WHERE contract_type_id = @id", "", "@id", "contract_type_id"));
			Tables.Add ("contract_types", PrepareTable);

			PrepareTable = new TableInfo();
			PrepareTable.ObjectsName = "Земельные участки";
			PrepareTable.ObjectName = "земельный участок"; 
			PrepareTable.SqlSelect = "SELECT name, id, address FROM stead ";
			PrepareTable.DisplayString = "{0} {2}";
			PrepareTable.PrimaryKey = new TableInfo.PrimaryKeys("id");
			PrepareTable.ClearItems.Add ("places", 
				new TableInfo.ClearDependenceItem ("WHERE stead_id = @id", "", "@id", "stead_id"));
			Tables.Add ("stead", PrepareTable);

			PrepareTable = new TableInfo();
			PrepareTable.ObjectsName = "Документы";
			PrepareTable.ObjectName = "измененый документа"; 
			PrepareTable.SqlSelect = "SELECT name, id FROM contract_docs ";
			PrepareTable.DisplayString = "Документ <{0}>";
			PrepareTable.PrimaryKey = new TableInfo.PrimaryKeys("id");
			Tables.Add ("contract_docs", PrepareTable);

			PrepareTable = new TableInfo();
			PrepareTable.ObjectsName = "Категории договоров";
			PrepareTable.ObjectName = "категория"; 
			PrepareTable.SqlSelect = "SELECT name, id FROM contract_category ";
			PrepareTable.DisplayString = "{0}";
			PrepareTable.PrimaryKey = new TableInfo.PrimaryKeys("id");
			PrepareTable.ClearItems.Add ("contracts", 
				new TableInfo.ClearDependenceItem ("WHERE category_id = @id", "", "@id", "category_id"));
			Tables.Add ("contract_category", PrepareTable);

			PrepareTable = new TableInfo();
			PrepareTable.ObjectsName = "Организации";
			PrepareTable.ObjectName = "организацию"; 
			PrepareTable.SqlSelect = "SELECT name, id FROM organizations ";
			PrepareTable.DisplayString = "{0}";
			PrepareTable.PrimaryKey = new TableInfo.PrimaryKeys("id");
			PrepareTable.DeleteItems.Add ("contracts", 
				new TableInfo.DeleteDependenceItem ("WHERE org_id = @id ", "", "@id"));
			PrepareTable.ClearItems.Add ("places", 
				new TableInfo.ClearDependenceItem ("WHERE org_id = @id", "", "@id", "org_id"));
			Tables.Add ("organizations", PrepareTable);

			PrepareTable = new TableInfo();
			PrepareTable.ObjectsName = "Типы мест";
			PrepareTable.ObjectName = "тип места"; 
			PrepareTable.SqlSelect = "SELECT name, description, id FROM place_types ";
			PrepareTable.DisplayString = "{0} - {1}";
			PrepareTable.PrimaryKey = new TableInfo.PrimaryKeys("id");
			PrepareTable.DeleteItems.Add ("places", 
				new TableInfo.DeleteDependenceItem ("WHERE type_id = @id ", "", "@id"));
			Tables.Add ("place_types", PrepareTable);

			PrepareTable = new TableInfo();
			PrepareTable.ObjectsName = "Пользователи";
			PrepareTable.ObjectName = "пользователя"; 
			PrepareTable.SqlSelect = "SELECT name, id FROM users ";
			PrepareTable.DisplayString = "{0}";
			PrepareTable.PrimaryKey = new TableInfo.PrimaryKeys("id");
			PrepareTable.ClearItems.Add ("contracts", 
				new TableInfo.ClearDependenceItem ("WHERE responsible_id = @id", "", "@id", "responsible_id"));
			Tables.Add ("users", PrepareTable);

		}

	}
}

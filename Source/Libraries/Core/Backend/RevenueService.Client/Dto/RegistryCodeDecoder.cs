using System;
using System.Collections.Generic;
using Dadata.Model;

namespace RevenueService.Client.Dto
{
	public static class RegistryCodeDecoder
	{
		/// <summary>
		/// Ключ: "Код статуса|Тип контрагента (INDIVIDUAL, LEGAL)"
		/// </summary>
		private static readonly IReadOnlyDictionary<string, RegistryCodeInfo> _map =
			new Dictionary<string, RegistryCodeInfo>(StringComparer.Ordinal)
			{
				["101|INDIVIDUAL"] = new RegistryCodeInfo(101, PartyType.INDIVIDUAL, PartyStatus.LIQUIDATED, "Отсутствует в связи со смертью"),
				["101|LEGAL"] = new RegistryCodeInfo(101, PartyType.LEGAL, PartyStatus.LIQUIDATING, "Находится в стадии ликвидации"),
				["102|LEGAL"] = new RegistryCodeInfo(102, PartyType.LEGAL, PartyStatus.LIQUIDATING,
					"Находится в процессе ликвидации (решение суда)"),
				["103|INDIVIDUAL"] = new RegistryCodeInfo(103, PartyType.INDIVIDUAL, PartyStatus.LIQUIDATED,
					"Отсутствует в связи с аннулированием документа, подтверждающего право проживать в Российской Федерации"),
				["104|INDIVIDUAL"] = new RegistryCodeInfo(104, PartyType.INDIVIDUAL, PartyStatus.LIQUIDATED,
					"Отсутствует в связи с окончанием срока действия документа, подтверждающего право проживать в Российской Федерации"),
				["105|LEGAL"] = new RegistryCodeInfo(105, PartyType.LEGAL, PartyStatus.LIQUIDATING,
					"Принято решение о предстоящем исключении недействующего юрлица из ЕГРЮЛ"),
				["106|LEGAL"] = new RegistryCodeInfo(106, PartyType.LEGAL, PartyStatus.LIQUIDATING,
					"Принято решение о предстоящем исключении из ЕГРЮЛ (невозможность ликвидации юрлица)"),
				["107|LEGAL"] = new RegistryCodeInfo(107, PartyType.LEGAL, PartyStatus.LIQUIDATING,
					"Принято решение о предстоящем исключении из ЕГРЮЛ (наличие в ЕГРЮЛ сведений о юридическом лице, в отношении которых внесена запись о " +
					"недостоверности)"),
				["108|LEGAL"] = new RegistryCodeInfo(108, PartyType.LEGAL, PartyStatus.LIQUIDATING,
					"Регистрирующим органом принято решение о предстоящем исключении юридического лица из ЕГРЮЛ (наличие оснований, предусмотренных подпунктом " +
					"«г» пункта 5 статьи 21.1 Федерального закона от 08.08.2001 No 129-ФЗ)"),
				["110|INDIVIDUAL"] = new RegistryCodeInfo(110, PartyType.INDIVIDUAL, PartyStatus.LIQUIDATING,
					"Принято решение о предстоящем исключении недействующего ИП из ЕГРИП"),
				["110|LEGAL"] = new RegistryCodeInfo(110, PartyType.LEGAL, PartyStatus.LIQUIDATING,
					"Принято решение о предстоящем исключении из ЕГРЮЛ (наличие оснований, предусмотренных ст.21.3 Федерального закона от 08.08.2001 №129-ФЗ)"),
				["111|LEGAL"] = new RegistryCodeInfo(111, PartyType.LEGAL, PartyStatus.ACTIVE,
					"Находится в процессе уменьшения уставного капитала"),
				["112|LEGAL"] =
					new RegistryCodeInfo(112, PartyType.LEGAL, PartyStatus.ACTIVE, "Принято решение об изменении места нахождения"),
				["113|LEGAL"] = new RegistryCodeInfo(113, PartyType.LEGAL, PartyStatus.BANKRUPT,
					"Возбуждено производство по делу о несостоятельности (банкротстве)"),
				["114|LEGAL"] = new RegistryCodeInfo(114, PartyType.LEGAL, PartyStatus.BANKRUPT,
					"В деле о несостоятельности (банкротстве) введено наблюдение"),
				["115|LEGAL"] = new RegistryCodeInfo(115, PartyType.LEGAL, PartyStatus.BANKRUPT,
					"В деле о несостоятельности (банкротстве) введено финансовое оздоровление"),
				["116|LEGAL"] = new RegistryCodeInfo(116, PartyType.LEGAL, PartyStatus.BANKRUPT,
					"В деле о несостоятельности (банкротстве) введено внешнее управление"),
				["117|LEGAL"] = new RegistryCodeInfo(117, PartyType.LEGAL, PartyStatus.BANKRUPT,
					"Признано несостоятельным (банкротом) и в отношении него открыто конкурсное производство"),
				["121|LEGAL"] = new RegistryCodeInfo(121, PartyType.LEGAL, PartyStatus.ACTIVE,
					"Находится в процессе реорганизации в форме преобразования"),
				["122|LEGAL"] = new RegistryCodeInfo(122, PartyType.LEGAL, PartyStatus.ACTIVE,
					"Находится в процессе реорганизации в форме слияния"),
				["123|LEGAL"] = new RegistryCodeInfo(123, PartyType.LEGAL, PartyStatus.ACTIVE,
					"Находится в процессе реорганизации в форме разделения"),
				["124|LEGAL"] = new RegistryCodeInfo(124, PartyType.LEGAL, PartyStatus.REORGANIZING,
					"Находится в процессе реорганизации в форме присоединения к другому юрлицу"),
				["125|LEGAL"] = new RegistryCodeInfo(125, PartyType.LEGAL, PartyStatus.ACTIVE,
					"Находится в процессе реорганизации в форме разделения, осуществляемой одновременно с присоединением"),
				["129|LEGAL"] = new RegistryCodeInfo(129, PartyType.LEGAL, PartyStatus.REORGANIZING,
					"Находится в процессе реорганизации при одновременном сочетании различных форм реорганизации и прекратит деятельность при ее завершении"),
				["131|LEGAL"] = new RegistryCodeInfo(131, PartyType.LEGAL, PartyStatus.ACTIVE,
					"Находится в процессе реорганизации в форме выделения"),
				["132|LEGAL"] = new RegistryCodeInfo(132, PartyType.LEGAL, PartyStatus.ACTIVE,
					"Находится в процессе реорганизации в форме присоединения к нему других юрлиц"),
				["133|LEGAL"] = new RegistryCodeInfo(133, PartyType.LEGAL, PartyStatus.ACTIVE,
					"Находится в процессе реорганизации в форме присоединения, осуществляемой одновременно с разделением"),
				["134|LEGAL"] = new RegistryCodeInfo(134, PartyType.LEGAL, PartyStatus.ACTIVE,
					"Находится в процессе реорганизации в форме присоединения, осуществляемой одновременно с выделением"),
				["136|LEGAL"] = new RegistryCodeInfo(136, PartyType.LEGAL, PartyStatus.ACTIVE,
					"Находится в процессе реорганизации в форме выделения, осуществляемой одновременно с присоединением"),
				["139|LEGAL"] = new RegistryCodeInfo(139, PartyType.LEGAL, PartyStatus.ACTIVE,
					"Находится в процессе реорганизации при одновременном сочетании различных форм реорганизации и продолжит деятельность при ее завершении"),
				["201|INDIVIDUAL"] = new RegistryCodeInfo(201, PartyType.INDIVIDUAL, PartyStatus.LIQUIDATED,
					"Прекратил деятельность по собственному решению"),
				["201|LEGAL"] = new RegistryCodeInfo(201, PartyType.LEGAL, PartyStatus.LIQUIDATED, "Ликвидация"),
				["202|INDIVIDUAL"] = new RegistryCodeInfo(202, PartyType.INDIVIDUAL, PartyStatus.LIQUIDATED,
					"Прекратил деятельность в связи со смертью"),
				["202|LEGAL"] = new RegistryCodeInfo(202, PartyType.LEGAL, PartyStatus.LIQUIDATED, "Ликвидация по решению суда"),
				["203|INDIVIDUAL"] = new RegistryCodeInfo(203, PartyType.INDIVIDUAL, PartyStatus.LIQUIDATED,
					"Прекратил деятельность в связи с принятием судом решения о признании банкротом"),
				["203|LEGAL"] = new RegistryCodeInfo(203, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Прекращение деятельности в связи с ликвидацией на основании определения арбитражного суда о завершении конкурсного производства"),
				["204|INDIVIDUAL"] = new RegistryCodeInfo(204, PartyType.INDIVIDUAL, PartyStatus.LIQUIDATED,
					"Прекратил деятельность в принудительном порядке по решению суда"),
				["205|INDIVIDUAL"] = new RegistryCodeInfo(205, PartyType.INDIVIDUAL, PartyStatus.LIQUIDATED,
					"Прекратил деятельность в связи с вступлением в силу приговора суда"),
				["206|INDIVIDUAL"] = new RegistryCodeInfo(206, PartyType.INDIVIDUAL, PartyStatus.LIQUIDATED,
					"Прекратил деятельность в связи с аннулированием документа, подтверждающего право проживать в Российской Федерации"),
				["207|INDIVIDUAL"] = new RegistryCodeInfo(207, PartyType.INDIVIDUAL, PartyStatus.LIQUIDATED,
					"Прекратил деятельность в связи с окончанием срока действия документа, подтверждающего право проживать в Российской Федерации"),
				["209|INDIVIDUAL"] =
					new RegistryCodeInfo(209, PartyType.INDIVIDUAL, PartyStatus.LIQUIDATED, "Недействующий ИП исключен из ЕГРИП"),
				["301|INDIVIDUAL"] = new RegistryCodeInfo(301, PartyType.INDIVIDUAL, PartyStatus.LIQUIDATED,
					"Прекратил деятельность по решению членов КФХ"),
				["301|LEGAL"] = new RegistryCodeInfo(301, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Прекращение деятельности путем реорганизации в форме преобразования"),
				["302|INDIVIDUAL"] = new RegistryCodeInfo(302, PartyType.INDIVIDUAL, PartyStatus.LIQUIDATED,
					"Прекратил деятельность на основании решения суда"),
				["302|LEGAL"] = new RegistryCodeInfo(302, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Прекращение деятельности путем реорганизации в форме слияния"),
				["303|INDIVIDUAL"] = new RegistryCodeInfo(303, PartyType.INDIVIDUAL, PartyStatus.LIQUIDATED,
					"Прекратил деятельность в связи с принятием судом решения о признании банкротом"),
				["303|LEGAL"] = new RegistryCodeInfo(303, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Прекращение деятельности путем реорганизации в форме разделения"),
				["304|LEGAL"] = new RegistryCodeInfo(304, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Прекращение деятельности путем реорганизации в форме присоединения"),
				["305|LEGAL"] = new RegistryCodeInfo(305, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Прекращение деятельности путем реорганизации в форме присоединения, осуществляемой одновременно с разделением"),
				["306|LEGAL"] = new RegistryCodeInfo(306, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Прекращение деятельности путем реорганизации в форме присоединения, осуществляемой одновременно с выделением"),
				["307|LEGAL"] = new RegistryCodeInfo(307, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Прекращение деятельности путем реорганизации в форме разделения, осуществляемой одновременно с присоединением"),
				["308|LEGAL"] = new RegistryCodeInfo(308, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Прекращение деятельности путем реорганизации в форме слияния, осуществляемой одновременно с разделением"),
				["309|LEGAL"] = new RegistryCodeInfo(309, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Прекращение деятельности путем реорганизации в форме слияния, осуществляемой одновременно с выделением"),
				["310|LEGAL"] = new RegistryCodeInfo(310, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Прекращение деятельности путем реорганизации в форме разделения, осуществляемой одновременно со слиянием"),
				["313|LEGAL"] = new RegistryCodeInfo(313, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Прекращение деятельности путем реорганизации в форме разделения при реорганизации с одновременным сочетанием различных ее форм"),
				["314|LEGAL"] = new RegistryCodeInfo(314, PartyType.LEGAL, PartyStatus.LIQUIDATED, "Ликвидация"),
				["320|LEGAL"] = new RegistryCodeInfo(320, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Прекращение юрлица одновременно с его созданием при реорганизации с сочетанием различных ее форм"),
				["400|LEGAL"] = new RegistryCodeInfo(400, PartyType.LEGAL, PartyStatus.LIQUIDATED, "Прекращение унитарного предприятия"),
				["401|INDIVIDUAL"] = new RegistryCodeInfo(401, PartyType.INDIVIDUAL, PartyStatus.LIQUIDATED,
					"Прекратил деятельность в связи с созданием производственного кооператива или хозяйственного товарищества"),
				["401|LEGAL"] = new RegistryCodeInfo(401, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Прекращение унитарного предприятия в связи с продажей его имущественного комплекса"),
				["402|LEGAL"] = new RegistryCodeInfo(402, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Прекращение юрлица, имущественный комплекс которого внесен в качестве вклада в уставный капитал открытого акционерного общества"),
				["403|LEGAL"] = new RegistryCodeInfo(403, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Прекращение КФХ в связи с приобретением главой КФХ статуса ИП без образования юрлица"),
				["404|LEGAL"] = new RegistryCodeInfo(404, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Прекращение деятельности общественного объединения в качестве юрлица по решению суда на основании ст.29 Федерального закона " +
					"от 19.05.1995 №82-ФЗ"),
				["405|LEGAL"] = new RegistryCodeInfo(405, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Прекращение деятельности религиозной организации в качестве юрлица по решению суда на основании ст.8 Федерального закона от " +
					"26.09.1997 №125-ФЗ"),
				["406|LEGAL"] = new RegistryCodeInfo(406, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Прекращение политической партии на основании п.6 ст.15 Федерального закона от 11.07.2001 №95-ФЗ"),
				["407|LEGAL"] =
					new RegistryCodeInfo(407, PartyType.LEGAL, PartyStatus.LIQUIDATED, "Исключение из ЕГРЮЛ недействующего юрлица"),
				["408|LEGAL"] = new RegistryCodeInfo(408, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Ликвидация политической партии по решению суда на основании ст.41 Федерального закона от 11.07.2001 №95-ФЗ"),
				["409|LEGAL"] = new RegistryCodeInfo(409, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Ликвидация регионального отделения политической партии по решению суда на основании ст.42 Федерального закона от 11.07.2001 №95-ФЗ"),
				["410|LEGAL"] = new RegistryCodeInfo(410, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Ликвидация регионального отделения политической партии в связи с ликвидацией политической партии по решению суда"),
				["411|LEGAL"] = new RegistryCodeInfo(411, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Прекращение деятельности регионального отделения политической партии в связи с реорганизацией политической партии в форме преобразования"),
				["412|LEGAL"] = new RegistryCodeInfo(412, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Ликвидация регионального отделения политической партии в связи с ликвидацией политической партии"),
				["413|LEGAL"] = new RegistryCodeInfo(413, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Ликвидация некоммерческой организации по решению суда"),
				["414|LEGAL"] = new RegistryCodeInfo(414, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Исключение из ЕГРЮЛ в связи с невозможностью ликвидации"),
				["415|LEGAL"] = new RegistryCodeInfo(415, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Исключение из ЕГРЮЛ в связи наличием сведений, в отношении которых внесена запись о недостоверности"),
				["418|LEGAL"] = new RegistryCodeInfo(418, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Исключение юридического лица из ЕГРЮЛ в связи с наличием оснований, предусмотренных подпунктом «г» пункта 5 статьи 21.1 Федерального " +
					"закона от 08.08.2001 № 129-ФЗ «О государственной регистрации юридических лиц и индивидуальных предпринимателей»"),
				["420|LEGAL"] = new RegistryCodeInfo(420, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Исключение из ЕГРЮЛ на основании ст.21.3 Федерального закона от 08.08.2001 №129-ФЗ"),
				["501|INDIVIDUAL"] = new RegistryCodeInfo(501, PartyType.INDIVIDUAL, PartyStatus.LIQUIDATED,
					"Утратил государственную регистрацию на основании ст.3 Федерального закона от 23.06.2003 №76-ФЗ"),
				["501|LEGAL"] = new RegistryCodeInfo(501, PartyType.LEGAL, PartyStatus.LIQUIDATED, "Ликвидация"),
				["502|INDIVIDUAL"] = new RegistryCodeInfo(502, PartyType.INDIVIDUAL, PartyStatus.LIQUIDATED,
					"Утратил государственную регистрацию на основании ст.3 Федерального закона от 23.06.2003 №76-ФЗ"),
				["601|INDIVIDUAL"] = new RegistryCodeInfo(601, PartyType.INDIVIDUAL, PartyStatus.LIQUIDATED,
					"Регистрация признана недействительной на основании п.4 ст.22.1 Федерального закона от 08.08.2001 №129-ФЗ"),
				["602|INDIVIDUAL"] = new RegistryCodeInfo(602, PartyType.INDIVIDUAL, PartyStatus.LIQUIDATED,
					"Регистрация признана недействительной на основании п.4 ст.22.1 Федерального закона от 08.08.2001 №129-ФЗ"),
				["701|INDIVIDUAL"] = new RegistryCodeInfo(701, PartyType.INDIVIDUAL, PartyStatus.LIQUIDATED,
					"Регистрация признана недействительной по решению суда"),
				["701|LEGAL"] = new RegistryCodeInfo(701, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Регистрация признана недействительной по решению суда"),
				["702|INDIVIDUAL"] = new RegistryCodeInfo(702, PartyType.INDIVIDUAL, PartyStatus.LIQUIDATED,
					"Регистрация признана недействительной по решению суда"),
				["702|LEGAL"] = new RegistryCodeInfo(702, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Внесение в ЕГРЮЛ сведений о юрлице, зарегистрированном до 01.07.2002, признано недействительным по решению суда"),
				["801|INDIVIDUAL"] = new RegistryCodeInfo(801, PartyType.INDIVIDUAL, PartyStatus.LIQUIDATED,
					"Регистрация признана ошибочной по решению регистрирующего органа"),
				["801|LEGAL"] = new RegistryCodeInfo(801, PartyType.LEGAL, PartyStatus.LIQUIDATED,
					"Регистрация признана ошибочной по решению регистрирующего органа"),
				["802|INDIVIDUAL"] = new RegistryCodeInfo(802, PartyType.INDIVIDUAL, PartyStatus.LIQUIDATED,
					"Регистрация признана ошибочной по решению регистрирующего органа"),
			};

		private static string Key(string code, PartyType type) => $"{code}|{type}";

		public static RegistryCodeInfo Decode(string code, PartyType type)
		{
			if(string.IsNullOrWhiteSpace(code) || type == null)
			{
				return null;
			}

			if(!_map.TryGetValue(Key(code, type), out var info))
			{
				throw new KeyNotFoundException($"Не найдена комбинация код={code}, тип={type}.");
			}

			return info;
		}

		public static bool TryDecode(string code, PartyType type, out RegistryCodeInfo info)
			=> _map.TryGetValue(Key(code, type), out info);

		public static IReadOnlyCollection<RegistryCodeInfo> All => (IReadOnlyCollection<RegistryCodeInfo>)_map.Values;
	}
}

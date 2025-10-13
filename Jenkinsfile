//-----------------------------------------------------------------------

//Описание
//Развертывание - это распаковка сборки из архива в каталоги для проведения тестов, развертывание не подразумевает взаимодействие с рабочими каталогами
//Публикация - это распаковка сборки из архива в рабочие каталоги
//	Публикация хотфикса - распаковка сразу в рабочий каталог с новой датой, запуск пользователями будет происходить сразу же после завершения публикации
//	Публикация релиза - распаковка в каталог для подготовки к новому релизу, после проведения полноценного обновления этот релиз будет перенесен в рабочий каталог скриптом обновления.

//-----------------------------------------------------------------------

// Оглавление
//
// 100	Настройки
// 101		Идентификаторы нод
// 102		Глобальные
// 103		Подготовка репозитория
// 104		Восстановление пакетов
// 105		Сборка
// 106		Архивация
// 107		Доставка
// 108		Развертывание
// 109		Публикация	
//
// 200	Этапы
// 201		Подготовка репозитория
// 202		Восстановление пакетов
// 203		Сборка
// 204		Запаковка
// 205		Доставка
// 206		Развертывание
// 207		Публикация
//
// 300	Фукнции
// 301		Подготовка репозитория
// 302		Восстановление пакетов
// 303		Сборка
// 304		Запаковка
// 305		Доставка
// 306		Развертывание
// 307		Публикация
// 308		Утилитарные

//-----------------------------------------------------------------------

// 100	Настройки
// 101	Настройки. Идентификаторы нод
NODE_VOD1 = "Vod1"
NODE_VOD3 = "Vod3"
NODE_VOD5 = "Vod5"
NODE_VOD6 = "Vod6"
NODE_VOD7 = "Vod7"
NODE_VOD13 = "Vod13"
NODE_WIN_BUILD = "WIN_BUILD"
NODE_DOCKER_BUILD = "DOCKER_BUILD"

// 102.1	Настройки. Глобальные
ARCHIVE_EXTENTION = '.7z'
APP_PATH = "Vodovoz/Source/Applications"
WEB_BUILD_OUTPUT_CATALOG = "bin/Release/net5.0_publish"
WIN_BUILD_TOOL = "C:/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe"
DESKTOP_WATER_DELIVERY_PATH = "C:/Program Files (x86)/Vodovoz/WaterDelivery"
DESKTOP_WORK_PATH = "${DESKTOP_WATER_DELIVERY_PATH}/Work"
UPDATE_LOCK_FILE = "${DESKTOP_WORK_PATH}/current.lock"
JOB_FOLDER_NAME = GetJobFolderName()
DOCKER_REGISTRY = "docker.vod.qsolution.ru:5100"

// 102.2	Настройки. Вычисляемые
GIT_BRANCH = env.BRANCH_NAME
JENKINS_BRANCH_NAME = env.BRANCH_NAME

// 102.3	Настройки. Флаги:
IS_PULL_REQUEST = env.CHANGE_ID != null
IS_DEVELOP = GIT_BRANCH == 'develop'
IS_HOTFIX = GIT_BRANCH == 'master'
IS_RELEASE = GIT_BRANCH ==~ /^[Rr]elease(.*?)/
IS_MANUAL_BUILD = GIT_BRANCH ==~ /^manual-build(.*?)/

// 103	Настройки. Подготовка репозитория

// 104	Настройки. Восстановление пакетов

// 105	Настройки. Сборка

ARTIFACT_DATE_TIME = new Date().format("MMdd_HHmm")
CAN_BUILD_DESKTOP = true
CAN_BUILD_WEB = true
CAN_PUBLISH_BUILD_WEB = IS_HOTFIX || IS_RELEASE

// 106	Настройки. Архивация
CAN_COMPRESS_DESKTOP = CAN_BUILD_DESKTOP && (IS_HOTFIX || IS_RELEASE || IS_DEVELOP || IS_PULL_REQUEST || IS_MANUAL_BUILD || GIT_BRANCH == 'Beta')
CAN_COMPRESS_WEB = CAN_PUBLISH_BUILD_WEB

// 107.1	Настройки. Доставка
CAN_DELIVERY_DESKTOP = CAN_COMPRESS_DESKTOP
CAN_DELIVERY_WEB = CAN_COMPRESS_WEB
WIN_DELIVERY_SHARED_FOLDER_NAME = "JenkinsWorkspace"

// 107.2	Настройки. Доставка. Пути
DESKTOP_VOD1_DELIVERY_PATH = "\\\\${NODE_VOD1}\\${WIN_DELIVERY_SHARED_FOLDER_NAME}\\${JOB_FOLDER_NAME}"
DESKTOP_VOD3_DELIVERY_PATH = "\\\\${NODE_VOD3}\\${WIN_DELIVERY_SHARED_FOLDER_NAME}\\${JOB_FOLDER_NAME}"
DESKTOP_VOD5_DELIVERY_PATH = "\\\\${NODE_VOD5}\\${WIN_DELIVERY_SHARED_FOLDER_NAME}\\${JOB_FOLDER_NAME}"
DESKTOP_VOD7_DELIVERY_PATH = "\\\\${NODE_VOD7}\\${WIN_DELIVERY_SHARED_FOLDER_NAME}\\${JOB_FOLDER_NAME}"
DESKTOP_VOD13_DELIVERY_PATH = "\\\\${NODE_VOD13}\\${WIN_DELIVERY_SHARED_FOLDER_NAME}\\${JOB_FOLDER_NAME}"
WEB_DELIVERY_PATH = "\\\\${NODE_VOD6}\\${WIN_DELIVERY_SHARED_FOLDER_NAME}\\${JOB_FOLDER_NAME}"

// 108	Настройки. Развертывание
DEPLOY_PATH = "F:/WORK/_BUILDS"
CAN_DEPLOY_FOR_TEST_DESKTOP = CAN_DELIVERY_DESKTOP && (GIT_BRANCH == 'Beta' || IS_PULL_REQUEST || IS_MANUAL_BUILD || IS_DEVELOP)
CAN_DEPLOY_FOR_USERS_DESKTOP = CAN_DELIVERY_DESKTOP && (IS_HOTFIX || IS_RELEASE)

// 109	Настройки. Публикация	
CAN_PUBLISH_WEB = CAN_DELIVERY_WEB
//Release потому что правила именования фиксов/релизов Release_MMDD_HHMM
NEW_DESKTOP_HOTFIX_FOLDER_NAME_PREFIX = "Release"
NEW_WEB_HOTFIX_FOLDER_NAME = "Hotfix"
NEW_RELEASE_FOLDER_NAME = "NewRelease"
DESKTOP_HOTFIX_PUBLISH_PATH = DESKTOP_WORK_PATH
DESKTOP_NEW_RELEASE_PUBLISH_PATH = "${DESKTOP_WATER_DELIVERY_PATH}/${NEW_RELEASE_FOLDER_NAME}"
WEB_PUBLISH_PATH = "E:/CD"


//-----------------------------------------------------------------------

// 200	Этапы

stage('Log') {

	echo "101	Настройки. Идентификаторы нод:"
	echo "Node Vod1: ${NODE_VOD1}"
	echo "Node Vod3: ${NODE_VOD3}"
	echo "Node Vod5: ${NODE_VOD5}"
	echo "Node Vod6: ${NODE_VOD6}"
	echo "Node Vod7: ${NODE_VOD7}"
	echo "Node Vod13: ${NODE_VOD13}"
	echo "Node Win Build: ${NODE_WIN_BUILD}"
	echo "Node Docker Build: ${NODE_DOCKER_BUILD}"

	echo "102.1	Настройки. Глобальные:"
	echo "Archive Extention: ${ARCHIVE_EXTENTION}"
	echo "App Path: ${APP_PATH}"
	echo "Web Build Output Catalog: ${WEB_BUILD_OUTPUT_CATALOG}"
	echo "Win Build Tool: ${WIN_BUILD_TOOL}"
	echo "Desktop Water Delivery Path: ${DESKTOP_WATER_DELIVERY_PATH}"
	echo "Desktop Work Path: ${DESKTOP_WORK_PATH}"
	echo "Update Lock File: ${UPDATE_LOCK_FILE}"
	echo "Job Folder Name: ${JOB_FOLDER_NAME}"

	echo "102.2	Настройки. Вычисляемые:"
	echo "Git Branch: ${GIT_BRANCH}"
	echo "Jenkins Branch Name: ${JENKINS_BRANCH_NAME}"
	echo "Change ID: ${env.CHANGE_ID}"

	echo "102.3	Настройки. Флаги:"
	echo "Is Pull Request: ${IS_PULL_REQUEST}"
	echo "Is Develop: ${IS_DEVELOP}"
	echo "Is Hotfix: ${IS_HOTFIX}"
	echo "Is Release: ${IS_RELEASE}"
	echo "Is Manual Build: ${IS_MANUAL_BUILD}"

	echo "103	Настройки. Подготовка репозитория"

	echo "104	Настройки. Восстановление пакетов"

	echo "105	Настройки. Сборка"
	echo "Artifact date time postfix: ${ARTIFACT_DATE_TIME}"
	echo "Can Build Desktop: ${CAN_BUILD_DESKTOP}"
	echo "Can Build Web: ${CAN_BUILD_WEB}"
	echo "Can Publish Build Web: ${CAN_PUBLISH_BUILD_WEB}"

	echo "106	Настройки. Архивация"
	echo "Can Compress Desktop: ${CAN_COMPRESS_DESKTOP}"
	echo "Can Compress Web: ${CAN_COMPRESS_WEB}"

	echo "107.1	Настройки. Доставка"
	echo "Can Delivery Desktop: ${CAN_DELIVERY_DESKTOP}"
	echo "Can Delivery Web: ${CAN_DELIVERY_WEB}"

	echo "107.2	Настройки. Доставка. Пути"
	echo "Win Delivery Shared Folder Name: ${WIN_DELIVERY_SHARED_FOLDER_NAME}"
	echo "Desktop Vod1 Delivery Path: ${DESKTOP_VOD1_DELIVERY_PATH}"
	echo "Desktop Vod3 Delivery Path: ${DESKTOP_VOD3_DELIVERY_PATH}"
	echo "Desktop Vod5 Delivery Path: ${DESKTOP_VOD5_DELIVERY_PATH}"
	echo "Desktop Vod7 Delivery Path: ${DESKTOP_VOD7_DELIVERY_PATH}"
	echo "Desktop Vod13 Delivery Path: ${DESKTOP_VOD13_DELIVERY_PATH}"
	echo "Web Delivery Path: ${WEB_DELIVERY_PATH}"

	echo "108	Настройки. Развертывание"
	echo "Deploy Path: ${DEPLOY_PATH}"
	echo "Can Deploy Desktop for test: ${CAN_DEPLOY_FOR_TEST_DESKTOP}"
	echo "Can Deploy Desktop for users: ${CAN_DEPLOY_FOR_USERS_DESKTOP}"

	echo "109	Настройки. Публикация"
	echo "Can Publish Web: ${CAN_PUBLISH_WEB}"

	echo "New Desktop Hotfix Folder Name Prefix: ${NEW_DESKTOP_HOTFIX_FOLDER_NAME_PREFIX}"
	echo "New Web Hotfix Folder Name: ${NEW_WEB_HOTFIX_FOLDER_NAME}"
	echo "New Release Folder Name: ${NEW_RELEASE_FOLDER_NAME}"
	echo "Desktop Hotfix Publish Path: ${DESKTOP_HOTFIX_PUBLISH_PATH}"
	echo "Desktop New Release Publish Path: ${DESKTOP_NEW_RELEASE_PUBLISH_PATH}"
	echo "Web Publish Path: ${WEB_PUBLISH_PATH}"
}

stage('Stop previous builds') {
	def decodedBranchName = java.net.URLDecoder.decode(GIT_BRANCH, "UTF-8")
	def jobName = "Vodovoz/Vodovoz/${decodedBranchName}"
	def job = Jenkins.get().getItemByFullName(jobName)
	if (job == null) {
		echo "Job ${jobName} not found"
		return
	}
	def currentBuildNumber = currentBuild.number.toInteger()
	if (job.builds != null && !job.builds.isEmpty()) {
		job.builds.each { build ->
			if (build.isBuilding() &&
				build.number.toInteger() < currentBuildNumber) {
					build.finish(hudson.model.Result.ABORTED, new java.io.IOException("Aborting build"))
			}
		}
	} else {
		echo "No builds found for job ${jobName}"
	}
}

// 201	Этапы. Подготовка репозитория
stage('Checkout'){
	parallel (
		"Win" : {
			node(NODE_WIN_BUILD){
				PrepareSources()
			}
		}
	)
}

stage('Desktop'){
	node(NODE_WIN_BUILD){
		if(CAN_BUILD_DESKTOP)
		{
			stage('Desktop.Restore'){
				bat "\"${WIN_BUILD_TOOL}\" Vodovoz/Source/Vodovoz.sln /t:Restore /p:Configuration=DebugWin /p:Platform=x86 /maxcpucount:2"
			}

			stage('Desktop.Build'){
				Build("WinDesktop")
				bat "copy \"D:\\CD\\WaterDelivery\\appsettings.Production.json\" \".\\Vodovoz\\Source\\Applications\\Desktop\\Vodovoz\\bin\\DebugWin\\\""
			}
		}
		else
		{
			echo "Build Desktop not needed"
		}
	}
}

// 203	Этапы. Сборка
stage('Web'){
	node(NODE_WIN_BUILD){
		
		if(CAN_PUBLISH_BUILD_WEB)
		{
			stage('Web.Restore'){
				bat "\"${WIN_BUILD_TOOL}\" Vodovoz/Source/Vodovoz.sln /t:Restore /p:Configuration=Release /p:Platform=x86 /maxcpucount:2"
			}
			stage('Web.Build'){
				// Docker
				DockerPublishBuild("${APP_PATH}/Backend/WebAPI/FastPaymentsAPI/FastPaymentsAPI.csproj")
				DockerPublishBuild("${APP_PATH}/Frontend/UnsubscribePage/UnsubscribePage.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/WebAPI/DeliveryRulesService/DeliveryRulesService.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/WebAPI/RoboatsService/RoboatsService.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/WebAPI/CustomerAppsApi/CustomerAppsApi.csproj")

				DockerPublishBuild("${APP_PATH}/Backend/WebAPI/DriverAPI/DriverAPI.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/WebAPI/CashReceiptApi/CashReceiptApi.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/CustomerOnlineOrdersRegistrar/CustomerOnlineOrdersRegistrar.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/CustomerOnlineOrdersStatusUpdateNotifier/CustomerOnlineOrdersStatusUpdateNotifier.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/DatabaseServiceWorker/DatabaseServiceWorker.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/EmailWorkers/EmailPrepareWorker/EmailPrepareWorker.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/EmailWorkers/EmailSendWorker/EmailSendWorker.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/EmailWorkers/EmailStatusUpdateWorker/EmailStatusUpdateWorker.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/ExternalCounterpartyAssignNotifier/ExternalCounterpartyAssignNotifier.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/FastDeliveryLateWorker/FastDeliveryLateWorker.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/WebAPI/LogisticsEventsApi/LogisticsEventsApi.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Workers/Vodovoz.SmsInformerWorker/Vodovoz.SmsInformerWorker.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/WebAPI/Warehouse/WarehouseApi/WarehouseApi.csproj")
				DockerPublishBuild("${APP_PATH}/Frontend/PayPageAPI/PayPageAPI.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Workers/IIS/TrueMarkCodePoolCheckWorker/TrueMarkCodePoolCheckWorker.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Workers/IIS/RoboatsCallsWorker/RoboatsCallsWorker.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/PushNotificationsWorker/PushNotificationsWorker.csproj")				
				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/ScannedTrueMarkCodesDelayedProcessingWorker/ScannedTrueMarkCodesDelayedProcessingWorker.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Services/Pacs.Admin.Service/Pacs.Admin.Service.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Services/Pacs.Calls.Service/Pacs.Calls.Service.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Services/Pacs.Operator.Service/Pacs.Operator.Service.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/WebAPI/RobotMia/Api/Api.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/WebAPI/CustomerOrdersApi/CustomerOrdersApi.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/WebAPI/EarchiveApi/EarchiveApi.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/WebAPI/Mango.Api.Service/Mango.Api.Service.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/WebAPI/Mango.Service/Mango.Service.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/WebAPI/Sms.Internal.Service/Sms.Internal.Service.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/WebAPI/Email/MailganerEventsDistributorApi/MailganerEventsDistributorApi.csproj")

				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/Edo/Edo.Admin.Worker/Edo.Admin.Worker.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/Edo/Edo.CodesSaver.Worker/Edo.CodesSaver.Worker.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/Edo/Edo.Docflow.Worker/Edo.Docflow.Worker.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/Edo/Edo.Document.Worker/Edo.Document.Worker.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/Edo/Edo.Receipt.Api/Edo.Receipt.Api.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/Edo/Edo.Receipt.Dispatcher.Worker/Edo.Receipt.Dispatcher.Worker.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/Edo/Edo.Receipt.Sender.Worker/Edo.Receipt.Sender.Worker.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/Edo/Edo.Scheduler.Worker/Edo.Scheduler.Worker.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/Edo/Edo.Tender.Worker/Edo.Tender.Worker.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/Edo/Edo.Transfer.Dispatcher.Worker/Edo.Transfer.Dispatcher.Worker.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/Edo/Edo.Transfer.Routine.Worker/Edo.Transfer.Routine.Worker.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/Edo/Edo.Transfer.Sender.Worker/Edo.Transfer.Sender.Worker.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/Edo/Edo.Withdrawal.Worker/Edo.Withdrawal.Worker.csproj")

				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/EdoServices/EdoDocumentsPreparer/EdoDocumentsPreparer.csproj")
				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/EdoServices/EdoDocumentsConsumer/EdoDocumentsConsumer.csproj")

				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/EdoServices/EdoAutoSendReceiveWorker/EdoAutoSendReceiveWorker.csproj")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/EdoAutoSendReceiveWorker/EdoAutoSendReceiveWorker.csproj", "registry-prod-beverages-world")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/EdoAutoSendReceiveWorker/EdoAutoSendReceiveWorker.csproj", "registry-prod-kuler-service")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/EdoAutoSendReceiveWorker/EdoAutoSendReceiveWorker.csproj", "registry-prod-non-alcoholic-beverages-world")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/EdoAutoSendReceiveWorker/EdoAutoSendReceiveWorker.csproj", "registry-prod-vv-north")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/EdoAutoSendReceiveWorker/EdoAutoSendReceiveWorker.csproj", "registry-prod-vv-south")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/EdoAutoSendReceiveWorker/EdoAutoSendReceiveWorker.csproj", "registry-prod-vv-chiruk")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/EdoAutoSendReceiveWorker/EdoAutoSendReceiveWorker.csproj", "registry-prod-vv-east")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/EdoAutoSendReceiveWorker/EdoAutoSendReceiveWorker.csproj", "registry-prod-vv-deshic")

				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/EdoServices/EdoContactsUpdater/EdoContactsUpdater.csproj")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/EdoContactsUpdater/EdoContactsUpdater.csproj", "registry-prod-kuler-service")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/EdoContactsUpdater/EdoContactsUpdater.csproj", "registry-prod-non-alcoholic-beverages-world")

				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/EdoServices/EdoDocumentFlowUpdater/EdoDocumentFlowUpdater.csproj")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/EdoDocumentFlowUpdater/EdoDocumentFlowUpdater.csproj", "registry-prod-beverages-world")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/EdoDocumentFlowUpdater/EdoDocumentFlowUpdater.csproj", "registry-prod-kuler-service")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/EdoDocumentFlowUpdater/EdoDocumentFlowUpdater.csproj", "registry-prod-non-alcoholic-beverages-world")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/EdoDocumentFlowUpdater/EdoDocumentFlowUpdater.csproj", "registry-prod-vv-north")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/EdoDocumentFlowUpdater/EdoDocumentFlowUpdater.csproj", "registry-prod-vv-south")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/EdoDocumentFlowUpdater/EdoDocumentFlowUpdater.csproj", "registry-prod-vv-chiruk")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/EdoDocumentFlowUpdater/EdoDocumentFlowUpdater.csproj", "registry-prod-vv-east")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/EdoDocumentFlowUpdater/EdoDocumentFlowUpdater.csproj", "registry-prod-vv-deshic")

				DockerPublishBuild("${APP_PATH}/Backend/Workers/Docker/EdoServices/TaxcomEdoConsumer/TaxcomEdoConsumer.csproj")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/TaxcomEdoConsumer/TaxcomEdoConsumer.csproj", "registry-prod-beverages-world")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/TaxcomEdoConsumer/TaxcomEdoConsumer.csproj", "registry-prod-kuler-service")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/TaxcomEdoConsumer/TaxcomEdoConsumer.csproj", "registry-prod-non-alcoholic-beverages-world")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/TaxcomEdoConsumer/TaxcomEdoConsumer.csproj", "registry-prod-vv")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/TaxcomEdoConsumer/TaxcomEdoConsumer.csproj", "registry-prod-vv-north")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/TaxcomEdoConsumer/TaxcomEdoConsumer.csproj", "registry-prod-vv-south")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/TaxcomEdoConsumer/TaxcomEdoConsumer.csproj", "registry-prod-vv-chiruk")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/TaxcomEdoConsumer/TaxcomEdoConsumer.csproj", "registry-prod-vv-east")
				DockerPublishBuildWithCustomProfileName("${APP_PATH}/Backend/Workers/Docker/EdoServices/TaxcomEdoConsumer/TaxcomEdoConsumer.csproj", "registry-prod-vv-deshic")
			}
		}
		else if(CAN_BUILD_WEB)
		{
			stage('Web.Restore'){
				bat "\"${WIN_BUILD_TOOL}\" Vodovoz/Source/Vodovoz.sln /t:Restore /p:Configuration=Web /p:Platform=x86 /maxcpucount:2"
			}
			stage('Web.Build'){
				//Сборка для проверки что нет ошибок, собранные проекты выкладывать не нужно
				Build("Web")
			}
		}
		else
		{
			echo "Build Web not needed"
		}
		
	}
}


// 204	Этапы. Запаковка
stage('Compress'){
	parallel(
		"Desktop" : { CompressDesktopArtifact() },
	)
}

// 205	Этапы. Доставка
stage('Delivery'){
	parallel(

		// Desktop
		"Desktop ${NODE_VOD1}" : { DeliveryDesktopArtifact(NODE_VOD1, DESKTOP_VOD1_DELIVERY_PATH) },
		"Desktop ${NODE_VOD3}" : { DeliveryDesktopArtifact(NODE_VOD3, DESKTOP_VOD3_DELIVERY_PATH) },
		"Desktop ${NODE_VOD5}" : { DeliveryDesktopArtifact(NODE_VOD5, DESKTOP_VOD5_DELIVERY_PATH) },
		"Desktop ${NODE_VOD7}" : { DeliveryDesktopArtifact(NODE_VOD7, DESKTOP_VOD7_DELIVERY_PATH) },
		"Desktop ${NODE_VOD13}" : { DeliveryDesktopArtifact(NODE_VOD13, DESKTOP_VOD13_DELIVERY_PATH) },
	)
}

// 206	Этапы. Развертывание
stage('Deploy'){
	node(NODE_VOD3){
		DeployDesktopForTest()
	}
}

// 207	Этапы. Публикация
stage('Publish'){
	parallel(
		"Desktop ${NODE_VOD1}" : { PublishDesktop(NODE_VOD1) },
		"Desktop ${NODE_VOD3}" : { PublishDesktop(NODE_VOD3) },
		"Desktop ${NODE_VOD5}" : { PublishDesktop(NODE_VOD5) },
		"Desktop ${NODE_VOD7}" : { PublishDesktop(NODE_VOD7) },
		"Desktop ${NODE_VOD13}" : { PublishDesktop(NODE_VOD13) },
	)
}

stage('CleanUp'){
	parallel(
		"Desktop ${NODE_VOD1}" : { DeleteCompressedArtifactAtNode(NODE_VOD1, "VodovozDesktop")  },
		"Desktop ${NODE_VOD3}" : { DeleteCompressedArtifactAtNode(NODE_VOD3, "VodovozDesktop") },
		"Desktop ${NODE_VOD5}" : { DeleteCompressedArtifactAtNode(NODE_VOD5, "VodovozDesktop") },
		"Desktop ${NODE_VOD7}" : { DeleteCompressedArtifactAtNode(NODE_VOD7, "VodovozDesktop") },
		"Desktop ${NODE_VOD13}" : { DeleteCompressedArtifactAtNode(NODE_VOD13, "VodovozDesktop") },
		"Desktop ${NODE_WIN_BUILD}" : { DeleteCompressedArtifactAtNode(NODE_WIN_BUILD, "VodovozDesktop") },
		"Desktop ${NODE_WIN_BUILD}" : { DeleteCompressedArtifactAtNode(NODE_WIN_BUILD, "VodovozDesktop") },
	)

	if(IS_RELEASE){
		WinRemoveOldJenkinsTempFiles();
	}
}

//-----------------------------------------------------------------------

// 300	Фукнции

// 301	Фукнции. Подготовка репозитория

def PrepareSources() {
	def REFERENCE_REPOSITORY_PATH = "${JENKINS_HOME_NODE}/workspace/_VODOVOZ_REFERENCE_REPOSITORY"
	echo "Prepare reference repository ${REFERENCE_REPOSITORY_PATH}"

	if (fileExists(REFERENCE_REPOSITORY_PATH)) {
		// fetch all on reference repository
		if (isUnix()) {
			sh script: """\
				cd ${REFERENCE_REPOSITORY_PATH} \
				git fetch --all \
				cd ${REFERENCE_REPOSITORY_PATH}/modules/Source/Libraries/External/GMap.NET \
				git fetch --all \
				cd ${REFERENCE_REPOSITORY_PATH}/modules/Source/Libraries/External/Gtk.DataBindings \
				git fetch --all \
				cd ${REFERENCE_REPOSITORY_PATH}/modules/Source/Libraries/External/My-FyiReporting \
				git fetch --all \
				cd ${REFERENCE_REPOSITORY_PATH}/modules/Source/Libraries/External/QSProjects \
				git fetch --all \
			""", returnStdout: true
		} else {
			RunPowerShell("""
				cd ${REFERENCE_REPOSITORY_PATH}
				git fetch --all
				cd ${REFERENCE_REPOSITORY_PATH}/modules/Source/Libraries/External/GMap.NET
				git fetch --all
				cd ${REFERENCE_REPOSITORY_PATH}/modules/Source/Libraries/External/Gtk.DataBindings
				git fetch --all
				cd ${REFERENCE_REPOSITORY_PATH}/modules/Source/Libraries/External/My-FyiReporting
				git fetch --all
				cd ${REFERENCE_REPOSITORY_PATH}/modules/Source/Libraries/External/QSProjects
				git fetch --all
			""")
		}		
	} else {
		// clone reference
		if (isUnix()) {
			sh script: """\
				git clone https://github.com/VodovozOrganization/Vodovoz.git --mirror ${REFERENCE_REPOSITORY_PATH} \
				git clone https://github.com/QualitySolution/GMap.NET.git --mirror ${REFERENCE_REPOSITORY_PATH}/modules/Source/Libraries/External/GMap.NET \
				git clone https://github.com/QualitySolution/Gtk.DataBindings.git --mirror ${REFERENCE_REPOSITORY_PATH}/modules/Source/Libraries/External/Gtk.DataBindings \
				git clone https://github.com/QualitySolution/My-FyiReporting.git --mirror ${REFERENCE_REPOSITORY_PATH}/modules/Source/Libraries/External/My-FyiReporting \
				git clone https://github.com/QualitySolution/QSProjects.git --mirror ${REFERENCE_REPOSITORY_PATH}/modules/Source/Libraries/External/QSProjects \
			""", returnStdout: true
		} else {
			RunPowerShell("""
				git clone https://github.com/VodovozOrganization/Vodovoz.git --mirror ${REFERENCE_REPOSITORY_PATH}
				git clone https://github.com/QualitySolution/GMap.NET.git --mirror ${REFERENCE_REPOSITORY_PATH}/modules/Source/Libraries/External/GMap.NET
				git clone https://github.com/QualitySolution/Gtk.DataBindings.git --mirror ${REFERENCE_REPOSITORY_PATH}/modules/Source/Libraries/External/Gtk.DataBindings
				git clone https://github.com/QualitySolution/My-FyiReporting.git --mirror ${REFERENCE_REPOSITORY_PATH}/modules/Source/Libraries/External/My-FyiReporting
				git clone https://github.com/QualitySolution/QSProjects.git --mirror ${REFERENCE_REPOSITORY_PATH}/modules/Source/Libraries/External/QSProjects
			""")
		}
	}

	checkout changelog: false, poll: false, scm:([
		$class: 'GitSCM',
		branches: scm.branches,
		extensions: scm.extensions
		+ [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'Vodovoz']]
		+ [[$class: 'CloneOption', reference: "${REFERENCE_REPOSITORY_PATH}"]]
		+ [[$class: 'SubmoduleOption', disableSubmodules: false, recursiveSubmodules: true, parentCredentials: true]],
		userRemoteConfigs: scm.userRemoteConfigs
	])
}

// 302	Фукнции. Восстановление пакетов



// 303	Фукнции. Сборка

def PublishBuild(projectPath){
	bat "\"${WIN_BUILD_TOOL}\" ${projectPath} /t:Publish /p:Configuration=Release /p:PublishProfile=FolderProfile /maxcpucount:2"
}

def DockerPublishBuild(projectPath){
	def workspacePath = GetWorkspacePath()
	bat "\"${WIN_BUILD_TOOL}\" ${workspacePath}/${projectPath} -restore:True /t:Publish /p:Configuration=Release /p:PublishProfile=registry-prod /maxcpucount:2"
}

def Build(config){
	bat "\"${WIN_BUILD_TOOL}\" Vodovoz/Source/Vodovoz.sln /t:Build /p:Configuration=${config} /p:Platform=x86 /maxcpucount:2 /nodeReuse:false"
}

def DockerFileBuild(projectPath, containerRepository){
	def workspacePath = GetWorkspacePath()
	bat "docker build -t ${containerRepository}:latest -f ${workspacePath}/${projectPath}/Dockerfile ${workspacePath}/${projectPath}"
}

def DockerPushAs(fromContainerRepository, toRemoteContainerRepository) {
	bat "docker tag ${fromContainerRepository}:latest ${DOCKER_REGISTRY}/${toRemoteContainerRepository}:latest"
	bat "docker push ${DOCKER_REGISTRY}/${toRemoteContainerRepository}:latest"
}

def DockerPublishBuildWithCustomProfileName(projectPath, profileName){
	def workspacePath = GetWorkspacePath()
	bat "\"${WIN_BUILD_TOOL}\" ${workspacePath}/${projectPath} -restore:True /t:Publish /p:Configuration=Release /p:PublishProfile=${profileName} /maxcpucount:2"
}

// 304	Фукнции. Запаковка

def CompressDesktopArtifact(){
	if(CAN_COMPRESS_DESKTOP)
	{
		node(NODE_WIN_BUILD){
			CompressArtifact("${APP_PATH}/Desktop/Vodovoz/bin/DebugWin", "VodovozDesktop") 
		}
	} 
	else
	{
		echo "Compress Desktop artifacts not needed"
	}
}

def CompressWebArtifact(relativeProjectPath){
	if(CAN_COMPRESS_WEB)
	{
		node(NODE_WIN_BUILD){
			def webProjectName = "${GetFolderName(relativeProjectPath)}"
			CompressArtifact("${APP_PATH}/${relativeProjectPath}/${WEB_BUILD_OUTPUT_CATALOG}", webProjectName)
		}
	} 
	else
	{
		echo "Compress Web artifacts not needed"
	}
}

def CompressArtifact(sourcePath, artifactName) {
	def archive_file = "${artifactName}_${ARTIFACT_DATE_TIME}${ARCHIVE_EXTENTION}"

	if (fileExists(archive_file)) {
		echo "Delete exiting artifact ${archive_file} from ${sourcePath}/*"
		fileOperations([fileDeleteOperation(excludes: '', includes: "${archive_file}")])
	}

	echo "Compressing artifact ${archive_file} from ./${sourcePath}/*"
	ZipFiles(sourcePath, archive_file)
}

def DecompressArtifact(destPath, artifactName) {
	def archive_file = "${artifactName}_${ARTIFACT_DATE_TIME}${ARCHIVE_EXTENTION}"

	echo "Decompressing artifact ${archive_file} to ${destPath}"
	UnzipFiles(archive_file, destPath)
}

def DeleteCompressedArtifact(artifactName) {
	def workspacePath = GetWorkspacePath()
	def archive_file = "${artifactName}_${ARTIFACT_DATE_TIME}${ARCHIVE_EXTENTION}"
	if (fileExists(archive_file)) {
		echo "Deleting artifact ${archive_file}"
		WinRemoveDirectory("${workspacePath}/${archive_file}");
	}
}

// 305	Фукнции. Доставка

def DeliveryDesktopArtifact(nodeName, deliveryPath){
	def nodeIsOnline = true;

	jenkins.model.Jenkins.instance.getNodes().each{node ->
		node.getAssignedLabels().each{label ->
			if(label.name == nodeName && node.toComputer().isOffline()){
				nodeIsOnline = false;
				return
			}
		}
	}

	if(!nodeIsOnline){
		unstable("${nodeName} - publish failed! node is offline")
		return
	}

	if(CAN_DELIVERY_DESKTOP)
	{
		DeliveryWinArtifact("VodovozDesktop_${ARTIFACT_DATE_TIME}${ARCHIVE_EXTENTION}", deliveryPath)
	}
	else
	{
		echo "Delivery Desktop artifact to ${deliveryPath} not needed"
	}
}

def DeliveryWebArtifact(projectName){
	if(CAN_DELIVERY_WEB)
	{
		DeliveryWinArtifact("${projectName}_${ARTIFACT_DATE_TIME}${ARCHIVE_EXTENTION}", WEB_DELIVERY_PATH)
	}
	else
	{
		echo "Delivery ${projectName} artifact  not needed"
	}
}

def DeliveryWinArtifact(artifactName, deliveryPath){
	node(NODE_WIN_BUILD){
		def workspacePath = GetWorkspacePath()
		RunPowerShell("""
			New-Item -ItemType File -Path "${deliveryPath}\\${artifactName}" -Force
			Copy-Item -Path "${workspacePath}/${artifactName}" -Destination "${deliveryPath}\\${artifactName}" -Force
		""")
	}
}

// 306	Фукнции. Развертывание

def DeployDesktopForTest(){
	if(CAN_DEPLOY_FOR_TEST_DESKTOP)
	{
		def OUTPUT_PATH = ""

		if(IS_PULL_REQUEST)
		{
			OUTPUT_PATH = "${DEPLOY_PATH}/pull_requests/${env.CHANGE_ID}"
		}
		else
		{
			OUTPUT_PATH = "${DEPLOY_PATH}/${GIT_BRANCH}"
		}

		DecompressArtifact(OUTPUT_PATH, 'VodovozDesktop')
	}
	else
	{
		echo "Deploy desktop builds not needed"
	}
}

// 307	Фукнции. Публикация

def PublishDesktop(nodeName){
	def nodeIsOnline = true;

	jenkins.model.Jenkins.instance.getNodes().each{node ->
		node.getAssignedLabels().each{label ->
			if(label.name == nodeName && node.toComputer().isOffline()){
				nodeIsOnline = false;
				return
			}
		}
	}

	if(!nodeIsOnline){
		unstable("${nodeName} - publish failed! node is offline")
		return
	}

	node(nodeName){
		if(CAN_DEPLOY_FOR_USERS_DESKTOP){
			if(IS_HOTFIX){
				def hotfixName = "${NEW_DESKTOP_HOTFIX_FOLDER_NAME_PREFIX}_${ARTIFACT_DATE_TIME}"
				def newHotfixPath = "${DESKTOP_HOTFIX_PUBLISH_PATH}/${hotfixName}"
				DecompressArtifact(newHotfixPath, 'VodovozDesktop')
				LockHotfix(hotfixName)
				return
			}

			if(IS_RELEASE){
				DecompressArtifact(DESKTOP_NEW_RELEASE_PUBLISH_PATH, 'VodovozDesktop')
				return
			}
		}
		echo "Publish not needed"
	}
}

def PublishWeb(projectName){
	def nodeIsOnline = true;

	jenkins.model.Jenkins.instance.getNodes().each{node ->
		node.getAssignedLabels().each{label ->
			if(label.name == NODE_VOD6 && node.toComputer().isOffline()){
				nodeIsOnline = false;
				return
			}
		}
	}

	if(!nodeIsOnline){
		unstable("${NODE_VOD6} - publish failed! node is offline")
		return
	}

	node(NODE_VOD6){
		if(CAN_PUBLISH_WEB)
		{
			if(IS_HOTFIX)
			{
				def newHotfixPath = "${WEB_PUBLISH_PATH}/${projectName}/${NEW_WEB_HOTFIX_FOLDER_NAME}"
				DecompressArtifact(newHotfixPath, projectName)
				return
			}

			if(IS_RELEASE)
			{
				def newReleasePath = "${WEB_PUBLISH_PATH}/${projectName}/${NEW_RELEASE_FOLDER_NAME}"
				DecompressArtifact(newReleasePath, projectName)
				return
			}
		}

		echo "Publish not needed"
	}
}

def DeleteCompressedArtifactAtNode(nodeName, projectName) {
	def nodeIsOnline = true;

	jenkins.model.Jenkins.instance.getNodes().each{node ->
		node.getAssignedLabels().each{label ->
			if(label.name == nodeName && node.toComputer().isOffline()){
				nodeIsOnline = false;
				return
			}
		}
	}

	if(!nodeIsOnline){
		unstable("${nodeName} - cleanup failed! node is offline")
		return
	}

	node(nodeName){
		if(CAN_COMPRESS_DESKTOP){
			DeleteCompressedArtifact(projectName)
			return
		}
		echo "Cleanup not needed"
	}
}

// 308	Фукнции. Утилитарные

def LockHotfix(hotfixName){
	writeFile(file: UPDATE_LOCK_FILE, text: hotfixName)
}

def ZipFiles(sourcePath, archiveFile){
	def workspacePath = GetWorkspacePath()
	if (isUnix()) {
		sh "7z a -stl -mx1 ${workspacePath}/${archiveFile} ${workspacePath}/${sourcePath}/*"
	}
	else {
		bat "7z a -stl -mx1 ${workspacePath}/${archiveFile} ${workspacePath}/${sourcePath}/*"
	}
}

def UnzipFiles(archiveFile, destPath){
	def workspacePath = GetWorkspacePath()
	if (isUnix()) {
		sh "7z x -y -o\"${destPath}\" ${workspacePath}/${archiveFile}"
	}
	else {
		bat "7z x -y -o\"${destPath}\" ${workspacePath}/${archiveFile}"
	}
}

def RunPowerShell(psScript){
	powershell"""
		\$ErrorActionPreference = "Stop";
		${psScript}
        """
}

def GetWorkspacePath()  {
	if (isUnix()) {
		return "${JENKINS_HOME_NODE}/workspace/${JOB_FOLDER_NAME}"
	}
	else {
		return "${JENKINS_HOME_NODE}/workspace/${JOB_FOLDER_NAME}"
	}
}

def GetJobFolderName(){
	node(NODE_WIN_BUILD){
		return GetFolderNameFromWinPath(env.WORKSPACE)
	}
}

def GetFolderNameFromWinPath(folderPath){
	splitted = folderPath.split("\\\\")
	folderName = splitted[splitted.length-1]
	return folderName
}

def GetFolderName(folderPath){
	splitted = folderPath.split("/")
	folderName = splitted[splitted.length-1]
	return folderName
}

def WinRemoveDirectory(destPath){
	RunPowerShell("""
		Remove-Item -LiteralPath "${destPath}" -Force -Recurse
	""")
}

def WinRemoveOldJenkinsTempFiles() {
    node(NODE_WIN_BUILD){
    	RunPowerShell("""
        Get-ChildItem 'C:\\Users\\jenkins\\AppData\\Local\\Temp\\Containers\\Content' -File |
        Where-Object { \$file = \$_; \$file.LastWriteTime -lt (Get-Date).AddDays(-7) } |
        ForEach-Object { Remove-Item \$file.FullName -Force }
        """)
    }
}

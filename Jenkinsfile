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
NODE_WIN_BUILD = "WIN_BUILD"
NODE_LINUX_BUILD = "LINUX_BUILD"
NODE_LINUX_RUNTIME = "LINUX_RUNTIME"

// 102	Настройки. Глобальные
ARCHIVE_EXTENTION = '.7z'
APP_PATH = "Vodovoz/Source/Applications"
WCF_BUILD_OUTPUT_CATALOG = "bin/Debug"
WEB_BUILD_OUTPUT_CATALOG = "bin/Release/net5.0_publish"
WIN_BUILD_TOOL = "C:/Program Files (x86)/Microsoft Visual Studio/2019/Community/MSBuild/Current/Bin/MSBuild.exe"
DESKTOP_WATER_DELIVERY_PATH = "C:/Program Files (x86)/Vodovoz/WaterDelivery"
DESKTOP_WORK_PATH = "${DESKTOP_WATER_DELIVERY_PATH}/Work"
UPDATE_LOCK_FILE = "${DESKTOP_WORK_PATH}/current.lock"
LINUX_BUILD_TOOL = "msbuild"
JOB_FOLDER_NAME = GetJobFolderName()
IS_PULL_REQUEST = env.CHANGE_ID != null
IS_HOTFIX = true//env.BRANCH_NAME == 'master'
IS_RELEASE = env.BRANCH_NAME ==~ /^[Rr]elease(.*?)/
IS_MANUAL_BUILD = env.BRANCH_NAME ==~ /^manual-build(.*?)/

// 103	Настройки. Подготовка репозитория

// 104	Настройки. Восстановление пакетов

// 105	Настройки. Сборка
CAN_BUILD_DESKTOP = true
CAN_BUILD_WEB = true
CAN_PUBLISH_BUILD_WEB = IS_HOTFIX || IS_RELEASE
CAN_BUILD_WCF = true

// 106	Настройки. Архивация
CAN_COMPRESS_DESKTOP = CAN_BUILD_DESKTOP && (IS_HOTFIX || IS_RELEASE || IS_PULL_REQUEST || IS_MANUAL_BUILD || env.BRANCH_NAME == 'Beta' || env.BRANCH_NAME == 'develop')
CAN_COMPRESS_WEB = CAN_PUBLISH_BUILD_WEB
CAN_COMPRESS_WCF = CAN_BUILD_WCF && (IS_HOTFIX || IS_RELEASE)

// 107	Настройки. Доставка
CAN_DELIVERY_DESKTOP = CAN_COMPRESS_DESKTOP
CAN_DELIVERY_WEB = CAN_COMPRESS_WEB
CAN_DELIVERY_WCF = CAN_COMPRESS_WCF
WIN_DELIVERY_SHARED_FOLDER_NAME = "JenkinsWorkspace"
DESKTOP_VOD1_DELIVERY_PATH = "\\\\${NODE_VOD1}\\${WIN_DELIVERY_SHARED_FOLDER_NAME}\\${JOB_FOLDER_NAME}"
DESKTOP_VOD3_DELIVERY_PATH = "\\\\${NODE_VOD3}\\${WIN_DELIVERY_SHARED_FOLDER_NAME}\\${JOB_FOLDER_NAME}"
DESKTOP_VOD5_DELIVERY_PATH = "\\\\${NODE_VOD5}\\${WIN_DELIVERY_SHARED_FOLDER_NAME}\\${JOB_FOLDER_NAME}"
DESKTOP_VOD7_DELIVERY_PATH = "\\\\${NODE_VOD7}\\${WIN_DELIVERY_SHARED_FOLDER_NAME}\\${JOB_FOLDER_NAME}"
WEB_DELIVERY_PATH = "\\\\${NODE_VOD6}\\${WIN_DELIVERY_SHARED_FOLDER_NAME}\\${JOB_FOLDER_NAME}"

// 108	Настройки. Развертывание
DEPLOY_PATH = "F:/WORK/_BUILDS"
CAN_DEPLOY_DESKTOP = CAN_DELIVERY_DESKTOP && (env.BRANCH_NAME == 'Beta' || IS_PULL_REQUEST || IS_MANUAL_BUILD || env.BRANCH_NAME == 'develop')
CAN_DEPLOY_WEB = false
CAN_DEPLOY_WCF = false

// 109	Настройки. Публикация	
CAN_PUBLISH_DESKTOP = CAN_DELIVERY_DESKTOP && (IS_HOTFIX || IS_RELEASE)
CAN_PUBLISH_WEB = CAN_DELIVERY_WEB
CAN_PUBLISH_WCF = CAN_DELIVERY_WCF
//Release потому что правила именования фиксов/релизов Release_MMDD_HHMM
NEW_DESKTOP_HOTFIX_FOLDER_NAME_PREFIX = "Release"
NEW_WEB_HOTFIX_FOLDER_NAME = "Hotfix"
NEW_WCF_HOTFIX_FOLDER_NAME = NEW_WEB_HOTFIX_FOLDER_NAME
NEW_RELEASE_FOLDER_NAME = "NewRelease"
DESKTOP_HOTFIX_PUBLISH_PATH = DESKTOP_WORK_PATH
DESKTOP_NEW_RELEASE_PUBLISH_PATH = "${DESKTOP_WATER_DELIVERY_PATH}/${NEW_RELEASE_FOLDER_NAME}"
WEB_PUBLISH_PATH = "E:/CD"
WCF_PUBLISH_PATH = "/opt/jenkins/builds"


//-----------------------------------------------------------------------

// 200	Этапы

// 201	Этапы. Подготовка репозитория
stage('Checkout'){
	parallel (
		"Win" : {
			node(NODE_WIN_BUILD){
				PrepareSources()
			}
		},
		"Linux" : {
			node(NODE_LINUX_BUILD){
				PrepareSources()
			}
		}
	)
}

// 202	Этапы. Восстановление пакетов
stage('Restore'){
	parallel (
		"Win" : {
			node(NODE_WIN_BUILD){
				bat "\"${WIN_BUILD_TOOL}\" Vodovoz/Source/Vodovoz.sln /t:Restore /p:Configuration=DebugWin /p:Platform=x86 /maxcpucount:2"
			}
		},
		"Linux" : {
			node(NODE_LINUX_BUILD){
				sh 'nuget restore Vodovoz/Source/Vodovoz.sln'
				sh 'nuget restore Vodovoz/Source/Libraries/External/QSProjects/QSProjectsLib.sln'
				sh 'nuget restore Vodovoz/Source/Libraries/External/My-FyiReporting/MajorsilenceReporting-Linux-GtkViewer.sln'
			}
		}
	)
}

// 203	Этапы. Сборка
stage('Build'){
	parallel (
		"Win" : {
			node(NODE_WIN_BUILD){
				stage('Build Desktop'){
					if(CAN_BUILD_DESKTOP)
					{
						Build("WinDesktop")
					}
					else
					{
						echo "Build Desktop not needed"
					}
				}
				stage('Build WEB'){
					if(CAN_PUBLISH_BUILD_WEB)
					{
						PublishBuild("${APP_PATH}/Backend/WebAPI/DriverAPI/DriverAPI.csproj")
						PublishBuild("${APP_PATH}/Backend/WebAPI/FastPaymentsAPI/FastPaymentsAPI.csproj")
						PublishBuild("${APP_PATH}/Frontend/PayPageAPI/PayPageAPI.csproj")
						PublishBuild("${APP_PATH}/Backend/WebAPI/Email/MailjetEventsDistributorAPI/MailjetEventsDistributorAPI.csproj")
						PublishBuild("${APP_PATH}/Frontend/UnsubscribePage/UnsubscribePage.csproj")
						PublishBuild("${APP_PATH}/Backend/WebAPI/DeliveryRulesService/DeliveryRulesService.csproj")
						PublishBuild("${APP_PATH}/Backend/WebAPI/RoboatsService/RoboatsService.csproj")
						PublishBuild("${APP_PATH}/Backend/WebAPI/TrueMarkAPI/TrueMarkAPI.csproj")
						PublishBuild("${APP_PATH}/Backend/WebAPI/TaxcomEdoApi/TaxcomEdoApi.csproj")
						PublishBuild("${APP_PATH}/Backend/WebAPI/CashReceiptApi/CashReceiptApi.csproj")
						PublishBuild("${APP_PATH}/Backend/WebAPI/CustomerAppsApi/CustomerAppsApi.csproj")
						PublishBuild("${APP_PATH}/Backend/Workers/IIS/CashReceiptPrepareWorker/CashReceiptPrepareWorker.csproj")
						PublishBuild("${APP_PATH}/Backend/Workers/IIS/CashReceiptSendWorker/CashReceiptSendWorker.csproj")
						PublishBuild("${APP_PATH}/Backend/Workers/IIS/TrueMarkCodePoolCheckWorker/TrueMarkCodePoolCheckWorker.csproj")
					}
					else if(CAN_BUILD_WEB)
					{
						//Сборка для проверки что нет ошибок, собранные проекты выкладывать не нужно
						Build("Web")
					}
					else
					{
						echo "Build Web not needed"
					}
				}
			}
		},
		"Linux" : {
			node(NODE_LINUX_BUILD){
				stage('Build WCF'){
					if(CAN_BUILD_WCF)
					{
						Build("WCF")
					}
					else
					{
						echo "Build WCF not needed"
					}
				}
			}
		}
	)
}


// 204	Этапы. Запаковка
stage('Compress'){
	parallel(
		"Desktop" : { CompressDesktopArtifact() },

		"DriverAPI" : { CompressWebArtifact("Backend/WebAPI/DriverAPI") },
		"FastPaymentsAPI" : { CompressWebArtifact("Backend/WebAPI/FastPaymentsAPI") },
		"PayPageAPI" : { CompressWebArtifact("Frontend/PayPageAPI") },
		"MailjetEventsDistributorAPI" : { CompressWebArtifact("Backend/WebAPI/Email/MailjetEventsDistributorAPI") },
		"UnsubscribePage" : { CompressWebArtifact("Frontend/UnsubscribePage") },
		"DeliveryRulesService" : { CompressWebArtifact("Backend/WebAPI/DeliveryRulesService") },
		"RoboatsService" : { CompressWebArtifact("Backend/WebAPI/RoboatsService") },
		"TrueMarkAPI" : { CompressWebArtifact("Backend/WebAPI/TrueMarkAPI") },
		"TaxcomEdoApi" : { CompressWebArtifact("Backend/WebAPI/TaxcomEdoApi") },
		"CashReceiptApi" : { CompressWebArtifact("Backend/WebAPI/CashReceiptApi") },
		"CustomerAppsApi" : { CompressWebArtifact("Backend/WebAPI/CustomerAppsApi") },
		"CashReceiptPrepareWorker" : { CompressWebArtifact("Backend/Workers/IIS/CashReceiptPrepareWorker") },
		"CashReceiptSendWorker" : { CompressWebArtifact("Backend/Workers/IIS/CashReceiptSendWorker") },
		"TrueMarkCodePoolCheckWorker" : { CompressWebArtifact("Backend/Workers/IIS/TrueMarkCodePoolCheckWorker") },

		"VodovozSmsInformerService" : { CompressWcfArtifact("Backend/Workers/Mono/VodovozSmsInformerService") },
		"VodovozSmsPaymentService" : { CompressWcfArtifact("Backend/WCF/VodovozSmsPaymentService") },
	)
}

// 205	Этапы. Доставка
stage('Delivery'){
	parallel(
		"Desktop ${NODE_VOD1}" : { DeliveryDesktopArtifact(DESKTOP_VOD1_DELIVERY_PATH) },
		"Desktop ${NODE_VOD3}" : { DeliveryDesktopArtifact(DESKTOP_VOD3_DELIVERY_PATH) },
		"Desktop ${NODE_VOD5}" : { DeliveryDesktopArtifact(DESKTOP_VOD5_DELIVERY_PATH) },
		"Desktop ${NODE_VOD7}" : { DeliveryDesktopArtifact(DESKTOP_VOD7_DELIVERY_PATH) },

		"DriverAPI" : { DeliveryWebArtifact("DriverAPI") },
		"FastPaymentsAPI" : { DeliveryWebArtifact("FastPaymentsAPI") },
		"PayPageAPI" : { DeliveryWebArtifact("PayPageAPI") },
		"MailjetEventsDistributorAPI" : { DeliveryWebArtifact("MailjetEventsDistributorAPI") },
		"UnsubscribePage" : { DeliveryWebArtifact("UnsubscribePage") },
		"DeliveryRulesService" : { DeliveryWebArtifact("DeliveryRulesService") },
		"RoboatsService" : { DeliveryWebArtifact("RoboatsService") },
		"TrueMarkAPI" : { DeliveryWebArtifact("TrueMarkAPI") },
		"TaxcomEdoApi" : { DeliveryWebArtifact("TaxcomEdoApi") },
		"CashReceiptApi" : { DeliveryWebArtifact("CashReceiptApi") },
		"CustomerAppsApi" : { DeliveryWebArtifact("CustomerAppsApi") },
		"CashReceiptPrepareWorker" : { DeliveryWebArtifact("CashReceiptPrepareWorker") },
		"CashReceiptSendWorker" : { DeliveryWebArtifact("CashReceiptSendWorker") },
		"TrueMarkCodePoolCheckWorker" : { DeliveryWebArtifact("TrueMarkCodePoolCheckWorker") },

		"SmsInformerService" : { DeliveryWcfArtifact("SmsInformerService") },
		"SmsPaymentService" : { DeliveryWcfArtifact("SmsPaymentService") },
	)
}

// 206	Этапы. Развертывание
stage('Deploy'){
	DeployDesktop()
}

// 207	Этапы. Публикация
stage('Publish'){
	parallel(
		"Desktop ${NODE_VOD1}" : { PublishDesktop(NODE_VOD1) },
		"Desktop ${NODE_VOD3}" : { PublishDesktop(NODE_VOD3) },
		"Desktop ${NODE_VOD5}" : { PublishDesktop(NODE_VOD5) },
		"Desktop ${NODE_VOD7}" : { PublishDesktop(NODE_VOD7) },

		"DriverAPI" : { PublishWeb("DriverAPI") },
		"FastPaymentsAPI" : { PublishWeb("FastPaymentsAPI") },
		"PayPageAPI" : { PublishWeb("PayPageAPI") },
		"MailjetEventsDistributorAPI" : { PublishWeb("MailjetEventsDistributorAPI") },
		"UnsubscribePage" : { PublishWeb("UnsubscribePage") },
		"DeliveryRulesService" : { PublishWeb("DeliveryRulesService") },
		"RoboatsService" : { PublishWeb("RoboatsService") },
		"TrueMarkAPI" : { PublishWeb("TrueMarkAPI") },
		"TaxcomEdoApi" : { PublishWeb("TaxcomEdoApi") },
		"CashReceiptApi" : { PublishWeb("CashReceiptApi") },
		"CustomerAppsApi" : { PublishWeb("CustomerAppsApi") },
		"CashReceiptPrepareWorker" : { PublishWeb("CashReceiptPrepareWorker") },
		"CashReceiptSendWorker" : { PublishWeb("CashReceiptSendWorker") },
		"TrueMarkCodePoolCheckWorker" : { PublishWeb("TrueMarkCodePoolCheckWorker") },

		"SmsInformerService" : { PublishWCF("SmsInformerService") },
		"SmsPaymentService" : { PublishWCF("SmsPaymentService") },
	)
}

//-----------------------------------------------------------------------

// 300	Фукнции

// 301	Фукнции. Подготовка репозитория

def PrepareSources() {
	def REFERENCE_ABSOLUTE_PATH = "${JENKINS_HOME_NODE}/workspace/Vodovoz_Vodovoz_master"

	checkout changelog: false, poll: false, scm:([
		$class: 'GitSCM',
		branches: scm.branches,
		extensions: scm.extensions
		+ [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'Vodovoz']]
		+ [[$class: 'CloneOption', reference: "${REFERENCE_ABSOLUTE_PATH}/Vodovoz"]]
		+ [[$class: 'SubmoduleOption', disableSubmodules: false, recursiveSubmodules: true, parentCredentials: true]],
		userRemoteConfigs: scm.userRemoteConfigs
	])
}

// 302	Фукнции. Восстановление пакетов



// 303	Фукнции. Сборка

def PublishBuild(projectPath){
	bat "\"${WIN_BUILD_TOOL}\" ${projectPath} /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=FolderProfile /maxcpucount:2"
}

def Build(config){
	if (isUnix()) {
		sh "${LINUX_BUILD_TOOL} Vodovoz/Source/Vodovoz.sln /t:Build /p:Configuration=${config} /p:Platform=x86 /maxcpucount:2"
	}
	else {
		bat "\"${WIN_BUILD_TOOL}\" Vodovoz/Source/Vodovoz.sln /t:Build /p:Configuration=${config} /p:Platform=x86 /maxcpucount:2"
	}
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
			def webProjectName = GetFolderName(relativeProjectPath)
			CompressArtifact("${APP_PATH}/${webProjectName}/${WEB_BUILD_OUTPUT_CATALOG}", webProjectName)
		}
	} 
	else
	{
		echo "Compress Web artifacts not needed"
	}
}

def CompressWcfArtifact(relativeProjectPath){
	if(CAN_COMPRESS_WCF)
	{
		node(NODE_LINUX_BUILD){
			def wcfProjectName = GetFolderName(relativeProjectPath)
			CompressArtifact("${APP_PATH}/${wcfProjectName}/${WCF_BUILD_OUTPUT_CATALOG}", wcfProjectName)
		}
	} 
	else
	{
		echo "Compress WCF artifacts not needed"
	}
}

def CompressArtifact(sourcePath, artifactName) {
	def archive_file = "${artifactName}${ARCHIVE_EXTENTION}"

	if (fileExists(archive_file)) {
		echo "Delete exiting artifact ${archive_file} from ${sourcePath}/*"
		fileOperations([fileDeleteOperation(excludes: '', includes: "${archive_file}")])
	}

	echo "Compressing artifact ${archive_file} from ./${sourcePath}/*"
	ZipFiles(sourcePath, archive_file)
}

def DecompressArtifact(destPath, artifactName) {
	def archive_file = "${artifactName}${ARCHIVE_EXTENTION}"

	echo "Decompressing artifact ${archive_file} to ${destPath}"
	UnzipFiles(archive_file, destPath)
}

// 305	Фукнции. Доставка

def DeliveryDesktopArtifact(deliveryPath){
	if(CAN_DELIVERY_DESKTOP)
	{
		DeliveryWinArtifact("VodovozDesktop${ARCHIVE_EXTENTION}", deliveryPath)
	}
	else
	{
		echo "Delivery Desktop artifact to ${deliveryPath} not needed"
	}
}

def DeliveryWebArtifact(projectName){
	if(CAN_DELIVERY_WEB)
	{
		DeliveryWinArtifact("${projectName}${ARCHIVE_EXTENTION}", WEB_DELIVERY_PATH)
	}
	else
	{
		echo "Delivery ${projectName} artifact  not needed"
	}
}

def DeliveryWcfArtifact(projectName){
	if(CAN_DELIVERY_WCF)
	{
		DeliveryLinuxArtifact("${projectName}${ARCHIVE_EXTENTION}")
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

def DeliveryLinuxArtifact(artifactName){
	node(NODE_LINUX_BUILD){
		def workspacePath = GetWorkspacePath()
		def copyingItem = "${workspacePath}/${artifactName}"
		echo "Copy ${copyingItem} to ${workspacePath}"
		withCredentials([sshUserPrivateKey(credentialsId: "linux_vadim_jenkins_key", keyFileVariable: 'keyfile', usernameVariable: 'userName')]) {
			sh 'rsync -v -rz -e "ssh -o StrictHostKeyChecking=no -i $keyfile -p 2213 -v" ' + copyingItem + ' $userName@srv2.vod.qsolution.ru:'+ workspacePath + '/ --delete-before'
		}
	}
}

// 306	Фукнции. Развертывание

def DeployDesktop(){
	node(NODE_VOD3){
		if(CAN_DEPLOY_DESKTOP)
		{
			def OUTPUT_PATH = ""

			if(IS_PULL_REQUEST)
			{
				OUTPUT_PATH = "${DEPLOY_PATH}/pull_requests/${env.CHANGE_ID}"
			}
			else
			{
				OUTPUT_PATH = "${DEPLOY_PATH}/${env.BRANCH_NAME}"
			}

			DecompressArtifact(OUTPUT_PATH, 'VodovozDesktop')
		}
		else
		{
			echo "Deploy desktop builds not needed"
		}
	}
}

// 307	Фукнции. Публикация

def PublishDesktop(nodeName){
	node(nodeName){
		if(CAN_PUBLISH_DESKTOP)
		{
			if(IS_HOTFIX)
			{
				def now = new Date()
        		def hofix_suffix = now.format("MMdd_HHmm")
				def hotfixName = "${NEW_DESKTOP_HOTFIX_FOLDER_NAME_PREFIX}_${hofix_suffix}"
				def newHotfixPath = "${DESKTOP_HOTFIX_PUBLISH_PATH}/${hotfixName}"
				DecompressArtifact(newHotfixPath, 'VodovozDesktop')
				LockHotfix(hotfixName)
				return
			}

			if(IS_RELEASE)
			{
				DecompressArtifact(DESKTOP_NEW_RELEASE_PUBLISH_PATH, 'VodovozDesktop')
				return
			}
		}

		echo "Publish not needed"
	}
}

def PublishWeb(projectName){
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

def PublishWCF(projectName){
	node(NODE_LINUX_RUNTIME){
		if(CAN_PUBLISH_WCF)
		{
			if(IS_HOTFIX)
			{
				def newHotfixPath = "${WCF_PUBLISH_PATH}/${projectName}/${NEW_WCF_HOTFIX_FOLDER_NAME}"
				DecompressArtifact(newHotfixPath, projectName)
				return
			}

			if(IS_RELEASE)
			{
				def newReleasePath = "${WCF_PUBLISH_PATH}/${projectName}/${NEW_RELEASE_FOLDER_NAME}"
				DecompressArtifact(newReleasePath, projectName)
				return
			}

		}

		echo "Publish not needed"
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
		return GetFolderName(env.WORKSPACE)
	}
}

def GetFolderName(folderPath){
	splitted = folderPath.split("\\\\")
	folderName = splitted[splitted.length-1]
	return folderName
}

def WinRemoveDirectory(destPath){
	RunPowerShell("""
		Remove-Item -LiteralPath "${destPath}" -Force -Recurse
	""")
}

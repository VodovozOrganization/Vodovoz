//Copy artifacts - копирование архивированных сборок на ноды
//Deploy - разархивация сборок на ноде в каталог для соответствующей ветки
//Publish - разархивация сборок на ноде в каталог для нового релиза

//0. Настройки
//Nodes
NODE_VOD1 = "Vod1"
NODE_VOD3 = "Vod3"
NODE_VOD5 = "Vod5"
NODE_VOD6 = "Vod6"
NODE_VOD7 = "Vod7"
NODE_WIN_BUILD = "WIN_BUILD"
NODE_LINUX_BUILD = "LINUX_BUILD"
NODE_LINUX_RUNTIME = "LINUX_RUNTIME"

//Global
ARCHIVE_EXTENTION = '.7z'
APP_PATH = "Vodovoz/Source/Applications"
WCF_BUILD_OUTPUT_CATALOG = "bin/Debug"
WEB_BUILD_OUTPUT_CATALOG = "bin/Release/net5.0_publish"
WIN_BUILD_TOOL = "C:/Program Files (x86)/Microsoft Visual Studio/2019/Community/MSBuild/Current/Bin/MSBuild.exe"
DESKTOP_WATER_DELIVERY_PATH = "C:/Program Files (x86)/Vodovoz/WaterDelivery"
DESKTOP_WORK_PATH = "${DESKTOP_WATER_DELIVERY_PATH}/WorkTest"
RELEASE_LOCKER_PATH = "C:/Program Files (x86)/Vodovoz/VodovozLauncher/ReleaseLocker.exe"
UPDATE_LOCK_FILE = "${DESKTOP_WORK_PATH}/update.lock"
LINUX_BUILD_TOOL = "msbuild"
JOB_FOLDER_NAME = GetJobFolderName()
//WIN_WORKSPACE_PATH = GetWorkspacePathForNode(NODE_WIN_BUILD)
//LINUX_WORKSPACE_PATH = GetWorkspacePathForNode(NODE_LINUX_RUNTIME)
IS_PULL_REQUEST = env.CHANGE_ID != null
//IS_HOTFIX = env.BRANCH_NAME == 'master'
IS_HOTFIX = false
IS_RELEASE = true
IS_MANUAL_BUILD = env.BRANCH_NAME ==~ /^manual-build(.*?)/

//Build
CAN_BUILD_DESKTOP = true
CAN_BUILD_WEB = true
CAN_PUBLISH_BUILD_WEB = IS_HOTFIX || IS_RELEASE
CAN_BUILD_WCF = true

//Compress
/*CAN_COMPRESS_DESKTOP = CAN_BUILD_DESKTOP && (IS_HOTFIX || IS_RELEASE || IS_PULL_REQUEST || IS_MANUAL_BUILD || env.BRANCH_NAME == 'Beta')
CAN_COMPRESS_WEB = CAN_PUBLISH_BUILD_WEB
CAN_COMPRESS_WCF = CAN_BUILD_WCF && (IS_HOTFIX || IS_RELEASE)*/
//Test compress config
CAN_COMPRESS_DESKTOP = true
CAN_COMPRESS_WEB = true
CAN_COMPRESS_WCF = true

//Delivery
CAN_DELIVERY_DESKTOP = CAN_COMPRESS_DESKTOP
CAN_DELIVERY_WEB = CAN_COMPRESS_WEB
CAN_DELIVERY_WCF = CAN_COMPRESS_WCF
WIN_DELIVERY_SHARED_FOLDER_NAME = "JenkinsWorkspace"
DESKTOP_VOD1_DELIVERY_PATH = "\\\\${NODE_VOD1}\\${WIN_DELIVERY_SHARED_FOLDER_NAME}\\${JOB_FOLDER_NAME}"
DESKTOP_VOD3_DELIVERY_PATH = "\\\\${NODE_VOD3}\\${WIN_DELIVERY_SHARED_FOLDER_NAME}\\${JOB_FOLDER_NAME}"
DESKTOP_VOD5_DELIVERY_PATH = "\\\\${NODE_VOD5}\\${WIN_DELIVERY_SHARED_FOLDER_NAME}\\${JOB_FOLDER_NAME}"
DESKTOP_VOD7_DELIVERY_PATH = "\\\\${NODE_VOD7}\\${WIN_DELIVERY_SHARED_FOLDER_NAME}\\${JOB_FOLDER_NAME}"
WEB_DELIVERY_PATH = "\\\\${NODE_VOD6}\\${WIN_DELIVERY_SHARED_FOLDER_NAME}\\${JOB_FOLDER_NAME}"

//Deploy
DEPLOY_PATH = "F:/WORK/_BUILDS"
CAN_DEPLOY_DESKTOP = CAN_DELIVERY_DESKTOP && (env.BRANCH_NAME == 'Beta' || IS_PULL_REQUEST || IS_MANUAL_BUILD)
CAN_DEPLOY_WEB = false
CAN_DEPLOY_WCF = false

//Publish
CAN_PUBLISH_DESKTOP = CAN_DELIVERY_DESKTOP && (IS_HOTFIX || IS_RELEASE)
CAN_PUBLISH_WEB = CAN_DELIVERY_WEB
CAN_PUBLISH_WCF = CAN_DELIVERY_WCF
//Release потому что правила именования фиксов/релизов Release_MMDD_HHMM
NEW_DESKTOP_HOTFIX_FOLDER_NAME_PREFIX = "Release"
NEW_WEB_HOTFIX_FOLDER_NAME = "Hotfix"
NEW_WCF_HOTFIX_FOLDER_NAME = NEW_WEB_HOTFIX_FOLDER_NAME
DESKTOP_HOTFIX_PUBLISH_PATH = DESKTOP_WORK_PATH
DESKTOP_NEW_RELEASE_PUBLISH_PATH = "${DESKTOP_WATER_DELIVERY_PATH}/NewReleaseTest"
WEB_PUBLISH_PATH = "E:/CD"
WCF_PUBLISH_PATH = "/opt/jenkins/builds"

echo "JOB_FOLDER_NAME: ${JOB_FOLDER_NAME}"

//Разархивируем 
//release/ в NewRelease
//master в NewHotfix
//Beta в F:\\WORK\\_BUILDS\\Beta
//PR в F:\\WORK\\_BUILDS\\pull_requests\ID
/*
//Desktop
CAN_DEPLOY_DESKTOP_BRANCH = env.BRANCH_NAME == 'master' || env.BRANCH_NAME == 'develop' || env.BRANCH_NAME == 'Beta' || env.BRANCH_NAME ==~ /^[Rr]elease(.*?)/
CAN_DEPLOY_DESKTOP_PR = env.CHANGE_ID != null
CAN_COPY_DESKTOP_ARTIFACTS = CAN_DEPLOY_DESKTOP_BRANCH || CAN_DEPLOY_DESKTOP_PR 
CAN_PUBLISH_DESKTOP = env.BRANCH_NAME == 'master'

//Web
CAN_PUBLISH_WEB_ARTIFACTS = env.BRANCH_NAME ==~ /(develop|master)/ || env.BRANCH_NAME ==~ /^[Rr]elease(.*?)/
CAN_COPY_WEB_ARTIFACTS = CAN_PUBLISH_WEB_ARTIFACTS

//WCF
CAN_PUBLISH_WCF_ARTIFACTS = env.BRANCH_NAME == 'master'
CAN_COPY_WCF_ARTIFACTS = CAN_PUBLISH_WCF_ARTIFACTS

//ARCHIVATION

echo "CAN_DEPLOY_DESKTOP_BRANCH: ${CAN_DEPLOY_DESKTOP_BRANCH}"
echo "CAN_DEPLOY_DESKTOP_PR: ${CAN_DEPLOY_DESKTOP_PR}"
echo "CAN_COPY_DESKTOP_ARTIFACTS: ${CAN_COPY_DESKTOP_ARTIFACTS}"
echo "CAN_PUBLISH_DESKTOP: ${CAN_PUBLISH_DESKTOP}"
echo "CAN_PUBLISH_WEB_ARTIFACTS: ${CAN_PUBLISH_WEB_ARTIFACTS}"
echo "CAN_COPY_WEB_ARTIFACTS: ${CAN_COPY_WEB_ARTIFACTS}"
echo "CAN_PUBLISH_WCF_ARTIFACTS: ${CAN_PUBLISH_WCF_ARTIFACTS}"
echo "CAN_COPY_WCF_ARTIFACTS: ${CAN_COPY_WCF_ARTIFACTS}"*/

//1. Подготовка репозиториев
/*
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

//2. Восстановление пакетов для проектов
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

//3. Сборка проектов
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
*/

//4. Архивация
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

//5. Доставка сборок на ноды
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

//6. Развертывание
stage('Deploy'){
	DeployDesktop()
}

//7.Публикация
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

def PrepareSources() {
	def REFERENCE_ABSOLUTE_PATH = "${JENKINS_HOME_NODE}/workspace/Vodovoz_Vodovoz_master"

	echo "checkout Vodovoz"	
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

/*
def PublishBuildWebServiceToFolder(serviceName, projectPath) {
	def branch_name = env.BRANCH_NAME.replaceAll('/','') + '\\'
	def csproj_path = "${projectPath}\\${serviceName}.csproj"
	echo "Publish ${serviceName} to folder (${env.BRANCH_NAME})"
	PublishBuild(csproj_path)

	//CompressArtifact(outputPath, serviceName)
}
*/

//Compress functions

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
			CompressArtifact("${APP_PATH}/${relativeProjectPath}/${WCF_BUILD_OUTPUT_CATALOG}", wcfProjectName)
		}
	} 
	else
	{
		echo "Compress WCF artifacts not needed"
	}
}

//Delivery functions

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


//Deploy functions

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


//Publish functions

def PublishDesktop(nodeName){
	node(nodeName){
		if(CAN_PUBLISH_DESKTOP)
		{
			if(IS_HOTFIX)
			{
				def now = new Date()
        		def hofix_suffix = now.format("MMdd_HHmm")
				def newHotfixPath = "${DESKTOP_HOTFIX_PUBLISH_PATH}/${NEW_DESKTOP_HOTFIX_FOLDER_NAME_PREFIX}_${hofix_suffix}"
				LockHotfix(newHotfixPath)
				DecompressArtifact(newHotfixPath, 'VodovozDesktop')
				UnlockHotfix(newHotfixPath)
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

/*
def PublishMasterDesktop() {
	script{
		if(CAN_PUBLISH_DESKTOP)
		{
			def PRERELEASE_PATH = "${MasterRuntimePath}\\prerelease"

			echo "Publish master to prerelease folder ${PRERELEASE_PATH}"
			DecompressArtifact(PRERELEASE_PATH, 'Vodovoz')
		}
		else
		{
			echo "Branch is not master, nothing to publish to prerelease folder"
		}
	}
}

def PublishWebServices(){
	script{
		if(CAN_PUBLISH_BUILD_WEB)
		{
			parallel (
				"Publish DriversAPI" : {
					PublishWebService('DriversAPI')
				},
				"Publish FastPaymentsAPI" : {
					PublishWebService('FastPaymentsAPI')
				},
				"Publish MailjetEventsDistributorAPI" : {
					PublishWebService('MailjetEventsDistributorAPI')
				},
				"Publish UnsubscribePage" : {
					PublishWebService('UnsubscribePage')
				},
				"Publish DeliveryRulesService" : {
					PublishWebService('DeliveryRulesService')
				},
				"Publish RoboatsService" : {
					PublishWebService('RoboatsService')
				},
				"Publish TaxcomEdoApi" : {
					PublishWebService('TaxcomEdoApi')
				},
				"Publish TrueMarkAPI" : {
					PublishWebService('TrueMarkAPI')
				},
				"Publish PayPageAPI" : {
					PublishWebService('PayPageAPI')
				},
				"Publish CashReceiptApi" : {
					PublishWebService('CashReceiptApi')
				},
				"Publish CustomerAppsApi" : {
					PublishWebService('CustomerAppsApi')
				},
				"Publish CashReceiptPrepareWorker" : {
					PublishWebService('CashReceiptPrepareWorker')
				},
				"Publish CashReceiptSendWorker" : {
					PublishWebService('CashReceiptSendWorker')
				},
				"Publish TrueMarkCodePoolCheckWorker" : {
					PublishWebService('TrueMarkCodePoolCheckWorker')
				},
			)
		}
		else
		{
			echo 'Skipped, branch (' + env.BRANCH_NAME + ')'
		}
	}
}

def PublishWebService(serviceName) {
	def BRANCH_NAME = env.BRANCH_NAME.replaceAll('/','') + '\\'
	def SERVICE_PATH = "E:\\CD\\${serviceName}\\${BRANCH_NAME}"
	
	echo "Deploy ${serviceName} to CD folder"
	DecompressArtifact(SERVICE_PATH, serviceName)
}

def PublishWCFServices(){
	script{
		if(CAN_PUBLISH_WCF_ARTIFACTS)
		{
			parallel (
				"Publish SmsInformerService" : {
					PublishWCFService('SmsInformerService')
				},
				"Publish SmsPaymentService" : {
					PublishWCFService('SmsPaymentService')
				},
			)
		}
		else
		{
			echo 'Skipped, branch (' + env.BRANCH_NAME + ')'
		}
	}
}

def PublishWCFService(serviceName) {
	def SERVICE_PATH = "/opt/jenkins/builds/${serviceName}"
	DecompressArtifact(SERVICE_PATH, serviceName)
}
*/


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

//Utility functions

def LockHotfix(hotfixPath){
	RunPowerShell("""
		Start-Process -FilePath "${RELEASE_LOCKER_PATH}" -ArgumentList '"${UPDATE_LOCK_FILE}" "lock" "${hotfixPath}/Vodovoz.exe"'
	""")
}

def UnlockHotfix(hotfixPath){
	RunPowerShell("""
		Start-Process -FilePath "${RELEASE_LOCKER_PATH}" -ArgumentList '"${UPDATE_LOCK_FILE}" "unlock" "${hotfixPath}/Vodovoz.exe"'
	""")
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

/*def GetWorkspacePathForNode (nodeName)  {  
	node(nodeName){
		return GetWorkspacePath()
	}  
}*/

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


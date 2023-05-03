//Copy artifacts - копирование архивированных сборок на ноды
//Deploy - разархивация сборок на ноде в каталог для соответствующей ветки
//Publish - разархивация сборок на ноде в каталог для нового релиза

//0. Настройки
//Global
APP_PATH = "Vodovoz\\Source\\Applications"
WEB_BUILD_OUTPUT_CATALOG = "bin\\Release\\net5.0_publish"

//Build
WIN_BUILD_TOOL = "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe"
LINUX_BUILD_TOOL = "msbuild"
CAN_PUBLISH_BUILD_WEB = env.BRANCH_NAME == 'master' || env.BRANCH_NAME ==~ /^[Rr]elease(.*?)/

//Compress


//Copy

//Deploy

//Publish


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
ARCHIVE_EXTENTION = '.7z'

echo "CAN_DEPLOY_DESKTOP_BRANCH: ${CAN_DEPLOY_DESKTOP_BRANCH}"
echo "CAN_DEPLOY_DESKTOP_PR: ${CAN_DEPLOY_DESKTOP_PR}"
echo "CAN_COPY_DESKTOP_ARTIFACTS: ${CAN_COPY_DESKTOP_ARTIFACTS}"
echo "CAN_PUBLISH_DESKTOP: ${CAN_PUBLISH_DESKTOP}"
echo "CAN_PUBLISH_WEB_ARTIFACTS: ${CAN_PUBLISH_WEB_ARTIFACTS}"
echo "CAN_COPY_WEB_ARTIFACTS: ${CAN_COPY_WEB_ARTIFACTS}"
echo "CAN_PUBLISH_WCF_ARTIFACTS: ${CAN_PUBLISH_WCF_ARTIFACTS}"
echo "CAN_COPY_WCF_ARTIFACTS: ${CAN_COPY_WCF_ARTIFACTS}"

//1. Подготовка репозиториев
stage('Checkout'){
	parallel (
		"Win" : {
			node('WIN_BUILD'){
				PrepareSources("${JENKINS_HOME_WIN}")
			}
		},
		"Linux" : {
			node('LINUX_BUILD'){
				PrepareSources("${JENKINS_HOME}")
			}
		}
	)
}

//2. Восстановление пакетов для проектов
stage('Restore'){
	parallel (
		"Win" : {
			node('WIN_BUILD'){
				bat '"$WIN_BUILD_TOOL" Vodovoz\\Source\\Vodovoz.sln /t:Restore /p:Configuration=DebugWin /p:Platform=x86 /maxcpucount:2'
			}
		},
		"Linux" : {
			node('LINUX_BUILD'){
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
			node('WIN_BUILD'){
				stage('Build Desktop'){
					Build("WinDesktop")
					
					//CompressArtifact('Vodovoz/Source/Applications/Desktop/Vodovoz/bin/DebugWin', 'Vodovoz')
					//archiveArtifacts artifacts: "Vodovoz${ARCHIVE_EXTENTION}", onlyIfSuccessful: true			
				}
				stage('Build WEB'){
					script{
						if(CAN_PUBLISH_BUILD_WEB)
						{
							PublishBuild("${APP_PATH}\\Backend\\WebAPI\\DriverAPI\\DriverAPI.csproj")
							PublishBuild("${APP_PATH}\\Backend\\WebAPI\\FastPaymentsAPI\\FastPaymentsAPI.csproj")
							PublishBuild("${APP_PATH}\\Frontend\\PayPageAPI\\PayPageAPI.csproj")
							PublishBuild("${APP_PATH}\\Backend\\WebAPI\\Email\\MailjetEventsDistributorAPI\\MailjetEventsDistributorAPI.csproj")
							PublishBuild("${APP_PATH}\\Frontend\\UnsubscribePage\\UnsubscribePage.csproj")
							PublishBuild("${APP_PATH}\\Backend\\WebAPI\\DeliveryRulesService\\DeliveryRulesService.csproj")
							PublishBuild("${APP_PATH}\\Backend\\WebAPI\\RoboatsService\\RoboatsService.csproj")
							PublishBuild("${APP_PATH}\\Backend\\WebAPI\\TrueMarkAPI\\TrueMarkAPI.csproj")
							PublishBuild("${APP_PATH}\\Backend\\WebAPI\\TaxcomEdoApi\\TaxcomEdoApi.csproj")
							PublishBuild("${APP_PATH}\\Backend\\WebAPI\\CashReceiptApi\\CashReceiptApi.csproj")
							PublishBuild("${APP_PATH}\\Backend\\WebAPI\\CustomerAppsApi\\CustomerAppsApi.csproj")
							PublishBuild("${APP_PATH}\\Backend\\Workers\\IIS\\CashReceiptPrepareWorker\\CashReceiptPrepareWorker.csproj")
							PublishBuild("${APP_PATH}\\Backend\\Workers\\IIS\\CashReceiptSendWorker\\CashReceiptSendWorker.csproj")
							PublishBuild("${APP_PATH}\\Backend\\Workers\\IIS\\TrueMarkCodePoolCheckWorker\\TrueMarkCodePoolCheckWorker.csproj")
						}
						else
						{
							//Сборка для проверки что нет ошибок, собранные проекты выкладывать не нужно
							Build("Web")
						}
					}
				}
			}
		},
		"Linux" : {
			node('LINUX_BUILD'){
				stage('Build WCF'){
					Build("WCF")
					sh 'msbuild /p:Configuration=WCF /p:Platform=x86 Vodovoz/Source/Vodovoz.sln -maxcpucount:2'

					if (fileExists("Vodovoz/Source/Applications/Backend/Workers/Mono/VodovozSmsInformerService/bin/Debug/SmsPaymentService${ARCHIVE_EXTENTION}")) {
						fileOperations([fileDeleteOperation(excludes: '', includes: "Vodovoz/Source/Applications/Backend/Workers/Mono/VodovozSmsInformerService/bin/Debug/SmsPaymentService${ARCHIVE_EXTENTION}")])
					}

					if (fileExists("Vodovoz/Source/Applications/Backend/WCF/VodovozSmsPaymentService/bin/Debug/SmsInformerService${ARCHIVE_EXTENTION}")) {
						fileOperations([fileDeleteOperation(excludes: '', includes: "Vodovoz/Source/Applications/Backend/WCF/VodovozSmsPaymentService/bin/Debug/SmsInformerService${ARCHIVE_EXTENTION}")])
					}

					CompressArtifact('Vodovoz/Source/Applications/Backend/Workers/Mono/VodovozSmsInformerService/bin/Debug', 'SmsInformerService')
					CompressArtifact('Vodovoz/Source/Applications/Backend/WCF/VodovozSmsPaymentService/bin/Debug', 'SmsPaymentService')

					archiveArtifacts artifacts: "*Service${ARCHIVE_EXTENTION}", onlyIfSuccessful: true
				}
			}
		}
	)
}


//4. Архивация
stage('Compress'){
	parallel (

	)
}

//5. Копирование на ноды
stage('Copy'){
	node('Vod1'){
		CopyDesktopArtifacts("Vod1")
	}
	node('Vod3'){
		CopyDesktopArtifacts("Vod3")
	}
	node('Vod5'){
		CopyDesktopArtifacts("Vod5")
	}
	node('Vod7'){
		CopyDesktopArtifacts("Vod7")
	}
	node('WIN_WEB_RUNTIME'){
		script{
			echo "Can copy artifacts for web: ${CAN_COPY_WEB_ARTIFACTS}"
			if(CAN_COPY_WEB_ARTIFACTS)
			{
				echo "Copy web artifacts"
				copyArtifacts(projectName: '${JOB_NAME}', selector: specific( buildNumber: '${BUILD_NUMBER}'));
			}
			else
			{
				echo "Copy web artifacts not needed"
			}
		}
	}
	node('LINUX_RUNTIME'){
		script{
			echo "Can copy artifacts for WCF: ${CAN_COPY_WCF_ARTIFACTS}"
			if(CAN_COPY_WCF_ARTIFACTS)
			{
				echo "Copy WCF artifacts"
				copyArtifacts(projectName: '${JOB_NAME}', selector: specific( buildNumber: '${BUILD_NUMBER}'));
			}
			else
			{
				echo "Copy WCF artifacts not needed"
			}
		}
	}	
}

//6. Развертывание
stage('Deploy'){
	script{
		node('Vod3'){
			def BUILDS_PATH = "F:\\WORK\\_BUILDS\\"
			if(CAN_DEPLOY_DESKTOP_BRANCH)
			{
				echo "Deploy branches build to desktop vod3"
				def OUTPUT_PATH = BUILDS_PATH + env.BRANCH_NAME
				DecompressArtifact(OUTPUT_PATH, 'Vodovoz')
			}
			else if(CAN_DEPLOY_DESKTOP_PR)
			{
				echo "Deploy pull request build to desktop vod3"
				def OUTPUT_PATH = BUILDS_PATH + "pull_requests\\" + env.CHANGE_ID
				DecompressArtifact(OUTPUT_PATH, 'Vodovoz')
			}
			else
			{
				echo "Deploy desktop builds not needed"
			}
		}	
	}
}

//7.Публикация
stage('Publish'){
	node('Vod1'){
		PublishMasterDesktop()
	}
	node('Vod3'){
		PublishMasterDesktop()
	}
	node('Vod5'){
		PublishMasterDesktop()
	}
	node('Vod7'){
		PublishMasterDesktop()
	}
	node('WIN_WEB_RUNTIME'){
		PublishWebServices()
	}
	node('LINUX_RUNTIME'){
		PublishWCFServices()
	}
}

def PrepareSources(jenkinsHome) {
	def REFERENCE_ABSOLUTE_PATH = "$jenkinsHome/workspace/Vodovoz_Vodovoz_master"

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

def PublishBuildWebServiceToFolder(serviceName, projectPath) {
	def branch_name = env.BRANCH_NAME.replaceAll('/','') + '\\'
	def csproj_path = "${projectPath}\\${serviceName}.csproj"
	echo "Publish ${serviceName} to folder (${env.BRANCH_NAME})"
	PublishBuild(csproj_path)

	//CompressArtifact(outputPath, serviceName)
}


def CopyDesktopArtifacts(serverName){
	script{
		echo "Can copy artifacts for desktop ${serverName}: ${CAN_COPY_DESKTOP_ARTIFACTS}"
		if(CAN_COPY_DESKTOP_ARTIFACTS)
		{
			echo "Copy desktop ${serverName} artifacts"
			copyArtifacts(projectName: '${JOB_NAME}', selector: specific( buildNumber: '${BUILD_NUMBER}'));
		}
		else
		{
			echo "Copy desktop artifacts not needed"
		}
	}
}

def PublishMasterDesktop() {
	script{
		if(CAN_PUBLISH_DESKTOP)
		{
			def PRERELEASE_PATH = "${MasterRuntimePath}\\prerelease"

			echo "Publish master to prerelease folder ${PRERELEASE_PATH}"
			DecompressArtifact(PRERELEASE_PATH, 'Vodovoz')
		}else{
			echo "Branch is not master, nothing to publish to prerelease folder"
		}
	}
}

def PublishWebServices(){
	script{
		if(CAN_PUBLISH_WEB_ARTIFACTS)
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

def CompressArtifact(sourcePath, artifactName) {
	def archive_file = ${artifactName}${ARCHIVE_EXTENTION}

	if (fileExists("${archive_file}")) {
		echo "Delete exiting artifact ${archive_file} from ./${sourcePath}/*"
		fileOperations([fileDeleteOperation(excludes: '', includes: "${archive_file}")])
	}

	echo "Compressing artifact ${archive_file} from ./${sourcePath}/*"
	ZipFiles(sourcePath, archive_file)
}

def DecompressArtifact(destPath, artifactName) {
	def archive_file = ${artifactName}${ARCHIVE_EXTENTION}

	echo "Decompressing archive ${archive_file} to ${destPath}"
	UnzipFiles(archive_file, destPath)
}

def PublishBuild(projectPath){
	bat "\"${WIN_BUILD_TOOL}\" ${projectPath} /t:Rebuild /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=FolderProfile /maxcpucount:2"
}

def Build(config){
	if (isUnix()) {
		sh "${LINUX_BUILD_TOOL} Vodovoz/Source/Vodovoz.sln /t:Build /p:Configuration=${config} /p:Platform=x86 /maxcpucount:2"
	}
	else {
		bat "\"${WIN_BUILD_TOOL}\" Vodovoz\\Source\\Vodovoz.sln /t:Build /p:Configuration=${config} /p:Platform=x86 /maxcpucount:2"
	}
}

//Utility functions

def ZipFiles(sourcePath, archiveFile){
	if (isUnix()) {
		sh "7z a -stl ${archiveFile} ./${sourcePath}/*"
	}
	else {
		bat "7z a -stl ${archiveFile} ./${sourcePath}/*"
	}
}

def UnzipFiles(archiveFile, destPath){
	if (isUnix()) {
		sh "7z x -y -o\"${destPath}\" ${archiveFile}"
	}
	else {
		bat "7z x -y -o\"${destPath}\" ${archiveFile}"
	}
}

//Copy artifacts - копирование архивированных сборок на ноды
//Deploy - разархивация сборок на ноде в каталог для соответствующей ветки
//Publish - разархивация сборок на ноде в каталог для нового релиза

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

echo "CAN_DEPLOY_DESKTOP_BRANCH: ${CAN_DEPLOY_DESKTOP_BRANCH}"
echo "CAN_DEPLOY_DESKTOP_PR: ${CAN_DEPLOY_DESKTOP_PR}"
echo "CAN_COPY_DESKTOP_ARTIFACTS: ${CAN_COPY_DESKTOP_ARTIFACTS}"
echo "CAN_PUBLISH_DESKTOP: ${CAN_PUBLISH_DESKTOP}"
echo "CAN_PUBLISH_WEB_ARTIFACTS: ${CAN_PUBLISH_WEB_ARTIFACTS}"
echo "CAN_COPY_WEB_ARTIFACTS: ${CAN_COPY_WEB_ARTIFACTS}"
echo "CAN_PUBLISH_WCF_ARTIFACTS: ${CAN_PUBLISH_WCF_ARTIFACTS}"
echo "CAN_COPY_WCF_ARTIFACTS: ${CAN_COPY_WCF_ARTIFACTS}"

//Подготовка репозитория и проектов
stage('Prepare sources'){
	echo "Checkout"
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
	echo "Restore packages"
	parallel (
		"Win" : {
			node('WIN_BUILD'){
				bat '"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe" Vodovoz\\Source\\Vodovoz.sln -t:Restore -p:Configuration=DebugWin -p:Platform=x86 -maxcpucount:2'
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

//Сборка проектов
parallel (
	"Win" : {
		node('WIN_BUILD'){
			stage('Build Desktop'){
				bat '"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe" Vodovoz\\Source\\Vodovoz.sln -t:Build -p:Configuration=WinDesktop -p:Platform=x86 -maxcpucount:2'

				fileOperations([fileDeleteOperation(excludes: '', includes: 'Vodovoz.zip')])
				zip zipFile: 'Vodovoz.zip', archive: false, dir: 'Vodovoz/Source/Applications/Desktop/Vodovoz/bin/DebugWin'
				archiveArtifacts artifacts: 'Vodovoz.zip', onlyIfSuccessful: true			
			}
			stage('Build WEB'){
				script{
					if(env.BRANCH_NAME ==~ /(develop|master)/ || env.BRANCH_NAME ==~ /^[Rr]elease(.*?)/)
					{				
						PublishBuildWebServiceToFolder('DriversAPI', 'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\DriverAPI\\DriverAPI.csproj', 
							'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\DriverAPI\\bin\\Release\\net5.0_publish')

						PublishBuildWebServiceToFolder('FastPaymentsAPI', 'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\FastPaymentsAPI\\FastPaymentsAPI.csproj', 
							'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\FastPaymentsAPI\\bin\\Release\\net5.0_publish')

						PublishBuildWebServiceToFolder('PayPageAPI', 'Vodovoz\\Source\\Applications\\Frontend\\PayPageAPI\\PayPageAPI.csproj', 
							'Vodovoz\\Source\\Applications\\Frontend\\PayPageAPI\\bin\\Release\\net5.0_publish')

						PublishBuildWebServiceToFolder('MailjetEventsDistributorAPI', 'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\Email\\MailjetEventsDistributorAPI\\MailjetEventsDistributorAPI.csproj', 
							'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\Email\\MailjetEventsDistributorAPI\\bin\\Release\\net5.0_publish')
							
						PublishBuildWebServiceToFolder('UnsubscribePage', 'Vodovoz\\Source\\Applications\\Frontend\\UnsubscribePage\\UnsubscribePage.csproj', 
							'Vodovoz\\Source\\Applications\\Frontend\\UnsubscribePage\\bin\\Release\\net5.0_publish')

						PublishBuildWebServiceToFolder('DeliveryRulesService', 'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\DeliveryRulesService\\DeliveryRulesService.csproj', 
							'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\DeliveryRulesService\\bin\\Release\\net5.0_publish')

						PublishBuildWebServiceToFolder('RoboatsService', 'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\RoboatsService\\RoboatsService.csproj', 
							'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\RoboatsService\\bin\\Release\\net5.0_publish')

						PublishBuildWebServiceToFolder('TrueMarkAPI', 'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\TrueMarkAPI\\TrueMarkAPI.csproj', 
							'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\TrueMarkAPI\\bin\\Release\\net5.0_publish')

						PublishBuildWebServiceToFolder('TaxcomEdoApi', 'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\TaxcomEdoApi\\TaxcomEdoApi.csproj', 
							'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\TaxcomEdoApi\\bin\\Release\\net5.0_publish')

						PublishBuildWebServiceToFolder('CashReceiptApi', 'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\CashReceiptApi\\CashReceiptApi.csproj', 
							'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\CashReceiptApi\\bin\\Release\\net5.0_publish')

						PublishBuildWebServiceToFolder('CashReceiptPrepareWorker', 'Vodovoz\\Source\\Applications\\Backend\\Workers\\IIS\\CashReceiptPrepareWorker\\CashReceiptPrepareWorker.csproj', 
							'Vodovoz\\Source\\Applications\\Backend\\Workers\\IIS\\CashReceiptPrepareWorker\\bin\\Release\\net5.0_publish')

						PublishBuildWebServiceToFolder('CashReceiptSendWorker', 'Vodovoz\\Source\\Applications\\Backend\\Workers\\IIS\\CashReceiptSendWorker\\CashReceiptSendWorker.csproj', 
							'Vodovoz\\Source\\Applications\\Backend\\Workers\\IIS\\CashReceiptSendWorker\\bin\\Release\\net5.0_publish')

						PublishBuildWebServiceToFolder('TrueMarkCodePoolCheckWorker', 'Vodovoz\\Source\\Applications\\Backend\\Workers\\IIS\\TrueMarkCodePoolCheckWorker\\TrueMarkCodePoolCheckWorker.csproj', 
							'Vodovoz\\Source\\Applications\\Backend\\Workers\\IIS\\TrueMarkCodePoolCheckWorker\\bin\\Release\\net5.0_publish')
					}
					else
					{
						//Сборка для проверки что нет ошибок, собранные проекты выкладывать не нужно
						bat '"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe" Vodovoz\\Source\\Vodovoz.sln -t:Build -p:Configuration=Web -p:Platform=x86 -maxcpucount:2'
					}
				}
			}	
		}
	},
	"Linux" : {
		node('LINUX_BUILD'){
			stage('Build WCF'){
				sh 'msbuild /p:Configuration=WCF /p:Platform=x86 Vodovoz/Source/Vodovoz.sln -maxcpucount:2'

				ZipArtifact('Vodovoz/Source/Applications/Backend/Workers/Mono/VodovozSmsInformerService/', 'SmsInformerService')
				ZipArtifact('Vodovoz/Source/Applications/Backend/WCF/VodovozSmsPaymentService/', 'SmsPaymentService')

				archiveArtifacts artifacts: '*Service.zip', onlyIfSuccessful: true
			}
		}						
	}
)

//Копирование на ноды
stage('Copy artifacts'){
	parallel (
		"Desktop vod1" : {
			node('Vod1'){
				CopyDesktopArtifacts("Vod1")
			}			
		},
		"Desktop vod3" : {
			node('Vod3'){
				CopyDesktopArtifacts("Vod3")
			}			
		},
		"Desktop vod5" : {
			node('Vod5'){
				CopyDesktopArtifacts("Vod5")
			}			
		},
		"Desktop vod7" : {
			node('Vod7'){
				CopyDesktopArtifacts("Vod7")
			}			
		},
		"Web" : {
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
		},
		"WCF" : {
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
	)	
}

//Развертывание и публикация сборок на нодах
parallel (
	//Развертывание сборок на нодах
	"Deploy" : {
		stage('Deploy'){
			script{
				node('Vod3'){
					if(CAN_DEPLOY_DESKTOP_BRANCH)
					{
						echo "Deploy branches build to desktop vod3"
						def OUTPUT_PATH = BUILDS_PATH + env.BRANCH_NAME
						unzip zipFile: 'Vodovoz.zip', dir: OUTPUT_PATH
					}
					else if(CAN_DEPLOY_DESKTOP_PR)
					{
						echo "Deploy pull request build to desktop vod3"
						def OUTPUT_PATH = BUILDS_PATH + "pull_requests\\" + env.CHANGE_ID
						unzip zipFile: 'Vodovoz.zip', dir: OUTPUT_PATH
					}
					else
					{
						echo "Deploy desktop builds not needed"
					}
				}	
			}
		}
	},
	//Публикация в предрелизный каталог на нодах
	"Publish" : {
		stage('Publish'){
			parallel (
				"Publish desktop Vod1" : {
					node('Vod1'){
						PublishMasterDesktop()
					}
				},
				"Publish desktop Vod3" : {
					node('Vod3'){
						PublishMasterDesktop()
					}
				},
				"Publish desktop Vod5" : {
					node('Vod5'){
						PublishMasterDesktop()
					}
				},
				"Publish desktop Vod7" : {
					node('Vod7'){
						PublishMasterDesktop()
					}
				},
				"Publish web services" : {
					node('WIN_WEB_RUNTIME'){
						PublishWebServices()
					}
				},
				"Publish WCF services" : {
					node('LINUX_RUNTIME'){
						PublishWCFServices()
					}
				},
			)
		}
	},
)

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

def ZipArtifact(path, serviceName) {
	fileOperations([fileDeleteOperation(excludes: '', includes: "${serviceName}.zip")])
	zip zipFile: "${serviceName}.zip", archive: false, dir: "${path}bin/Debug"  
}

def PublishBuildWebServiceToFolder(serviceName, csprojPath, outputPath) {
	def BRANCH_NAME = env.BRANCH_NAME.replaceAll('/','') + '\\'
	
	echo "Publish ${serviceName} to folder (${env.BRANCH_NAME})"
	bat '"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe" ' + csprojPath + ' /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=FolderProfile -maxcpucount:2'

	
	fileOperations([fileDeleteOperation(excludes: '', includes: "${serviceName}.zip")])
	zip zipFile: "${serviceName}.zip", archive: false, dir: outputPath
	archiveArtifacts artifacts: "${serviceName}.zip", onlyIfSuccessful: true
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
			unzip zipFile: 'Vodovoz.zip', dir: PRERELEASE_PATH
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
	unzip zipFile: "${serviceName}.zip", dir: SERVICE_PATH
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
	unzip zipFile: "${serviceName}.zip", dir: SERVICE_PATH 
}
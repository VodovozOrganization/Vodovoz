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
stage('Restore'){
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

parallel (
	/*"Desktop" : {
		node('Vod3'){
			stage('Deploy desktop'){
				script{
					def BUILDS_PATH = "F:\\WORK\\_BUILDS\\"
					if(env.BRANCH_NAME == 'develop'
						|| env.BRANCH_NAME == 'Beta'
						|| env.BRANCH_NAME ==~ /^[Rr]elease(.*?)/)
					{
						def OUTPUT_PATH = BUILDS_PATH + env.BRANCH_NAME
						echo "Deploy branch " + env.BRANCH_NAME
						copyArtifacts(projectName: '${JOB_NAME}', selector: specific( buildNumber: '${BUILD_NUMBER}'));
						unzip zipFile: 'Vodovoz.zip', dir: OUTPUT_PATH
					} else if(env.CHANGE_ID != null){
						def OUTPUT_PATH = BUILDS_PATH + "pull_requests\\" + env.CHANGE_ID
						echo "Deploy pull request " + env.CHANGE_ID
						copyArtifacts(projectName: '${JOB_NAME}', selector: specific( buildNumber: '${BUILD_NUMBER}'));
						unzip zipFile: 'Vodovoz.zip', dir: OUTPUT_PATH
					} else{
						echo "Nothing to deploy"
					}
				}
			}		
		}
	},*/
	"DesktopRuntime" : {
		node('DESKTOP_RUNTIME'){
			stage('Deploy master desktop'){
				script{
					/*if(env.BRANCH_NAME == 'master')
					{*/
						DeployWinRuntime();
					/*}else{
						echo "Nothing to publish"
					}*/
				}
			}		
		}
	}/*,
	"Linux" : {
		node('LINUX_RUNTIME'){
			stage('Deploy WCF'){
				script{					
					 if(env.BRANCH_NAME == 'master')
					 {					
						copyArtifacts(projectName: '${JOB_NAME}', selector: specific( buildNumber: '${BUILD_NUMBER}'));

						UnzipArtifact('SmsInformerService')
						UnzipArtifact('SmsPaymentService')
					 } else{
					 	echo "Nothing to deploy"
					 }
				}
			}					
		}	
	},
	"WEB" : {
		node('WIN_WEB_RUNTIME'){
			stage('Deploy WEB'){
				if(env.BRANCH_NAME ==~ /(develop|master)/ || env.BRANCH_NAME ==~ /^[Rr]elease(.*?)/)
				{
					copyArtifacts(projectName: '${JOB_NAME}', selector: specific( buildNumber: '${BUILD_NUMBER}'));

					DeployWebService('DriversAPI')
					DeployWebService('FastPaymentsAPI')
					DeployWebService('MailjetEventsDistributorAPI')
					DeployWebService('UnsubscribePage')
					DeployWebService('DeliveryRulesService')
					DeployWebService('RoboatsService')
					DeployWebService('TaxcomEdoApi')
					DeployWebService('TrueMarkAPI')
					DeployWebService('PayPageAPI')
					DeployWebService('CashReceiptApi')
					DeployWebService('CashReceiptPrepareWorker')
					DeployWebService('CashReceiptSendWorker')
					DeployWebService('TrueMarkCodePoolCheckWorker')
				}
				else
				{
					echo 'Skipped, branch (' + env.BRANCH_NAME + ')'
				}
			}
		}						
	}*/
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

def UnzipArtifact(serviceName) {
	def SERVICE_PATH = "/opt/jenkins/builds/${serviceName}"
	unzip zipFile: "${serviceName}.zip", dir: SERVICE_PATH 
}

def PublishBuildWebServiceToFolder(serviceName, csprojPath, outputPath) {
	def BRANCH_NAME = env.BRANCH_NAME.replaceAll('/','') + '\\'
	
	echo "Publish ${serviceName} to folder (${env.BRANCH_NAME})"
	bat '"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe" ' + csprojPath + ' /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=FolderProfile -maxcpucount:2'

	
	fileOperations([fileDeleteOperation(excludes: '', includes: "${serviceName}.zip")])
	zip zipFile: "${serviceName}.zip", archive: false, dir: outputPath
	archiveArtifacts artifacts: "${serviceName}.zip", onlyIfSuccessful: true
}

def PublishBuildWebServiceToDockerRegistry(serviceName, csprojPath) {	
	echo "Publish ${serviceName} to docker registry"
	sh 'msbuild ' + csprojPath + ' /p:Configuration=Web /p:Platform=x86 /p:DeployOnBuild=true /p:PublishProfile=DockerRegistry -maxcpucount:2'
}

def DeployWebService(serviceName) {
	def BRANCH_NAME = env.BRANCH_NAME.replaceAll('/','') + '\\'
	def SERVICE_PATH = "E:\\CD\\${serviceName}\\${BRANCH_NAME}"
	
    echo "Deploy ${serviceName} to CD folder"
	unzip zipFile: "${serviceName}.zip", dir: SERVICE_PATH
}

def DeployWinRuntime() {
	def RUNTIME_DEPLOY_PATH = "${RuntimePath}\\DeployInProgress"
	def RUNTIME_LATEST_PATH = "${RuntimePath}\\latest"

	echo "Deploy master to runtime folder to ${RUNTIME_LATEST_PATH}"
	copyArtifacts(projectName: '${JOB_NAME}', selector: specific( buildNumber: '${BUILD_NUMBER}'));

	bat 'rename "' + RUNTIME_LATEST_PATH + '" "' + RUNTIME_DEPLOY_PATH + '"'
	unzip zipFile: 'Vodovoz.zip', dir: RUNTIME_DEPLOY_PATH
	bat 'rename "' + RUNTIME_DEPLOY_PATH + '" "' + RUNTIME_LATEST_PATH + '"'
}
stage('Checkout'){
	parallel (
		"Desktop" : {
			node('DESKTOP_BUILD'){
				PrepareSources("${JENKINS_HOME_WIN}")
			}
		},
		"WCF" : {
			node('WCF_BUILD'){
				PrepareSources("${JENKINS_HOME}")
			}						
		}
	)				
}
stage('Restore'){
	parallel (
		"Desktop" : {
			node('DESKTOP_BUILD'){
				bat '"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe" Vodovoz\\Source\\Vodovoz.sln -t:Restore -p:Configuration=DebugWin -p:Platform=x86'
			}
		},
		"WCF" : {
			node('WCF_BUILD'){
				sh 'nuget restore Vodovoz/Source/Vodovoz.sln'
				sh 'nuget restore Vodovoz/Source/Libraries/External/QSProjects/QSProjectsLib.sln'
				sh 'nuget restore Vodovoz/Source/Libraries/External/My-FyiReporting/MajorsilenceReporting-Linux-GtkViewer.sln'
			}						
		}
	)				
}
parallel (
	"Desktop" : {
		node('DESKTOP_BUILD'){
			stage('Build Desktop'){
				bat '"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe" Vodovoz\\Source\\Vodovoz.sln -t:Build -p:Configuration=WinDesktop -p:Platform=x86'

				fileOperations([fileDeleteOperation(excludes: '', includes: 'Vodovoz.zip')])
				zip zipFile: 'Vodovoz.zip', archive: false, dir: 'Vodovoz/Source/Applications/Desktop/Vodovoz/bin/DebugWin'
				archiveArtifacts artifacts: 'Vodovoz.zip', onlyIfSuccessful: true			
			}

			stage('Build WEB'){
				if(env.BRANCH_NAME ==~ /(develop|master)/ || env.BRANCH_NAME ==~ /^[Rr]elease(.*?)/)
				{				
					PublishBuildWebService('DriversAPI', 'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\DriverAPI\\DriverAPI.csproj', 
						'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\DriverAPI\\bin\\Release\\net5.0_publish')

					PublishBuildWebService('FastPaymentsAPI', 'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\FastPaymentsAPI\\FastPaymentsAPI.csproj', 
						'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\FastPaymentsAPI\\bin\\Release\\net5.0_publish')

					PublishBuildWebService('PayPageAPI', 'Vodovoz\\Source\\Applications\\Frontend\\PayPageAPI\\PayPageAPI.csproj', 
						'Vodovoz\\Source\\Applications\\Frontend\\PayPageAPI\\bin\\Release\\net5.0_publish')

					PublishBuildWebService('MailjetEventsDistributorAPI', 'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\Email\\MailjetEventsDistributorAPI\\MailjetEventsDistributorAPI.csproj', 
						'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\Email\\MailjetEventsDistributorAPI\\bin\\Release\\net5.0_publish')
						
					PublishBuildWebService('UnsubscribePage', 'Vodovoz\\Source\\Applications\\Frontend\\UnsubscribePage\\UnsubscribePage.csproj', 
						'Vodovoz\\Source\\Applications\\Frontend\\UnsubscribePage\\bin\\Release\\net5.0_publish')

					PublishBuildWebService('DeliveryRulesService', 'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\DeliveryRulesService\\DeliveryRulesService.csproj', 
						'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\DeliveryRulesService\\bin\\Release\\net5.0_publish')

					PublishBuildWebService('RoboAtsService', 'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\RoboAtsService\\RoboAtsService.csproj', 
						'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\RoboAtsService\\bin\\Release\\net5.0_publish')

					PublishBuildWebService('TrueMarkAPI', 'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\TrueMarkAPI\\TrueMarkAPI.csproj', 
						'Vodovoz\\Source\\Applications\\Backend\\WebAPI\\TrueMarkAPI\\bin\\Release\\net5.0_publish')
				}
				else
				{
					//Сборка для проверки что нет ошибок, собранные проекты выкладывать не нужно
					bat '"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe" Vodovoz\\Source\\Vodovoz.sln -t:Build -p:Configuration=Web -p:Platform=x86'
				}
			}
			
		}
	},
	"WCF" : {
		node('WCF_BUILD'){
			stage('Build WCF'){
				sh 'msbuild /p:Configuration=WCF /p:Platform=x86 Vodovoz/Source/Vodovoz.sln -maxcpucount:4'

				ZipArtifact('Vodovoz/Source/Applications/Backend/WCF/VodovozInstantSmsService/', 'InstantSmsService')
				ZipArtifact('Vodovoz/Source/Applications/Backend/Workers/Mono/VodovozSalesReceiptsService/', 'SalesReceiptsService')
				ZipArtifact('Vodovoz/Source/Applications/Backend/Workers/Mono/VodovozSmsInformerService/', 'SmsInformerService')
				ZipArtifact('Vodovoz/Source/Applications/Backend/WCF/VodovozSmsPaymentService/', 'SmsPaymentService')

				archiveArtifacts artifacts: '*Service.zip', onlyIfSuccessful: true
			}
		}						
	}
)

parallel (
	"Desktop" : {
		node('DESKTOP_RUNTIME'){
			stage('Deploy Desktop'){
				script{
					def BUILDS_PATH = "F:\\WORK\\_BUILDS\\"
					if(
						env.BRANCH_NAME == 'master'
						|| env.BRANCH_NAME == 'develop'
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
	},
	"WCF" : {
		node('WCF_RUNTIME'){
			stage('Deploy WCF'){
				script{					
					if(env.BRANCH_NAME == 'master')
					{					
						copyArtifacts(projectName: '${JOB_NAME}', selector: specific( buildNumber: '${BUILD_NUMBER}'));

						UnzipArtifact('InstantSmsService')
						UnzipArtifact('SalesReceiptsService')
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
					DeployWebService('RoboAtsService')
					DeployWebService('TrueMarkAPI')
				}
				else
				{
					echo 'Skipped, branch (' + env.BRANCH_NAME + ')'
				}
			}
		}						
	}
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

def PublishBuildWebService(serviceName, csprojPath, outputPath) {
	def BRANCH_NAME = env.BRANCH_NAME.replaceAll('/','') + '\\'
	
	echo "Publish ${serviceName} to folder (${env.BRANCH_NAME})"
	bat '"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe" ' + csprojPath + ' /p:Configuration=Web /p:Platform=x86 /p:DeployOnBuild=true /p:PublishProfile=FolderProfile'

	
	fileOperations([fileDeleteOperation(excludes: '', includes: "${serviceName}.zip")])
	zip zipFile: "${serviceName}.zip", archive: false, dir: outputPath
	archiveArtifacts artifacts: "${serviceName}.zip", onlyIfSuccessful: true
}

def DeployWebService(serviceName) {
	def BRANCH_NAME = env.BRANCH_NAME.replaceAll('/','') + '\\'
	def SERVICE_PATH = "E:\\CD\\${serviceName}\\${BRANCH_NAME}"
	
    echo "Deploy ${serviceName} to CD folder"
	unzip zipFile: "${serviceName}.zip", dir: SERVICE_PATH
}
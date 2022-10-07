stage('Checkout'){
	parallel (
		"Desktop" : {
			node('Vod6'){
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
			node('Vod6'){
				bat '"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe" Source\\Vodovoz.sln -t:Restore -p:Configuration=DebugWin -p:Platform=x86'
			}
		},
		"WCF" : {
			node('WCF_BUILD'){
				sh 'nuget restore Source/Vodovoz.sln'
				sh 'nuget restore Source/Libraries/External/QSProjects/QSProjectsLib.sln'
				sh 'nuget restore Source/Libraries/External/My-FyiReporting/MajorsilenceReporting-Linux-GtkViewer.sln'
			}						
		}
	)				
}
parallel (
	"Desktop" : {
		node('Vod6'){
			stage('Build Desktop, WEB'){
				bat '"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe" Source\\Vodovoz.sln -t:Build -p:Configuration=DebugWin -p:Platform=x86'

				fileOperations([fileDeleteOperation(excludes: '', includes: 'Vodovoz.zip')])
				zip zipFile: 'Vodovoz.zip', archive: false, dir: 'Source/Applications/Desktop/Vodovoz/bin/DebugWin'
				archiveArtifacts artifacts: 'Vodovoz.zip', onlyIfSuccessful: true			
			}
		}
	},
	"WCF" : {
		node('WCF_BUILD'){
			stage('Build WCF'){
				sh 'msbuild /p:Configuration=WCF /p:Platform=x86 Source/Vodovoz.sln -maxcpucount:4'

				ZipArtifact('Source/Applications/Backend/WCF/VodovozDeliveryRulesService', 'DeliveryRulesService')
				ZipArtifact('Source/Applications/Backend/WCF/VodovozInstantSmsService', 'InstantSmsService')
				ZipArtifact('Source/Applications/Backend/Workers/Mono/VodovozSalesReceiptsService', 'SalesReceiptsService')
				ZipArtifact('Source/Applications/Backend/Workers/Mono/VodovozSmsInformerService', 'SmsInformerService')
				ZipArtifact('Source/Applications/Backend/WCF/VodovozSmsPaymentService', 'SmsPaymentService')

				archiveArtifacts artifacts: '*Service.zip', onlyIfSuccessful: true
			}
		}						
	}
)

parallel (
	"Desktop" : {
		node('Vod3'){
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

						UnzipArtifact('DeliveryRulesService')
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
		node('Vod6'){
			stage('Deploy WEB'){
				echo 'DriverAPI Deploy'
				if(env.BRANCH_NAME ==~ /(develop|master)/
					|| env.BRANCH_NAME ==~ /^[Rr]elease(.*?)/)
				{
					echo 'Publish DriverAPI to folder (' + env.BRANCH_NAME + ')'
					bat '"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe" Source\\Applications\\Backend\\WebAPI\\DriverAPI\\DriverAPI.csproj /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=FolderProfile'
					
					echo 'Move files to CD folder'
					bat 'xcopy "Source\\Applications\\Backend\\WebAPI\\DriverAPI\\bin\\Release\\net5.0\\publish" "E:\\CD\\DriversAPI\\' + env.BRANCH_NAME.replaceAll('/','') + '\\" /R /Y /E'
				}
				else
				{
					echo 'Skipped, branch (' + env.BRANCH_NAME + ')'
				}

				echo 'FastPaymentsAPI Deploy'
				if(env.BRANCH_NAME ==~ /(develop|master)/
					|| env.BRANCH_NAME ==~ /^[Rr]elease(.*?)/)
				{
					echo 'Publish FastPaymentsAPI to folder (' + env.BRANCH_NAME + ')'
					bat '"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe" Source\\Applications\\Backend\\WebAPI\\FastPaymentsAPI\\FastPaymentsAPI.csproj /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=FolderProfile'
					
					echo 'Move files to CD folder'
					bat 'xcopy "Source\\Applications\\Backend\\WebAPI\\FastPaymentsAPI\\bin\\Release\\net5.0\\publish" "E:\\CD\\FastPaymentsAPI\\' + env.BRANCH_NAME.replaceAll('/','') + '\\" /R /Y /E'
				}
				else
				{
					echo 'Skipped, branch (' + env.BRANCH_NAME + ')'
				}

				echo 'PayPageAPI Deploy'
				if(env.BRANCH_NAME ==~ /(develop|master)/
					|| env.BRANCH_NAME ==~ /^[Rr]elease(.*?)/)
				{
					echo 'Publish PayPageAPI to folder (' + env.BRANCH_NAME + ')'
					bat '"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe" Source\\Applications\\Frontend\\PayPageAPI\\PayPageAPI.csproj /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=FolderProfile'
					
					echo 'Move files to CD folder'
					bat 'xcopy "Source\\Applications\\Frontend\\PayPageAPI\\bin\\Release\\net5.0\\publish" "E:\\CD\\PayPageAPI\\' + env.BRANCH_NAME.replaceAll('/','') + '\\" /R /Y /E'
				}
				else
				{
					echo 'Skipped, branch (' + env.BRANCH_NAME + ')'
				}

				echo 'MailjetEventsDistributorAPI Deploy'
				if(env.BRANCH_NAME ==~ /(develop|master)/
					|| env.BRANCH_NAME ==~ /^[Rr]elease(.*?)/)
				{
					echo 'Publish MailjetEventsDistributorAPI to folder (' + env.BRANCH_NAME + ')'
					bat '"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe" Source\\Applications\\Backend\\WebAPI\\Email\\MailjetEventsDistributorAPI\\MailjetEventsDistributorAPI.csproj /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=FolderProfile'
					
					echo 'Move files to CD folder'
					bat 'xcopy "Source\\Applications\\Backend\\WebAPI\\Email\\MailjetEventsDistributorAPI\\bin\\Release\\net5.0\\publish" "E:\\CD\\MailjetEventsDistributorAPI\\' + env.BRANCH_NAME.replaceAll('/','') + '\\" /R /Y /E'
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
		doGenerateSubmoduleConfigurations: false,
		extensions: scm.extensions 
		+ [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'Vodovoz']]
		+ [[$class: 'CloneOption', reference: "${REFERENCE_ABSOLUTE_PATH}/Vodovoz"]],
		+ [[$class: 'SubmoduleOption', recursiveSubmodules: true, parentCredentials: true]],
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
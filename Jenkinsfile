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
				bat '"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe" Vodovoz\\Vodovoz.sln -t:Restore -p:Configuration=DebugWin -p:Platform=x86'
			}
		},
		"WCF" : {
			node('WCF_BUILD'){
				sh 'nuget restore Vodovoz/Vodovoz.sln'
				sh 'nuget restore QSProjects/QSProjectsLib.sln'
				sh 'nuget restore My-FyiReporting/MajorsilenceReporting-Linux-GtkViewer.sln'
			}						
		}
	)				
}
stage('Build'){
	parallel (
		"Desktop" : {
			node('Vod6'){
				bat '"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe" Vodovoz\\Vodovoz.sln -t:Build -p:Configuration=DebugWin -p:Platform=x86'

				fileOperations([fileDeleteOperation(excludes: '', includes: 'Vodovoz.zip')])
				zip zipFile: 'Vodovoz.zip', archive: false, dir: 'Vodovoz/Vodovoz/bin/DebugWin'
				archiveArtifacts artifacts: 'Vodovoz.zip', onlyIfSuccessful: true			
			}
		},
		"WCF" : {
			node('WCF_BUILD'){
				sh 'msbuild /p:Configuration=WCF /p:Platform=x86 Vodovoz/Vodovoz.sln -maxcpucount:4'

				ZipArtifact('DeliveryRuleService')
				ZipArtifact('InstantSmsService')
				ZipArtifact('SalesReceiptsService')
				ZipArtifact('SmsInformerService')
				ZipArtifact('SmsPaymentService')

				archiveArtifacts artifacts: '*Service.zip', onlyIfSuccessful: true
			}						
		}
	)				
}
stage('Deploy'){
	parallel (
		"Desktop" : {
			node('Vod3'){
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
		},
		"WCF" : {
			node('WCF_RUNTIME'){
				script{					
					//if(env.BRANCH_NAME == 'master')
					//{					
						copyArtifacts(projectName: '${JOB_NAME}', selector: specific( buildNumber: '${BUILD_NUMBER}'));

						UnzipArtifact('DeliveryRuleService')
						UnzipArtifact('InstantSmsService')
						UnzipArtifact('SalesReceiptsService')
						UnzipArtifact('SmsInformerService')
						UnzipArtifact('SmsPaymentService')
					//} else{
					//	echo "Nothing to deploy"
					//}
				}
			}						
		},
		"WEB" : {
			node('Vod6'){
				step('DriverAPI Deploy')
				{
					if(env.BRANCH_NAME ==~ /(develop|master)/
						|| env.BRANCH_NAME ==~ /^[Rr]elease(.*?)/)
					{
						echo 'Publish DriverAPI to folder (' + env.BRANCH_NAME + ')'
						bat '"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe" Vodovoz\\Services\\WebApi\\DriverAPI\\DriverAPI.csproj /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=FolderProfile'
						
						echo 'Move files to CD folder'
						bat 'xcopy "Vodovoz\\Services\\WebApi\\DriverAPI\\bin\\Release\\net5.0\\publish" "E:\\CD\\DriversAPI\\' + env.BRANCH_NAME.replaceAll('/','') + '\\" /R /Y /E'
					}
					else
					{
						echo 'Skipped, branch (' + env.BRANCH_NAME + ')'
					}
				}
				step('FastPaymentsAPI Deploy')
				{
					if(env.BRANCH_NAME ==~ /(develop|master)/
						|| env.BRANCH_NAME ==~ /^[Rr]elease(.*?)/)
					{
						echo 'Publish FastPaymentsAPI to folder (' + env.BRANCH_NAME + ')'
						bat '"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe" Vodovoz\\Services\\WebApi\\FastPaymentsAPI\\FastPaymentsAPI.csproj /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=FolderProfile'
						
						echo 'Move files to CD folder'
						bat 'xcopy "Vodovoz\\Services\\WebApi\\FastPaymentsAPI\\bin\\Release\\net5.0\\publish" "E:\\CD\\FastPaymentsAPI\\' + env.BRANCH_NAME.replaceAll('/','') + '\\" /R /Y /E'
					}
					else
					{
						echo 'Skipped, branch (' + env.BRANCH_NAME + ')'
					}
				}
				step('PayPageAPI Deploy')
				{
					if(env.BRANCH_NAME ==~ /(develop|master)/
						|| env.BRANCH_NAME ==~ /^[Rr]elease(.*?)/)
					{
						echo 'Publish PayPageAPI to folder (' + env.BRANCH_NAME + ')'
						bat '"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe" Vodovoz\\Services\\WebApi\\PayPageAPI\\PayPageAPI.csproj /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=FolderProfile'
						
						echo 'Move files to CD folder'
						bat 'xcopy "Vodovoz\\Services\\WebApi\\PayPageAPI\\bin\\Release\\net5.0\\publish" "E:\\CD\\PayPageAPI\\' + env.BRANCH_NAME.replaceAll('/','') + '\\" /R /Y /E'
					}
					else
					{
						echo 'Skipped, branch (' + env.BRANCH_NAME + ')'
					}
				}
				step('MailjetEventsDistributorAPI Deploy')
				{
					if(env.BRANCH_NAME ==~ /(develop|master)/
						|| env.BRANCH_NAME ==~ /^[Rr]elease(.*?)/)
					{
						echo 'Publish MailjetEventsDistributorAPI to folder (' + env.BRANCH_NAME + ')'
						bat '"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe" Vodovoz\\Services\\WebApi\\MailjetEventsDistributorAPI\\MailjetEventsDistributorAPI.csproj /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=FolderProfile'
						
						echo 'Move files to CD folder'
						bat 'xcopy "Vodovoz\\Services\\WebApi\\MailjetEventsDistributorAPI\\bin\\Release\\net5.0\\publish" "E:\\CD\\MailjetEventsDistributorAPI\\' + env.BRANCH_NAME.replaceAll('/','') + '\\" /R /Y /E'
					}
					else
					{
						echo 'Skipped, branch (' + env.BRANCH_NAME + ')'
					}
				}
			}						
		}
	)				
}

def PrepareSources(jenkinsHome) {
    def REFERENCE_ABSOLUTE_PATH = "$jenkinsHome/workspace/Vodovoz_Vodovoz_master"

	echo "checkout Gtk.DataBindings"	
	checkout changelog: false, poll: false, scm:([
		$class: 'GitSCM',
		branches: [[name: '*/vodovoz']],
		doGenerateSubmoduleConfigurations: false,
		extensions: 
		[[$class: 'RelativeTargetDirectory', relativeTargetDir: 'Gtk.DataBindings']]
		+ [[$class: 'CloneOption', reference: "${REFERENCE_ABSOLUTE_PATH}/Gtk.DataBindings"]],
		userRemoteConfigs: [[url: 'https://github.com/QualitySolution/Gtk.DataBindings.git']]
	])

	echo "checkout GMap.NET"	
	checkout changelog: false, poll: false, scm:([
		$class: 'GitSCM',
		branches: [[name: '*/master']],
		doGenerateSubmoduleConfigurations: false,
		extensions:
		[[$class: 'RelativeTargetDirectory', relativeTargetDir: 'GMap.NET']]
		+ [[$class: 'CloneOption', reference: "${REFERENCE_ABSOLUTE_PATH}/GMap.NET"]],
		userRemoteConfigs: [[url: 'https://github.com/QualitySolution/GMap.NET.git']]
	])

	echo "checkout My-FyiReporting"	
	checkout changelog: false, poll: false, scm:([
		$class: 'GitSCM',
		branches: [[name: '*/Vodovoz']],
		doGenerateSubmoduleConfigurations: false,
		extensions:
		[[$class: 'RelativeTargetDirectory', relativeTargetDir: 'My-FyiReporting']]
		+ [[$class: 'CloneOption', reference: "${REFERENCE_ABSOLUTE_PATH}/My-FyiReporting"]],
		userRemoteConfigs: [[url: 'https://github.com/QualitySolution/My-FyiReporting.git']]
	])

	echo "checkout QSProjects"	
	checkout changelog: false, poll: false, scm:([
		$class: 'GitSCM',
		branches: [[name: '*/VodovozMonitoring']],
		doGenerateSubmoduleConfigurations: false,
		extensions:
		[[$class: 'RelativeTargetDirectory', relativeTargetDir: 'QSProjects']]
		+ [[$class: 'CloneOption', reference: "${REFERENCE_ABSOLUTE_PATH}/QSProjects"]],
		userRemoteConfigs: [[url: 'https://github.com/QualitySolution/QSProjects.git']]
	])

	echo "checkout Vodovoz"	
	checkout changelog: false, poll: false, scm:([
		$class: 'GitSCM',
		branches: scm.branches,
		doGenerateSubmoduleConfigurations: scm.doGenerateSubmoduleConfigurations,
		extensions: scm.extensions 
		+ [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'Vodovoz']]
		+ [[$class: 'CloneOption', reference: "${REFERENCE_ABSOLUTE_PATH}/Vodovoz"]],
		userRemoteConfigs: scm.userRemoteConfigs
	])
}

def ZipArtifact(serviceName) {
	fileOperations([fileDeleteOperation(excludes: '', includes: '${serviceName}.zip')])
	zip zipFile: '${serviceName}.zip', archive: false, dir: 'Vodovoz/Services/WCF/${serviceName}/bin/Debug'  
}

def UnzipArtifact(serviceName) {
	def SERVICE_PATH = "/opt/jenkins/builds/$serviceName"
	unzip zipFile: '${serviceName}.zip', dir: SERVICE_PATH 
}
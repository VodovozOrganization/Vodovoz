node('Vod6'){
	stage('Checkout'){
		def REFERENCE_ABSOLUTE_PATH = "${JENKINS_HOME_WIN}/workspace/Vodovoz_Vodovoz_master"

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
			branches: [[name: '*/QSBuild']],
			doGenerateSubmoduleConfigurations: false,
			extensions:
			[[$class: 'RelativeTargetDirectory', relativeTargetDir: 'My-FyiReporting']]
			+ [[$class: 'CloneOption', reference: "${REFERENCE_ABSOLUTE_PATH}/My-FyiReporting"]],
			userRemoteConfigs: [[url: 'https://github.com/QualitySolution/My-FyiReporting.git']]
		])

		echo "checkout QSProjects"	
		checkout changelog: false, poll: false, scm:([
			$class: 'GitSCM',
			branches: [[name: '*/master']],
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
	stage('Restore'){
		echo 'Prepare Vodovoz'	
		bat '"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe" Vodovoz\\Vodovoz.sln -t:Restore -p:Configuration=DebugWin -p:Platform=x86'
	}
	stage('Build'){
		echo 'Build solution'
		bat '"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe" Vodovoz\\Vodovoz.sln -t:Build -p:Configuration=DebugWin -p:Platform=x86'

		fileOperations([fileDeleteOperation(excludes: '', includes: 'Vodovoz.zip')])
		zip zipFile: 'Vodovoz.zip', archive: false, dir: 'Vodovoz/Vodovoz/bin/DebugWin'
		archiveArtifacts artifacts: 'Vodovoz.zip', onlyIfSuccessful: true
	}
	stage('DriverAPI Delivery')
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
	stage('FastPaymentsAPI Deploy')
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
	stage('MailjetEventsDistributorAPI Deploy')
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
node('Vod3') {
	stage('Deploy'){
		echo "Checking the deployment for a branch " + env.BRANCH_NAME
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

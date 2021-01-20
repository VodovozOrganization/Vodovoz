node {
   stage('Gtk.DataBindings') {
      checkout changelog: false, poll: false, scm: [$class: 'GitSCM', branches: [[name: '*/master']], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'Gtk.DataBindings']], submoduleCfg: [], userRemoteConfigs: [[url: 'https://github.com/QualitySolution/Gtk.DataBindings.git']]]
   }
   stage('GammaBinding') {
      checkout changelog: false, poll: false, scm: [$class: 'GitSCM', branches: [[name: '*/master']], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'GammaBinding']], submoduleCfg: [], userRemoteConfigs: [[url: 'https://github.com/QualitySolution/GammaBinding.git']]]
   }
   stage('GMap.NET') {
      checkout changelog: false, poll: false, scm: [$class: 'GitSCM', branches: [[name: '*/master']], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'GMap.NET']], submoduleCfg: [], userRemoteConfigs: [[url: 'https://github.com/QualitySolution/GMap.NET.git']]]
   }
   stage('My-FyiReporting') {
      checkout changelog: false, scm: [$class: 'GitSCM', branches: [[name: '*/QSBuild']], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'My-FyiReporting']], submoduleCfg: [], userRemoteConfigs: [[url: 'https://github.com/QualitySolution/My-FyiReporting.git']]]
      sh 'nuget restore My-FyiReporting/MajorsilenceReporting-Linux-GtkViewer.sln'
   }
   stage('QSProjects') {
      checkout([$class: 'GitSCM', branches: [[name: '*/release/1.3.1']], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'QSProjects']], submoduleCfg: [], userRemoteConfigs: [[url: 'https://github.com/QualitySolution/QSProjects.git']]])
      sh 'nuget restore QSProjects/QSProjectsLib.sln'
   }
   stage('Vodovoz') {
      checkout([
         $class: 'GitSCM',
         branches: scm.branches,
         doGenerateSubmoduleConfigurations: scm.doGenerateSubmoduleConfigurations,
         extensions: scm.extensions + [[$class: 'RelativeTargetDirectory', relativeTargetDir: 'Vodovoz']],
         userRemoteConfigs: scm.userRemoteConfigs
      ])
      sh 'nuget restore Vodovoz/Vodovoz.sln'
   }
   stage('Build') {
        sh 'msbuild /p:Configuration=DebugWin /p:Platform=x86 Vodovoz/Vodovoz.sln'
        fileOperations([fileDeleteOperation(excludes: '', includes: 'Vodovoz.zip')])
        zip zipFile: 'Vodovoz.zip', archive: false, dir: 'Vodovoz/Vodovoz/bin/DebugWin'
        archiveArtifacts artifacts: 'Vodovoz.zip', onlyIfSuccessful: true
   }
}
node('Vod3') {
	stage('Deploy'){
		echo "Checking the deployment for a branch " + env.BRANCH_NAME
		script{
			if(env.BRANCH_NAME == 'master'){
				echo "Deploy branch " + env.BRANCH_NAME
				copyArtifacts(projectName: '${JOB_NAME}', selector: specific( buildNumber: '${BUILD_NUMBER}'));
				unzip zipFile: 'Vodovoz.zip', dir: 'F:\\WORK\\_BUILDS\\master'
			} else if(env.BRANCH_NAME == 'develop'){
				echo "Deploy branch " + env.BRANCH_NAME
				copyArtifacts(projectName: '${JOB_NAME}', selector: specific( buildNumber: '${BUILD_NUMBER}'));
				unzip zipFile: 'Vodovoz.zip', dir: 'F:\\WORK\\_BUILDS\\develop'
			} else if(env.BRANCH_NAME ==~ /^[Rr]elease(.*?)/){
				echo "Deploy branch " + env.BRANCH_NAME
				copyArtifacts(projectName: '${JOB_NAME}', selector: specific( buildNumber: '${BUILD_NUMBER}'));
				unzip zipFile: 'Vodovoz.zip', dir: 'F:\\WORK\\_BUILDS\\' + env.BRANCH_NAME
			} else{
				echo "Nothing to deploy"
			}
		}
   }
}
 

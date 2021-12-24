node('Vod6'){
	def pr_ref = env.BRANCH_NAME;
	echo pr_ref;

	if(env.CHANGE_ID == null){
		echo "Branch is not PR"
	}else{
		echo "Is PR " + env.CHANGE_ID
	}	
}

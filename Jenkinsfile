node('Vod6'){
	def pr_ref = env.BRANCH_NAME;
	echo pr_ref;

	if(enc.CHANGE_ID == null){
		echo "Branch is not PR"
	}else{
		echo "Is PR " + enc.CHANGE_ID
	}	
}

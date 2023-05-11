#!/bin/bash

echo "Какие службы необходимо обновить?"
echo "1) SmsInformer"
echo "2) SmsPayment"
echo "3) Mango"

echo "Можно вызывать вместе, перечислив номера через запятую, например SmsInformer+SmsPayment=1,2"
read service;

echo "Какую сборку использовать?"
echo "1) Release"
echo "2) Debug"
read build;

smsServiceFolder="VodovozSmsInformerService"
smsServiceName="vodovoz-smsinformer.service"

smsPaymentServiceFolder="VodovozSmsPaymentService"
smsPaymentServiceName="vodovoz-sms-payment.service"

mangoServiceFolder="VodovozMangoService"
mangoServiceName="vodovoz-mango.service"

serverAddress="root@srv2.vod.qsolution.ru"
serverPort="2203"

buildFolderName=""
case $build in
	1)
		buildFolderName="Release"
	;;
	2)
		buildFolderName="Debug"
	;;
esac

function DeleteHttpDll {
	deletedFilePath="./$1/$2/bin/$buildFolderName/System.Net.Http.dll"

	echo "-- Delete incorrect generated files: $deletedFilePath"

	if [ -f $deletedFilePath ]
		then rm $deletedFilePath
	fi
}

function CopyFiles {
	rsync -vizaP --delete -e "ssh -p $serverPort" ./$1/$2/bin/$buildFolderName/ $serverAddress:/opt/$2
}

function CopyFilesPublished {
	rsync -vizaP --delete -e "ssh -p $serverPort" $1/$2/bin/$buildFolderName/$3/publish/ $serverAddress:/opt/$2
}

function PublishProject {
    dotnet build "$1/$2" --configuration $buildFolderName
    dotnet publish "$1/$2" --configuration $buildFolderName
}

function UpdateSMSInformerService {
	printf "\nОбновление службы SMS информирования\n"

	echo "-- Stoping $smsServiceName"
	ssh $serverAddress -p$serverPort sudo systemctl stop $smsServiceName

	echo "-- Copying $smsServiceName files"
	DeleteHttpDll "Workers/Mono" $smsServiceFolder
	CopyFiles "Workers/Mono" $smsServiceFolder

	echo "-- Starting $smsServiceName"
	ssh $serverAddress -p$serverPort sudo systemctl start $smsServiceName
}

function UpdateSmsPaymentService {
	printf "\nОбновление службы отправки платежей по sms\n"

	echo "-- Stoping $smsPaymentServiceName"
	ssh $serverAddress -p$serverPort sudo systemctl stop $smsPaymentServiceName

	echo "-- Copying $smsPaymentServiceName files"
	DeleteHttpDll "WCF" $smsPaymentServiceFolder
	CopyFiles "WCF" $smsPaymentServiceFolder

	echo "-- Starting $smsPaymentServiceName"
	ssh $serverAddress -p$serverPort sudo systemctl start $smsPaymentServiceName
}

function UpdateMangoService {
	printf "\nОбновление службы работы с Mango\n"

  	PublishProject "WebAPI" $mangoServiceFolder

	ssh $serverAddress -p$serverPort sudo systemctl stop $mangoServiceName
	echo "-- Stoping $mangoServiceName"

	echo "-- Copying $mangoServiceName files"
	
	CopyFilesPublished "WebAPI" $mangoServiceFolder "netcoreapp3.1"

	echo "-- Starting $mangoServiceName"
	ssh $serverAddress -p$serverPort sudo systemctl start $mangoServiceName
}

service2=",$service,"
case $service2 in
	*,1,*)
		UpdateSMSInformerService
	;;&
	*,2,*)
		UpdateSmsPaymentService
	;;&
	*,3,*)
		UpdateMangoService
	;;
esac

read -p "Press enter to exit"
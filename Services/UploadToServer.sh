#!/bin/bash

echo "Какие службы необходимо обновить?"
echo "1) Driver"
echo "2) Email"
echo "3) Mobile"
echo "4) OSM"
echo "5) SmsInformer"
echo "6) ModulKassa (SalesReceipts)"
echo "7) InstantSms"
echo "8) DeliveryRules"
echo "9) SmsPayment"
echo "10) Mango"

echo "Можно вызывать вместе, перечислив номера через запятую, например Driver+Email=1,2"
read service;

echo "Какую сборку использовать?"
echo "1) Release"
echo "2) Debug"
read build;

emailServiceFolder="VodovozEmailService"
emailServiceName="vodovoz-email.service"

smsServiceFolder="VodovozSmsInformerService"
smsServiceName="vodovoz-smsinformer.service"

kassaServiceFolder="VodovozSalesReceiptsService"
kassaServiceName="vodovoz-sales-receipts.service"

instantSmsServiceFolder="VodovozInstantSmsService"
instantSmsServiceName="vodovoz-instant-sms.service"

deliveryRulesServiceFolder="VodovozDeliveryRulesService"
deliveryRulesServiceName="vodovoz-delivery-rules.service"

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
	deletedFilePath="./Application/$1/bin/$buildFolderName/System.Net.Http.dll"

	echo "-- Delete incorrect generated files: $deletedFilePath"

	if [ -f $deletedFilePath ]
		then rm $deletedFilePath
	fi
}

function CopyFiles {
	rsync -vizaP --delete -e "ssh -p $serverPort" ./Application/$1/bin/$buildFolderName/ $serverAddress:/opt/$1
}

function CopyFilesPublished {
	rsync -vizaP --delete -e "ssh -p $serverPort" WebApi/$1/bin/$buildFolderName/$2/publish/ $serverAddress:/opt/$1
}

function PublishProject {
    dotnet build "WebApi/$1" --configuration $buildFolderName
    dotnet publish "WebApi/$1" --configuration $buildFolderName
}

function UpdateEmailService {
	printf "\nОбновление службы отправки электронной почты\n"

	echo "-- Stoping $emailServiceName"
	ssh $serverAddress -p$serverPort sudo systemctl stop $emailServiceName	

	echo "-- Copying $emailServiceName files"
	DeleteHttpDll $emailServiceFolder
	CopyFiles $emailServiceFolder

	echo "-- Starting $emailServiceName"
	ssh $serverAddress -p$serverPort sudo systemctl start $emailServiceName
}

function UpdateSMSInformerService {
	printf "\nОбновление службы SMS информирования\n"

	echo "-- Stoping $smsServiceName"
	ssh $serverAddress -p$serverPort sudo systemctl stop $smsServiceName

	echo "-- Copying $smsServiceName files"
	DeleteHttpDll $smsServiceFolder
	CopyFiles $smsServiceFolder

	echo "-- Starting $smsServiceName"
	ssh $serverAddress -p$serverPort sudo systemctl start $smsServiceName
}

function UpdateSalesReceiptsService {
	printf "\nОбновление службы управления кассовым апаратом\n"

	echo "-- Stoping $kassaServiceName"
	ssh $serverAddress -p$serverPort sudo systemctl stop $kassaServiceName

	echo "-- Copying $kassaServiceName files"
	DeleteHttpDll $kassaServiceFolder
	CopyFiles $kassaServiceFolder

	echo "-- Starting $kassaServiceName"
	ssh $serverAddress -p$serverPort sudo systemctl start $kassaServiceName
}

function UpdateInstantSmsService {
	printf "\nОбновление службы моментальных SMS сообщений\n"

	echo "-- Stoping $instantSmsServiceName"
	ssh $serverAddress -p$serverPort sudo systemctl stop $instantSmsServiceName

	echo "-- Copying $instantSmsServiceName files"
	DeleteHttpDll $instantSmsServiceFolder
	CopyFiles $instantSmsServiceFolder

	echo "-- Starting $instantSmsServiceName"
	ssh $serverAddress -p$serverPort sudo systemctl start $instantSmsServiceName
}

function UpdateDeliveryRulesService {
	printf "\nОбновление службы правил доставки\n"

	echo "-- Stoping $deliveryRulesServiceName"
	ssh $serverAddress -p$serverPort sudo systemctl stop $deliveryRulesServiceName

	echo "-- Copying $deliveryRulesServiceName files"
	DeleteHttpDll $deliveryRulesServiceFolder
	CopyFiles $deliveryRulesServiceFolder

	echo "-- Starting $deliveryRulesServiceName"
	ssh $serverAddress -p$serverPort sudo systemctl start $deliveryRulesServiceName
}

function UpdateSmsPaymentService {
	printf "\nОбновление службы отправки платежей по sms\n"

	echo "-- Stoping $smsPaymentServiceName"
	ssh $serverAddress -p$serverPort sudo systemctl stop $smsPaymentServiceName

	echo "-- Copying $smsPaymentServiceName files"
	DeleteHttpDll $smsPaymentServiceFolder
	CopyFiles $smsPaymentServiceFolder

	echo "-- Starting $smsPaymentServiceName"
	ssh $serverAddress -p$serverPort sudo systemctl start $smsPaymentServiceName
}

function UpdateMangoService {
	printf "\nОбновление службы работы с Mango\n"

  	PublishProject $mangoServiceFolder

	ssh $serverAddress -p$serverPort sudo systemctl stop $mangoServiceName
	echo "-- Stoping $mangoServiceName"

	echo "-- Copying $mangoServiceName files"
	
	CopyFilesPublished $mangoServiceFolder "netcoreapp3.1"

	echo "-- Starting $mangoServiceName"
	ssh $serverAddress -p$serverPort sudo systemctl start $mangoServiceName
}

service2=",$service,"
case $service2 in
	*,2,*)
		UpdateEmailService
	;;&
	*,5,*)
		UpdateSMSInformerService
	;;&
	*,6,*)
		UpdateSalesReceiptsService
	;;&
	*,7,*)
		UpdateInstantSmsService
	;;&
	*,8,*)
		UpdateDeliveryRulesService
	;;&
	*,9,*)
		UpdateSmsPaymentService
	;;&
	*,10,*)
		UpdateMangoService
	;;
esac

read -p "Press enter to exit"

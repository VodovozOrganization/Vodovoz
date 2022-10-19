#!/bin/bash

echo "Какие службы необходимо обновить?"
echo "1) SmsInformer"
echo "2) ModulKassa (SalesReceipts)"
echo "3) InstantSms"
echo "4) DeliveryRules"
echo "5) SmsPayment"
echo "6) Mango"

echo "Можно вызывать вместе, перечислив номера через запятую, например SmsInformer+ModulKassa=1,2"
read service;

echo "Какую сборку использовать?"
echo "1) Release"
echo "2) Debug"
read build;

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
	DeleteHttpDll "Workers\Mono" $smsServiceFolder
	CopyFiles "Workers\Mono" $smsServiceFolder

	echo "-- Starting $smsServiceName"
	ssh $serverAddress -p$serverPort sudo systemctl start $smsServiceName
}

function UpdateSalesReceiptsService {
	printf "\nОбновление службы управления кассовым апаратом\n"

	echo "-- Stoping $kassaServiceName"
	ssh $serverAddress -p$serverPort sudo systemctl stop $kassaServiceName

	echo "-- Copying $kassaServiceName files"
	DeleteHttpDll "Workers\Mono" $kassaServiceFolder
	CopyFiles "Workers\Mono" $kassaServiceFolder

	echo "-- Starting $kassaServiceName"
	ssh $serverAddress -p$serverPort sudo systemctl start $kassaServiceName
}

function UpdateInstantSmsService {
	printf "\nОбновление службы моментальных SMS сообщений\n"

	echo "-- Stoping $instantSmsServiceName"
	ssh $serverAddress -p$serverPort sudo systemctl stop $instantSmsServiceName

	echo "-- Copying $instantSmsServiceName files"
	DeleteHttpDll "WCF" $instantSmsServiceFolder
	CopyFiles "WCF" $instantSmsServiceFolder

	echo "-- Starting $instantSmsServiceName"
	ssh $serverAddress -p$serverPort sudo systemctl start $instantSmsServiceName
}

function UpdateDeliveryRulesService {
	printf "\nОбновление службы правил доставки\n"

	echo "-- Stoping $deliveryRulesServiceName"
	ssh $serverAddress -p$serverPort sudo systemctl stop $deliveryRulesServiceName

	echo "-- Copying $deliveryRulesServiceName files"
	DeleteHttpDll "WCF" $deliveryRulesServiceFolder
	CopyFiles "WCF" $deliveryRulesServiceFolder

	echo "-- Starting $deliveryRulesServiceName"
	ssh $serverAddress -p$serverPort sudo systemctl start $deliveryRulesServiceName
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
		UpdateSalesReceiptsService
	;;&
	*,3,*)
		UpdateInstantSmsService
	;;&
	*,4,*)
		UpdateDeliveryRulesService
	;;&
	*,5,*)
		UpdateSmsPaymentService
	;;&
	*,6,*)
		UpdateMangoService
	;;
esac

read -p "Press enter to exit"

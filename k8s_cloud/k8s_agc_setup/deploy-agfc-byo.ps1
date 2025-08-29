#!/bin/bash
#Set environment variables
$RESOURCE_GROUP = 'aks-agfc-byo-rg'
$RESOURCE_GROUP_Aks = 'ms_aks_demo'
$AKS_NAME = 'ms_aks'
$IDENTITY_RESOURCE_NAME = 'azure-alb-identity'
$AGFC_NAME = 'alb-byo' #Name of the Application Gateway for Containers resource to be created
$FRONTEND_NAME = 'frontend-byo'
$ASSOCIATION_NAME = 'association-byo'
$VNET_RESOURCE_GROUP = 'MC_ms_aks_demo_ms_aks_eastus'
$VNET_NAME = 'aks-vnet-76646707'

#Create the Application Gateway for Containers
az network alb create -g $RESOURCE_GROUP -n $AGFC_NAME

#Create a Frontend
az network alb frontend create -g $RESOURCE_GROUP -n $FRONTEND_NAME --alb-name $AGFC_NAME

#Create a subnet for the Application Gateway for Containers within the AKS VNET
$MC_RESOURCE_GROUP = $(az aks show --name $AKS_NAME --resource-group $RESOURCE_GROUP_Aks --query "nodeResourceGroup" -o tsv)
$CLUSTER_SUBNET_ID = $(az vmss list --resource-group $MC_RESOURCE_GROUP --query '[0].virtualMachineProfile.networkProfile.networkInterfaceConfigurations[0].ipConfigurations[0].subnet.id' -o tsv)
#read -d '' VNET_NAME VNET_RESOURCE_GROUP VNET_ID <<< $(az network vnet show --ids $CLUSTER_SUBNET_ID --query '[name, resourceGroup, id]' -o tsv)

$SUBNET_ADDRESS_PREFIX = '10.225.0.0/24'
$ALB_SUBNET_NAME = 'subnet-alb' #The subnet name can be any non-reserved subnet name (i.e. GatewaySubnet, AzureFirewallSubnet, AzureBastionSubnet would all be invalid)

az network vnet subnet create --resource-group $VNET_RESOURCE_GROUP --vnet-name $VNET_NAME --name $ALB_SUBNET_NAME --address-prefixes $SUBNET_ADDRESS_PREFIX --delegations 'Microsoft.ServiceNetworking/trafficControllers'

$ALB_SUBNET_ID = $(az network vnet subnet show --name $ALB_SUBNET_NAME --resource-group $VNET_RESOURCE_GROUP --vnet-name $VNET_NAME --query '[id]' --output tsv)

$ResourceGroupId = $(az group show --name $RESOURCE_GROUP --query id -otsv)
$principalId = $(az identity show -g $RESOURCE_GROUP_Aks -n $IDENTITY_RESOURCE_NAME --query principalId -otsv)

#Delegate AppGw for Containers Configuration Manager role to AKS/Application Gateway for Containers RG
az role assignment create --assignee-object-id $principalId --assignee-principal-type ServicePrincipal --scope $ResourceGroupId --role "AppGw for Containers Configuration Manager" 

#Delegate Network Contributor permission for join to association subnet
az role assignment create --assignee-object-id $principalId --assignee-principal-type ServicePrincipal --scope $ALB_SUBNET_ID --role "Network Contributor"

#Create the Association
az network alb association create -g $RESOURCE_GROUP -n $ASSOCIATION_NAME --alb-name $AGFC_NAME --subnet $ALB_SUBNET_ID
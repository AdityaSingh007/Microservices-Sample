# Sign in to your Azure subscription.
$SUBSCRIPTION_ID = '22ef334e-0063-40f0-9a4a-8d5fa092e6f5'
az login
az account set --subscription $SUBSCRIPTION_ID

# Register required resource providers on Azure.
az provider register --namespace Microsoft.ContainerService
az provider register --namespace Microsoft.Network
az provider register --namespace Microsoft.NetworkFunction
az provider register --namespace Microsoft.ServiceNetworking

# Install Azure CLI extensions.
az extension add --name alb

$AKS_NAME = 'ms_aks'
$RESOURCE_GROUP = 'ms_aks_demo'
az aks update -g $RESOURCE_GROUP -n $AKS_NAME --enable-oidc-issuer --enable-workload-identity --no-wait

$RESOURCE_GROUP = 'ms_aks_demo'
$AKS_NAME = 'ms_aks'
$IDENTITY_RESOURCE_NAME = 'azure-alb-identity'

$mcResourceGroup = $(az aks show --resource-group $RESOURCE_GROUP --name $AKS_NAME --query "nodeResourceGroup" -o tsv)
$mcResourceGroupId = $(az group show --name $mcResourceGroup --query id -otsv)

echo "Creating identity $IDENTITY_RESOURCE_NAME in resource group $RESOURCE_GROUP"
az identity create --resource-group $RESOURCE_GROUP --name $IDENTITY_RESOURCE_NAME
$principalId = "$(az identity show -g $RESOURCE_GROUP -n $IDENTITY_RESOURCE_NAME --query principalId -otsv)"

echo "Apply Reader role to the AKS managed cluster resource group for the newly provisioned identity"
az role assignment create --assignee-object-id $principalId --assignee-principal-type ServicePrincipal --scope $mcResourceGroupId --role "acdd72a7-3385-48ef-bd42-f606fba81ae7" # Reader role

echo "Set up federation with AKS OIDC issuer"
$AKS_OIDC_ISSUER = "$(az aks show -n "$AKS_NAME" -g "$RESOURCE_GROUP" --query "oidcIssuerProfile.issuerUrl" -o tsv)"
az identity federated-credential create --name "azure-alb-identity" --identity-name "$IDENTITY_RESOURCE_NAME"  --resource-group $RESOURCE_GROUP  --issuer "$AKS_OIDC_ISSUER"  --subject "system:serviceaccount:azure-alb-system:alb-controller-sa"

$HELM_NAMESPACE = 'default'
$CONTROLLER_NAMESPACE = 'azure-alb-system'
az aks get-credentials --resource-group $RESOURCE_GROUP --name $AKS_NAME
helm install alb-controller oci://mcr.microsoft.com/application-lb/charts/alb-controller --namespace $HELM_NAMESPACE --version 1.7.9 --set albController.namespace=$CONTROLLER_NAMESPACE --set albController.podIdentity.clientID=$(az identity show -g $RESOURCE_GROUP -n azure-alb-identity --query clientId -o tsv)
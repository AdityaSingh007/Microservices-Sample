**Cluster deployment instructions for on-prem**

**=================================================================================**

**Install ECK operator**

### kubectl create -f https://download.elastic.co/downloads/eck/3.1.0/crds.yaml

### kubectl apply -f https://download.elastic.co/downloads/eck/3.1.0/operator.yaml

**Verify if eck pod is running**

### kubectl get pods -n elastic-system

**Delete default elastic admin secret**

### kubectl delete secret elasticsearch-development-es-elastic-user -n elastic-system

**Install gloo api gateway**

## kubectl apply -f https://github.com/kubernetes-sigs/gateway-api/releases/download/v1.2.1/standard-install.yaml

## helm repo add gloo https://storage.googleapis.com/solo-public-helm

## helm repo update

## helm install -n gloo-system gloo gloo/gloo --create-namespace --version 1.20.1 -f gloo-gateway-values.yaml

**Verify gloo api gateway installation**

## kubectl get pods -n gloo-system | grep gloo

## kubectl get gatewayclass gloo-gateway

**Set up an API gateway**

## kubectl apply -f k8s_gateway/gateway-deployment.yaml

## kubectl get gateway ms-gateway -n gloo-system

**set up azure container registry secert**

### kubectl create secret docker-registry acr-secret --docker-server=microservicesprivate-cngmana9bbhsa5ck.azurecr.io --docker-username=msprivateregistry --docker-password={password from azure portal}

**Deploy secrets**

### kubectl apply -f k8s_secret

### kubectl create secret tls ms-tls-secret --cert=localhost.crt --key=localhost.key

**Deploy infrastructure**

### kubectl apply -f k8s_infrastructure

**Set up and deploy config map**

### get local ip address and replace in k8s_configmap/microservices-variables.yaml

### kubectl apply -f k8s_configmap

**Deploy backend**

### kubectl apply -f k8s_backend

**Verify if all pods are running**

### kubectl get pods --watch

**Verify if gateway-api is accessible and all services ae healthy**

### https://localhost:8081/hc-ui#/healthchecks

**Deploy frontend**

### cd frontend

### kubectl apply -f k8s_onprem

### Access frontend at - https://localhost:4200

**Deploy gateway upstream and http-route**

## kubectl apply -f k8s_gateway/http-upstream.yaml

## kubectl apply -f k8s_gateway/http-route-deployment.yaml

## kubectl apply -f k8s_gateway/http-route-frontend-deployment.yaml

**===============================================================================**

**Cluster deployment instructions for on-prem**

**=================================================================================**

**Install ECK operator**

### kubectl create -f https://download.elastic.co/downloads/eck/3.1.0/crds.yaml

### kubectl apply -f https://download.elastic.co/downloads/eck/3.1.0/operator.yaml

**Verify if eck pod is running**

### kubectl get pods -n elastic-system

**Delete default elastic admin secret**

### kubectl delete secret elasticsearch-development-es-elastic-user -n elastic-system

**Deploy secrets**

### kubectl apply -f k8s_secret

### kubectl create secret tls ms-tls-secret --cert=localhost.crt --key=localhost.key

**set up docker container registry secert**

### kubectl create secret docker-registry acr-secret --docker-server=adityasingh1991/microservices_onprem --docker-username=adityasingh1991 --docker-password={password from docker portal}

**Set up and deploy config map**

### get local ip address and replace in k8s_configmap/microservices-variables.yaml

### kubectl apply -f k8s_configmap

**Deploy storage**

### Create a folder kubernetes_mount in C: and copy the contents from folder kubernetes_mount in solution

### Run kubectl apply -f k8s_storage

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

**Deploy infrastructure**

### kubectl apply -f k8s_infrastructure

**Deploy backend**

### kubectl apply -f k8s_backend

**Verify if all pods are running**

### kubectl get pods --watch

### Additionaly verify if elastic search - https://localhost:9200 and kibana - https://localhost:5601 is also accessible

**Deploy frontend**

### cd frontend

### kubectl apply -f k8s_onprem

**Deploy gateway upstream and http-route**

## kubectl apply -f k8s_gateway/http-upstream.yaml

## kubectl apply -f k8s_gateway/http-route-deployment.yaml

## kubectl apply -f k8s_gateway/http-route-frontend-deployment.yaml

**Verify if gateway-api is accessible and all services ae healthy**

### https://localhost/hc-ui#/healthchecks

**Access front-end**

### Access frontend at - https://localhost/mf-shell

**===============================================================================**

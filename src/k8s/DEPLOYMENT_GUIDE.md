# Learnify OpenShift Deployment Guide

This guide describes how to deploy the Learnify microservices architecture to an OpenShift cluster.

## 1. Project Structure
The following files have been created in your workspace:
- **Dockerfiles**: Located in each microservice directory (e.g., `Learnify.Identity.API/Dockerfile`).
- **k8s/secret-route.yaml**: Contains sensitive configurations (JWT Key, DB Strings) and the external Route for the Gateway.
- **k8s/deployment.yaml**: Contains Deployment and Service definitions for all 9 services.

## 2. Build and Push Images
You need to build the images and push them to a registry (like OpenShift's internal registry or Quay.io).

Example for the Identity service:
```bash
# From the root directory (src)
docker build -t quay.io/youruser/identity-service:latest -f Learnify.Identity.API/Dockerfile .
docker push quay.io/youruser/identity-service:latest
```
*Note: Update the image names in `k8s/deployment.yaml` to match your registry paths before applying.*

## 3. Deployment Steps

### Step A: Create Secrets and Routes
```bash
oc apply -f k8s/secret-route.yaml
```

### Step B: Deploy Services
```bash
oc apply -f k8s/deployment.yaml
```

## 4. Configuration Overrides
The `deployment.yaml` uses environment variables to override the `localhost` settings in `appsettings.json`:
- **Gateway**: Routes traffic to internal service names (e.g., `http://identity-service:8080`).
- **Secrets**: Database connection strings and JWT keys are pulled from the `learnify-secrets` secret.

## 5. Accessing the Application
Once deployed, find the external URL for the API Gateway:
```bash
oc get route learnify-gateway
```

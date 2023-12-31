templateFile="azuredeploy.json"
stagingParameterFile="azuredeploy.parameters.staging.json"

# Create group if needed
az group create --name ExampleGroup --location "East US"

# Create deployment
az deployment group create \
  --name StagingDeployment \
  --resource-group ExampleGroup \
  --template-file $templateFile \
  --parameters $devParameterFile

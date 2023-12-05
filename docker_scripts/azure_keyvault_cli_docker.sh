#Open a terminal docker Azure CLI
#Manage Azure CLI at https://learn.microsoft.com/en-us/azure/key-vault/general/manage-with-cli2

#Login to Azure account
az login

#list subscription
az account list

#list resource groups and key vault
az group list
az keyvault list

#Create a directory in the azure cli container to store secrets to transfer
docker exec -it intelligent_buck mkdir /user-secrets

#open a terminal in the user secrets directory
#copy secrets.json to the azure cli container
docker cp secrets.json intelligent_buck:/user-secrets/


#create an Azure Key Vault Secret in the azure keyvault and load the file secrets.json into it
az keyvault secret set --name "user-secrets1" --vault-name "full-stack-example-kv" --file "/user-secrets/secrets.json"

#remove the secrets.json in the /user-secrets/ directory
#in Azure cli containerls
rm /user-secrets/secrets.json


###to setup an application to access Azure keyvault

###1. Register the application
#IN Pane App registrations: Name and register the application
#Now you get:
#- ClientID: fe0b7da8-b3b8-47f1-a36a-fd0d2a5dca73
#- TentantID: 1572fbad-267f-4fa0-82b2-2a5de30ac664

###2. Create an access certificate for the application
#IN Pane App registrations->name-of-registstered-app ->Certificates and Secrets: Create New Client Secret
#Now you get:
#- Client Secret: .nr8Q~yzd.l0yDWi5YvvYcW.T6GM2jK~~3vbtb9y


###3. Add registered app for the access policy of azure keyvault
#IN Pane full-stack-example-kv KeyVault -> Access policies  
#Create an Access policy with required permissions,
#Pricipal should be the name of the registered app
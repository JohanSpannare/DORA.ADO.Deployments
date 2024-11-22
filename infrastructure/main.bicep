param location string = resourceGroup().location
param environment string = resourceGroup().tags.environment

param namePrefix string = 'ado'
param vnetName string = 'vnet-weu-ado-server-non-prod'
param subnetName string = 'sb-weu-ado-server-db01'
param vnetResourceGroup string = 'rg-weu-nw-ado-server-nonprod'
param keyVaultName string = 'kv-ado-server-nonprod'
param dnsZoneSubscriptionId string = '079c18fd-b130-4cc7-b3f3-8582636bccef'
param dnsZoneResourceGroupName string = 'rg_weu-pvt-dns'

resource keyVault 'Microsoft.KeyVault/vaults@2019-09-01' existing = {
  name: keyVaultName
}

module applicationInsights 'applicationInsights.bicep' = {
  name: 'applicationInsights'
  params: {
    environment: environment
    location: location
  }
}

resource keyVaultSecret 'Microsoft.KeyVault/vaults/secrets@2019-09-01' = {
  parent: keyVault
  name: 'ApplicationInsights--ConnectionString'
  properties: {
    value: applicationInsights.outputs.ConnectionString
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2020-12-01' = {
  name: '${environment}-appServicePlan'
  location: location
  sku: {
    name: 'S1'
    capacity: 1
  }
  properties: {
    reserved: false // Linux=true, Windows=false
  }
}

resource webApplication 'Microsoft.Web/sites@2022-09-01' = {
  name: '${namePrefix}-webapp-${environment}'
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    // TODO: virtualNetworkSubnetId: used for outbound communications
  }
}

module privateEndpoint 'privateEndpoint.bicep' = {
  name: '${webApplication.name}-privateEndpoint'
  params: {
    location: location
    subnetName: subnetName
    vnetName: vnetName
    vnetResourceGroup: vnetResourceGroup
    webApplicationId: webApplication.id
    webApplicationName: webApplication.name
    groupId: 'sites'
    dnsZoneSubscriptionId: dnsZoneSubscriptionId
    dnsZoneResourceGroupName: dnsZoneResourceGroupName
  }
}

resource webApplicationStage 'Microsoft.Web/sites/slots@2022-09-01' = {
  name: 'stage'
  location: location
  parent: webApplication
  kind: 'app'
  properties: {
    serverFarmId: appServicePlan.id
  }
}

module privateEndpointStage 'privateEndpoint.bicep' = {
  name: '${webApplicationStage.name}-privateEndpoint'
  params: {
    location: location
    subnetName: subnetName
    vnetName: vnetName
    vnetResourceGroup: vnetResourceGroup
    webApplicationId: webApplication.id
    webApplicationName: '${webApplicationStage.properties.repositorySiteName}-${webApplicationStage.name}'
    groupId: 'sites-stage'
    dnsZoneSubscriptionId: dnsZoneSubscriptionId
    dnsZoneResourceGroupName: dnsZoneResourceGroupName
  }
}


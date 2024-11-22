param location string = resourceGroup().location


param vnetName string
param subnetName string
param vnetResourceGroup string
param webApplicationId string
param webApplicationName string
param dnsZoneSubscriptionId string
param dnsZoneResourceGroupName string
param groupId string = 'sites'

module subnet 'subnet.bicep' = {
  name: '${webApplicationName}-subnet'
  params: {
    subnetName: subnetName
    vnetName: vnetName
    vnetResourceGroup: vnetResourceGroup
  }

}



resource privateEndpoint 'Microsoft.Network/privateEndpoints@2022-01-01' = {
  name: '${webApplicationName}-privateEndpoint'
  location: location
  properties: {
    privateLinkServiceConnections: [
      {
        name: '${webApplicationName}-privateEndpoint'
        properties: {
          privateLinkServiceId: webApplicationId
          groupIds: [groupId]
        }
      }
    ]
    subnet: {
      id: subnet.outputs.subnetId
    }

  }
}

resource privateLink 'Microsoft.Network/privateDnsZones@2020-06-01' existing = {
  name: 'privatelink.azurewebsites.net'
  scope: resourceGroup(dnsZoneSubscriptionId, dnsZoneResourceGroupName)
}

resource privateLinkSCM 'Microsoft.Network/privateDnsZones@2020-06-01' existing = {
  name: 'scm.privatelink.azurewebsites.net'
  scope: resourceGroup(dnsZoneSubscriptionId, dnsZoneResourceGroupName)
}

resource pvtEndpointDnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  parent: privateEndpoint
  name: '${webApplicationName}-default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privateLink'
        properties: {
          privateDnsZoneId: privateLink.id
        }
      }
      {
        name: 'privateLinkSCM'
        properties: {
          privateDnsZoneId: privateLinkSCM.id
          
        }
      }
    ]
  }
}

module privateEndpointNic 'networkInterfaces.bicep' ={
  name: '${webApplicationName}-nested'
  params:{
    nicName: last(split(privateEndpoint.properties.networkInterfaces[0].id, '/'))
  }
}

module dnsRecord 'dnsRecord.bicep' = {
  name: '${webApplicationName}-scm-record'

  scope: resourceGroup(dnsZoneSubscriptionId, dnsZoneResourceGroupName)
  params: {
    // webApplicationIP: webApplicationIP
    webApplicationIP: privateEndpointNic.outputs.ip
    webApplicationName: webApplicationName

  }
}

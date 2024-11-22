param vnetName string
param subnetName string
param vnetResourceGroup string

resource VNET 'Microsoft.Network/virtualNetworks@2022-09-01' existing = {
  name: vnetName
  scope: resourceGroup(vnetResourceGroup)
}

output subnetId string = filter(VNET.properties.subnets, s => s.name == subnetName)[0].id

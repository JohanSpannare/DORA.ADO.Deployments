param webApplicationName string
param webApplicationIP string

resource dnsZone 'Microsoft.Network/privateDnsZones@2018-09-01' existing = {
  name: 'scm.privatelink.azurewebsites.net'
}

resource dnsRecord 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: dnsZone
  name: webApplicationName
  properties: {
    ttl: 5
    aRecords: [{
      ipv4Address: webApplicationIP
    }]
  }
}

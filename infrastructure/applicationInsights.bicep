param location string
param environment string

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2020-10-01' = {
  name: '${environment}-LogWorkspace'
  location: location
}

resource appInsightsComponents 'Microsoft.Insights/components@2020-02-02' = {
  name: '${environment}-ApplicationInsights'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

output ConnectionString string = appInsightsComponents.properties.ConnectionString

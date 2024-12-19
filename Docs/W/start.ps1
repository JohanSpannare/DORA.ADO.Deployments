if (-not (Test-Path Env:AZP_URL)) {
  Write-Error "error: missing AZP_URL environment variable"
  exit 1
}

if (-not (Test-Path Env:AZP_TOKEN_FILE)) {
  if (-not (Test-Path Env:AZP_TOKEN)) {
    Write-Error "error: missing AZP_TOKEN environment variable"
    exit 1
  }

  $Env:AZP_TOKEN_FILE = "\azp\.token"
  $Env:AZP_TOKEN | Out-File -FilePath $Env:AZP_TOKEN_FILE
}

Remove-Item Env:AZP_TOKEN

if ((Test-Path Env:AZP_WORK) -and -not (Test-Path $Env:AZP_WORK)) {
  New-Item $Env:AZP_WORK -ItemType directory | Out-Null
}


Set-Location /azp/agent

$env:VSO_AGENT_IGNORE="VSO_AGENT_IGNORE,AZP_TOKEN,AZP_TOKEN_FILE,PROXYURL,PROXYUSERNAME,PROXYPASSWORD,AZP_AGENT_NAME,AZP_POOL,AZP_URL"

try
{
  Write-Host "Configuring Azure Pipelines agent..." -ForegroundColor Cyan

  .\config.cmd --unattended `
    --agent "$(if (Test-Path Env:AZP_AGENT_NAME) { ${Env:AZP_AGENT_NAME} } else { hostname })" `
    --url "$(${Env:AZP_URL})" `
    --auth PAT `
    --token "$(Get-Content ${Env:AZP_TOKEN_FILE})" `
    --pool "$(if (Test-Path Env:AZP_POOL) { ${Env:AZP_POOL} } else { 'Default' })" `
    --work "$(if (Test-Path Env:AZP_WORK) { ${Env:AZP_WORK} } else { '_work' })" `
    --replace `
    --gituseschannel

  Write-Host "4. Running Azure Pipelines agent..." -ForegroundColor Cyan

  $processId = (start-process -NoNewWindow -PassThru -FilePath .\run.cmd).Id
  Write-Host "Started process with ID $processId"
  Wait-Process -Id $processId
}
finally
{
  try {
    Write-Host "Cleanup. Removing Azure Pipelines agent..."
    Stop-Process $processId -Force
    .\config.cmd remove --unattended `
      --auth PAT `
      --token "$(Get-Content ${Env:AZP_TOKEN_FILE})"
  }
  catch {
    Write-Host "Finally Error"
    Write-Host $_
    Write-Host $_.ScriptStackTrace
  }
  
}
ARG REGISTRY=artifactory.adtr.th.tryg.com:443/mcr.microsoft.remote
FROM ${REGISTRY}/dotnet/framework/runtime:4.8-windowsservercore-ltsc2019 as base

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'Continue'; $verbosePreference='Continue';"]

# Install trygg-hansa certificates to be able to communicate with other servers and internet
COPY ["RSA Group Root CA 01 v1.pem","."]
COPY ["RSA Group Issuing CA 01 v1.pem","."]
COPY ["trygghansaproxy.pem","."]
COPY ["cert_SSL Inspection MMA1.pem","."]
COPY ["cert_SSL Inspection MMA2.pem","."]
COPY ["cert_SSL Inspection MMA1_2023.crt","."]
COPY ["cert_SSL Inspection MMA2_2023.crt","."]
COPY ["THIssuingCA.cer","."]
COPY ["THRootCA.cer","."]

RUN Import-Certificate -FilePath '.\cert_SSL Inspection MMA1_2023.crt' -CertStoreLocation Cert:\LocalMachine\Root
RUN Import-Certificate -FilePath '.\cert_SSL Inspection MMA2_2023.crt' -CertStoreLocation Cert:\LocalMachine\Root
RUN Import-Certificate -FilePath '.\RSA Group Root CA 01 v1.pem' -CertStoreLocation Cert:\LocalMachine\Root
RUN Import-Certificate -FilePath '.\RSA Group Issuing CA 01 v1.pem' -CertStoreLocation Cert:\LocalMachine\Root
RUN Import-Certificate -FilePath '.\RSA Group Issuing CA 01 v1.pem' -CertStoreLocation Cert:\LocalMachine\CA
RUN Import-Certificate -FilePath '.\trygghansaproxy.pem' -CertStoreLocation Cert:\LocalMachine\Root
RUN Import-Certificate -FilePath '.\cert_SSL Inspection MMA1.pem' -CertStoreLocation Cert:\LocalMachine\Root
RUN Import-Certificate -FilePath '.\cert_SSL Inspection MMA2.pem' -CertStoreLocation Cert:\LocalMachine\Root
RUN Import-Certificate -FilePath '.\THIssuingCA.cer' -CertStoreLocation Cert:\LocalMachine\Root
RUN Import-Certificate -FilePath '.\THIssuingCA.cer' -CertStoreLocation Cert:\LocalMachine\CA
RUN Import-Certificate -FilePath '.\THRootCA.cer' -CertStoreLocation Cert:\LocalMachine\Root

WORKDIR /TEMP

SHELL ["cmd","/S","/C"]

# Visual Studio 2019 Build Tools
RUN curl -SL --ssl-no-revoke --output vs_Enterprise-2019.exe https://aka.ms/vs/16/release/vs_Enterprise.exe
RUN start /w vs_Enterprise-2019.exe \
    --quiet \
    --wait \
    --norestart \
    --nocache \     
    --add Microsoft.VisualStudio.Workload.Office \
    || IF "%ERRORLEVEL%"=="3010" EXIT 0

RUN powershell -command "if(-NOT (test-path -path 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Microsoft\VisualStudio\v16.0\SharePointTools')){write-error 'SharePoint Tools missing'}"

# Visual Studio 2022 Build Tools
RUN curl -SL --ssl-no-revoke --output vs_Enterprise-2022.exe https://aka.ms/vs/17/release/vs_Enterprise.exe
RUN start /w vs_Enterprise-2022.exe \
    --quiet \
    --wait \
    --norestart \
    --nocache \     
    --allWorkloads \
    --includeRecommended \
    --includeOptional \
    || IF "%ERRORLEVEL%"=="3010" EXIT 0

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'Continue'; $verbosePreference='Continue';"]

# Install Azure DevOps Agent
RUN Invoke-WebRequest https://vstsagentpackage.azureedge.net/agent/3.243.0/vsts-agent-win-x64-3.243.0.zip -OutFile vsts-agent-win-x64.zip
RUN Expand-Archive -Path "vsts-agent-win-x64.zip" -DestinationPath "\azp\agent"

# Install .NET SDKS
RUN Invoke-WebRequest https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.ps1 -OutFile dotnet-install.ps1; \
    .\dotnet-install.ps1 -Channel 6.0 -InstallDir 'C:\Program Files\dotnet\'; \
    .\dotnet-install.ps1 -Channel 8.0 -InstallDir 'C:\Program Files\dotnet\'; \
    .\dotnet-install.ps1 -Channel 9.0 -InstallDir 'C:\Program Files\dotnet\';

# Verify SDKS´s Installed
RUN dotnet --info; \
    dotnet sdk check; \
    if(-NOT (((dotnet --list-sdks) | %{$_ -match '6.0.'}) -contains $true)){write-error 'Missing SDK 6.0.*'}; \
    if(-NOT (((dotnet --list-sdks) | %{$_ -match '8.0.'}) -contains $true)){write-error 'Missing SDK 8.0.*'}; \
    if(-NOT (((dotnet --list-sdks) | %{$_ -match '9.0.'}) -contains $true)){write-error 'Missing SDK 9.0.*'};

# Install Chocolatey
RUN Invoke-WebRequest https://community.chocolatey.org/install.ps1 -OutFile install.ps1
ARG chocolateyProxyLocation chocolateyProxyUser chocolateyProxyPassword
RUN [Net.WebRequest]::DefaultWebProxy.Credentials = [Net.CredentialCache]::DefaultCredentials; \
    Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; .\install.ps1;
RUN choco source add --name=artifactory --source=https://artifactory.adtr.th.tryg.com/artifactory/api/nuget/chocolatey-remote
RUN choco source disable --name chocolatey

# Install tools for builds through Chocolatey
RUN choco install --stoponfirstfailure nuget.commandline -y;
RUN choco install --stoponfirstfailure nodejs-lts -y;
RUN choco install --stoponfirstfailure python --ignoredependencies -y;
RUN choco install --stoponfirstfailure visualstudio2017-workload-webbuildtools -y;
RUN choco install --stoponfirstfailure openjdk openjdk11jre -y;
RUN choco install --stoponfirstfailure docker-cli -y;
RUN choco install --stoponfirstfailure visualstudio2022testagent --ignoredependencies -y;
RUN choco install --stoponfirstfailure git.portable --ignoredependencies -y;
RUN choco install --stoponfirstfailure kubernetes-cli --ignoredependencies -y;
RUN choco install --stoponfirstfailure kubernetes-helm --version=3.3.1 --ignoredependencies -y;
RUN choco install --stoponfirstfailure azure-cli -y;

#Install .NET Framework 3.5 (includes .NET 2.0 and 3.0) NFOS depends on 'Microsoft.VisualBasic, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
RUN Set-Service -Name wuauserv -StartupType Manual; Install-WindowsFeature -Name NET-Framework-Features -Verbose


# Install codeql
ENV AGENT_TOOLSDIRECTORY=C:\\azp\\agent\\_work\\_tool
RUN Invoke-WebRequest https://raw.githubusercontent.com/microsoft/GHAzDO-Resources/refs/heads/main/src/agent-setup/codeql-install-windows.ps1 -OutFile codeql-install-windows.ps1; \
    .\codeql-install-windows.ps1;

# Cleanup after installs
WORKDIR /
RUN Remove-Item C:\TEMP -Recurse;

# Configure GIT to use Windows Certificate Store
COPY .gitconfig.config /Users/ContainerAdministrator/.gitconfig

# Configure NuGet.Config to use Artifactory and not nuget.org
COPY ["Nuget.Config.config","/Program Files (x86)/NuGet/Config/NuGet.Config"]
RUN Add-Content -Path C:\Windows\system32\drivers\etc\hosts -Value '0.0.0.0 api.nuget.org'
RUN Add-Content -Path C:\Windows\system32\drivers\etc\hosts -Value '0.0.0.0 www.nuget.org'

# Configure NPM default registry
RUN Add-Content -Path C:\Windows\system32\drivers\etc\hosts -Value '0.0.0.0 registry.npmjs.org'
RUN npm config set registry=https://artifactory.adtr.th.tryg.com/artifactory/api/npm/npm.remote/ --location=global; \
    npm config list

# Generate one cert file containing all certs
RUN Get-Content '.\RSA Group Issuing CA 01 v1.pem', '.\RSA Group Root CA 01 v1.pem', .\trygghansaproxy.pem, '.\cert_SSL Inspection MMA1.pem','.\cert_SSL Inspection MMA2.pem','.\cert_SSL Inspection MMA1_2023.crt','.\cert_SSL Inspection MMA2_2023.crt','.\THIssuingCA.cer','.\THRootCA.cer' | Set-Content certs.pem

# Configure Azure CLI to trust RSA Certificates
RUN Get-Content C:\certs.pem | Add-Content 'C:\Program Files\Microsoft SDKs\Azure\CLI2\Lib\site-packages\certifi\cacert.pem'

# Configure JAVA to trust RSA Certificates
RUN & """$($env:JAVA_HOME)\bin\keytool.exe""" -import -trustcacerts -keystore """$($env:JAVA_HOME)\lib\security\cacerts""" -noprompt -alias RSA-Root -file c:\certs.pem
RUN & """$($env:JAVA_HOME)\bin\keytool.exe""" -import -trustcacerts -keystore """$($env:JAVA_HOME)\lib\security\cacerts""" -noprompt -alias RSA-Root-TH -file c:\THRootCA.cer
RUN & """$($env:JAVA_HOME)\bin\keytool.exe""" -import -trustcacerts -keystore """$($env:JAVA_HOME)\lib\security\cacerts""" -noprompt -alias RSA-Root-THI -file c:\THIssuingCA.cer

# Configure NODE to use the all cert file as fallback
ENV NODE_EXTRA_CA_CERTS=c://certs.pem

# Fix missing node in latest azure buildagent
RUN copy /azp/agent/externals/node10 /azp/agent/externals/node -Recurse

# opt-out of telemetry
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# Copy & run Azure Pipelines Agent startup script
SHELL ["cmd","/S","/C"]
WORKDIR /azp
COPY start.ps1 .
CMD powershell .\start.ps1
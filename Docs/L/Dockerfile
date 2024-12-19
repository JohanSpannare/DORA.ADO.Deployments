FROM ubuntu:20.04
RUN DEBIAN_FRONTEND=noninteractive apt-get update
RUN DEBIAN_FRONTEND=noninteractive apt-get upgrade -y

RUN DEBIAN_FRONTEND=noninteractive apt-get install -y -qq --no-install-recommends \
    apt-transport-https \
    apt-utils \
    ca-certificates \
    curl \
    git \
    iputils-ping \
    jq \
    lsb-release \
    wget \
    software-properties-common

RUN curl -sL https://aka.ms/InstallAzureCLIDeb | bash

# Can be 'linux-x64', 'linux-arm64', 'linux-arm', 'rhel.6-x64'.
ENV TARGETARCH=linux-x64

WORKDIR /azp

# Install Certificates
COPY  ["RSA Group Root CA 01 v1.crt","/usr/local/share/ca-certificates"] 
COPY  ["RSA Group Issuing CA 01 v1.crt","/usr/local/share/ca-certificates"]
COPY  ["trygghansaproxy.crt","/usr/local/share/ca-certificates"]
COPY  ["cert_SSL Inspection MMA1.crt","/usr/local/share/ca-certificates"]
COPY  ["cert_SSL Inspection MMA2.crt","/usr/local/share/ca-certificates"]
COPY  ["cert_SSL Inspection MMA1_2023.crt","/usr/local/share/ca-certificates"]
COPY  ["cert_SSL Inspection MMA2_2023.crt","/usr/local/share/ca-certificates"]
COPY  ["THIssuingCA.crt","/usr/local/share/ca-certificates"]
COPY  ["THRootCA.crt","/usr/local/share/ca-certificates"]

# Verify external and internal TLS
RUN update-ca-certificates && curl -v https://gitlab.gnome.org/GNOME/gnome-2048.git && curl -v https://ss00pazure.adtr.th.tryg.com/NordicCollection/

# Install Azure DevOps Agent
RUN wget -q https://vstsagentpackage.azureedge.net/agent/3.243.0/vsts-agent-linux-x64-3.243.0.tar.gz
RUN tar xzf ./vsts-agent-linux-x64-3.243.0.tar.gz

# Install donet-sdk
RUN wget -q https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb && \
    apt-get update
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# Disable GIT SSL since it not currently working on Ubuntu 20.04
RUN git config --global http.sslVerify false

# Install nodejs 18
RUN echo 'Acquire::https::deb.nodesource.com::Verify-Peer "false";' > /etc/apt/apt.conf.d/nodesource-cert
RUN curl -fsSL https://deb.nodesource.com/setup_18.x | bash - &&\
    apt-get install -y nodejs

# Install donet-sdk
RUN wget -q https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh && \
    chmod +x ./dotnet-install.sh && \
    ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet && \
    ./dotnet-install.sh --channel 6.0 --install-dir /usr/share/dotnet && \
    ./dotnet-install.sh --channel 8.0 --install-dir /usr/share/dotnet && \
    ./dotnet-install.sh --channel 9.0 --install-dir /usr/share/dotnet

ENV PATH="$PATH:/root/.dotnet/tools"

# Verify SDKS´s Installed
RUN /usr/share/dotnet/dotnet --info && \
    /usr/share/dotnet/dotnet sdk check

# Add kubernetes and helm source
RUN curl -fsSL https://pkgs.k8s.io/core:/stable:/v1.28/deb/Release.key | gpg --dearmor -o /etc/apt/trusted.gpg.d/kubernetes-apt-keyring.gpg && \
    echo 'deb [signed-by=/etc/apt/trusted.gpg.d/kubernetes-apt-keyring.gpg] https://pkgs.k8s.io/core:/stable:/v1.28/deb/ /' | tee /etc/apt/sources.list.d/kubernetes.list && \
    curl -fsSL https://baltocdn.com/helm/signing.asc | gpg --dearmor -o /etc/apt/trusted.gpg.d/helm.gpg && \
    echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/trusted.gpg.d/helm.gpg] https://baltocdn.com/helm/stable/debian/ all main" | tee /etc/apt/sources.list.d/helm-stable-debian.list && \
    apt-get update

# Install Build Tools
RUN DEBIAN_FRONTEND=noninteractive apt-get install -y -qq python python3 python3.8-venv python3.9-venv
RUN DEBIAN_FRONTEND=noninteractive apt-get install -y -qq moby-cli
RUN DEBIAN_FRONTEND=noninteractive apt-get install -y -qq openjdk-21-jdk
RUN DEBIAN_FRONTEND=noninteractive apt-get install -y -qq zip
RUN DEBIAN_FRONTEND=noninteractive apt-get install -y -qq build-essential
RUN DEBIAN_FRONTEND=noninteractive apt-get install -y -qq powershell
RUN DEBIAN_FRONTEND=noninteractive apt-get install -y -qq kubectl
RUN DEBIAN_FRONTEND=noninteractive apt-get install -y -qq helm=3.3.1-1
RUN DEBIAN_FRONTEND=noninteractive apt-get install -y -qq python-dev python3-dev unixodbc-dev python3.9-dev
RUN DEBIAN_FRONTEND=noninteractive apt-get install -y -qq openssh-server

#Install Python Azure Tools
RUN python3.8 -m venv x64 /azp/_work/_tool/Python/3.8.10/x64 && \
    ln -s /azp/_work/_tool/Python/3.8.10 /azp/_work/_tool/Python/3.8 && \
    touch /azp/_work/_tool/Python/3.8.10/x64.complete && \
    chmod +x /azp/_work/_tool/Python/3.8.10/x64.complete

RUN python3.9 -m venv x64 /azp/_work/_tool/Python/3.9.5/x64 && \
    ln -s /azp/_work/_tool/Python/3.9.5 /azp/_work/_tool/Python/3.9 && \
    touch /azp/_work/_tool/Python/3.9.5/x64.complete && \
    chmod +x /azp/_work/_tool/Python/3.9.5/x64.complete

#Install Azure-CLI
RUN curl -sL https://aka.ms/InstallAzureCLIDeb | bash
RUN cat /etc/ssl/certs/ca-certificates.crt >> /opt/az/lib/python3.12/site-packages/certifi/cacert.pem

#Install codeql
ENV AGENT_TOOLSDIRECTORY=/azp/_work/_tool
RUN wget -q https://raw.githubusercontent.com/microsoft/GHAzDO-Resources/refs/heads/main/src/agent-setup/codeql-install-ubuntu.sh -O codeql-install-linux.sh && \
    chmod +x ./codeql-install-linux.sh && \
    ./codeql-install-linux.sh

# Configure NODE CA
ENV NODE_EXTRA_CA_CERTS=/etc/ssl/certs/ca-certificates.crt

# Disable nuget.org and add nuget.remote as default repository
RUN dotnet nuget disable source nuget.org
RUN mkdir --parents /etc/opt/NuGet/Config/ && mv ~/.nuget/NuGet/NuGet.Config /etc/opt/NuGet/Config/NuGet.Config
RUN dotnet nuget add source https://artifactory.adtr.th.tryg.com/artifactory/api/nuget/nuget.remote/ -n nuget.remote --configfile /etc/opt/NuGet/Config/NuGet.Config
RUN dotnet nuget list source

# Configure NPM default registry
RUN npm config set registry=https://artifactory.adtr.th.tryg.com/artifactory/api/npm/npm.remote/ --location=global &&\
    npm config list

COPY ./.proxybypass .
COPY ./start.sh .
RUN chmod +x start.sh
ENTRYPOINT [ "./start.sh" ]

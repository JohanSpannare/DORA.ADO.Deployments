# Introduction 
Linux build agent for Azure DevOps containerized on Kubernetes

## Build
```
docker build `
    -t azure-build-agent-linux:latest `
    -f Dockerfile `
    .
```

## Run
```
docker run `
-v /var/run/docker.sock:/var/run/docker.sock `
-e AZP_URL=https://ss00pazure.adtr.th.tryg.com/NordicCollection/ `
-e AZP_TOKEN=<TOKEN> `
-e AZP_AGENT_NAME=azure-build-agent-linux `
-e AZP_POOL=TestAgents `
-e PROXYUSERNAME=<USERNAME>@ADEP01.NORDIC.RSA-INS.COM `
-e PROXYPASSWORD=<PASSWORD> `
-e PROXYURL=http://seproxy-a.adep01.nordic.rsa-ins.com:8080 `
azure-build-agent-linux:latest
```

## Debug
```
docker run `
-it --rm --entrypoint bash `
-v /var/run/docker.sock:/var/run/docker.sock `
-e AZP_URL=https://ss00pazure.adtr.th.tryg.com/NordicCollection/ `
-e AZP_TOKEN=<TOKEN> `
-e AZP_AGENT_NAME=azure-build-agent-linux `
-e AZP_POOL=TestAgents `
-e PROXYUSERNAME=<USERNAME>@ADEP01.NORDIC.RSA-INS.COM `
-e PROXYPASSWORD=<PASSWORD> `
-e PROXYURL=http://seproxy-a.adep01.nordic.rsa-ins.com:8080 `
azure-build-agent-linux:latest
```

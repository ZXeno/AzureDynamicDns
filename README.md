# Azure Dynamic DNS V2

If you've ever needed a dynamic DNS host, but didn't feel like paying the $5-$10 USD (or more!) as a dynamic DNS host, this solution will get you up-and-running in no-time. Especially since running an Azure DNS zone is only like, $0.50 cents per month!

## Pre-reqs
You'll need a few things first. Go to Azure and set yourself up a DNS zone. [The official docs](https://learn.microsoft.com/en-us/azure/dns/dns-getstarted-portal) will take you pretty far along the right path to getting things set up.

Next, you'll need an [App Registration](https://learn.microsoft.com/en-us/entra/identity-platform/quickstart-register-app?tabs=certificate) in the same Resource Group as your DNS zone. Set up your registration to only run this application. Never run multiple things on a single registration!

This app runs best in [Docker](https://www.docker.com), so if you don't have that set up, you should definitely go do some reading and get up-to-speed on setting up docker. This repo includes a dockerfile to build your own custom docker image.

## Config
Unlike the previous version, this new version utilizes environment configs and writes nothing to disk. In that regard, we're going to use a `docker-compose.yml` file to explain the configuration.

You'll need the following information from Azure for this to run.
```
version: '3.3'
services:
  AzDynDnsv2:
    image: mydockerregistry:5000/azuredynamicdns:latest
    container_name: AzDynDnsv2
    environment:
      - TZ=Etc/UTC
      - SubscriptionId=00000000-1234-1234-1234-000000000000 ### [REQUIRED] Required to identify where in Azure your resource exists
      - AZURE_TENANT_ID=00000000-1234-1234-1234-000000000001 ### [REQUIRED] Required to identify where in Azure your resource exists
      - AZURE_CLIENT_ID=00000000-1234-1234-1234-000000000010 ### [REQUIRED] The Client ID of the App Registration used for AzDDNSv2
      - AZURE_CLIENT_SECRET=<azure_client_secret> ### [REQUIRED] The secret used to authenticate the app in Azure DO NOT SHARE THIS VALUE WITH ANYONE
      - ResourceGroup=myresourcegroup ### [REQUIRED] this is the Resource Group your zone exists in
      - ZoneName=mydomain.com ### [REQUIRED] this specifies which zone is being updated
      - RecordNames=mysubdomain ### [REQUIRED] This is the record(s) or subdomain(s) you will updating! To include more than one, simply separate each value by comma. E.g.: www,connect,email 
      - TimeToLive=3600 ### [OPTIONAL] TTL of the record being updated
      - UpdateInterval=300 ### [OTIONAL] The interval (in seconds) that the app will check for updates to your IP address
      - CreatedBy=AzureDynamicDnsV2 ### [OPTIONAL] this is the default value, only change if you want to change the "createdBy" metadata value in the A record being utilized.
    restart: unless-stopped

```

This will update the record name specified in `zoneName`. To use the root record, use `@` or otherwise specify your preferred record. This will update any A-type record you specify, and create a new one if one is not already present.

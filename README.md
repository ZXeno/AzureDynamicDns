# Azure Dynamic DNS

This program is a cross-platform implementation for using an Azure DNS zone as a dynamic DNS service. You'll need the relevant information from Azure for this software to work.


## Config
The program will create a default configuration file in the executing user's home directory.

Windows: ``` %AppData%\\azdns\\azdns.json ```

Linux/Mac: ``` ~/.config/azdns/azdns.json ```

Use the follwing information from your Azure account DNS zone service. Below is an example:
```json
{
    "resourceGroup": "myresourcegroup",
    "zoneName": "myzone.name",
    "recordName": "@",
    "subscriptionId": "00000000-0000-0000-0000-000000000000",
    "tenantId": "00000000-0000-0000-0000-000000000001",
    "clientId": "00000000-0000-0000-0000-000000000010",
    "clientSecret": "AbCdEfGhIjKlMnOpQrStUvWxYz~012345678"
}
```
This will update the record name specified in `zoneName`. To use the root record, use `@` or otherwise specify your preferred record. This will update any A-type record you specify.


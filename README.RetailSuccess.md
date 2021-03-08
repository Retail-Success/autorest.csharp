## building the docker image

### one time setup

1. Install [Autorest](https://github.com/Azure/autorest) using npm.
1. Clone our fork of [autorest.csharp](https://github.com/Retail-Success/autorest.csharp)
    1. Checkout the `feature/retailsuccess_customization` branch where we keep the customizations we have made.
1. Run ```'autorest --csharp --use=@microsoft.azure/autorest.csharp@2.3.82'``` on the command line to install the `autorest.csharp` extension
    1. This will create a folder under your user directory:\
`C:\Users\<YOURUSERNAME>\.autorest\@microsoft.azure_autorest.csharp@2.3.82`
1. Copy the installed `@microsoft.azure_autorest.csharp@2.3.82` extension folder from your user folder into the folder you cloned the Retail Success fork of the `autorest.csharp` module.
    1. You should now have a directory structure like the following:\
`<CLONEDFOLDER>\@microsoft.azure_autorest.csharp@2.3.82`
1. Open a command prompt or powershell window at the root of the folder location you cloned the Retail Success fork of the `autorest.csharp` module and run:\
`npm install`

### putting the new code in the right places

* use CopyToAutoRestCSharp.ps1

#### manual instructions for CopyToAutoRestCSharp.ps1

1. copy the folders inside the src folder (except bin and obj) of the clone you built in step #2 over the src folders in the `@microsoft.azure_autorest.csharp@2.3.82` folder you copied into your cloned folder.
1. publish to a folder (i.e. dotnet publish -o published)
1. copy the bin from the folder you published over the src/bin autorest.csharp extension local copy

### build the image

* `docker build -t retailsuccess/autorest:latest .`

**note:** you may have to comment out the code in the `.codegen\Write-SDK.ps1` script that pulls the latest version of the image from docker hub while testing locally:

`docker pull retailsuccess/autorest:latest`

## publishing the docker image

* see build-docker-image.ps1

## Troubleshooting
If you are experiencing compile errors in the test project, make sure you run `npm install` and see if that fixes your compile issues.
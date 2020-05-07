## building the docker image

### one time setup

* clone our fork of https://github.com/Azure/autorest.csharp
* ```run 'autorest --csharp'``` so that autorest will install the autorest.csharp extension
* copy the autorest.csharp extension folder into this folder (C:\Users\USER\.autorest\@microsoft.azure_autorest.csharp@2.3.82 => .\@microsoft.azure_autorest.csharp@2.3.82) -- note that docker may not be able to access it in the user folder

### putting the new code in the right places

* use CopyToAutoRestCSharp.ps1

#### manual instructions for CopyToAutoRestCSharp.ps1

* copy the folders inside the src folder (except bin and obj) of the clone you build in step #1 over the src folders in the installed autorest.csharp extension local copy
* publish to a folder (i.e. dotnet publish -o published)
* copy the bin from the folder you published over the src/bin autorest.csharp extension local copy

### build the image

* ```docker build -t retailsuccess/autorest:latest .```

note: you may have to comment out the code that pulls the latest version of the image from docker hub while testing locally

## publishing the docker image

* see build-docker-image.ps1

## Troubleshooting
If you are experiencing compile errors in the test project, make sure you run `npm install` and see if that fixes your compile issues.
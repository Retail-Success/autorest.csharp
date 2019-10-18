# clone our fork of https://github.com/Azure/autorest.csharp and publish to a folder
# run 'autorest --csharp' so that autorest will install the autorest.csharp extension
# copy the autorest.csharp extension folder into this folder (C:\Users\USER\.autorest\@microsoft.azure_autorest.csharp@2.3.82 => .\@microsoft.azure_autorest.csharp@2.3.82) -- note that docker cannot access it in the user folder
# copy the folders inside the src folder of the clone you build in step #1 over the src folders in the installed autorest.csharp extension local copy
# copy the bin from the folder you published over the src/bin autorest.csharp extension local copy

FROM azuresdk/autorest
COPY ./@microsoft.azure_autorest.csharp@2.3.82 /root/.autorest/@microsoft.azure_autorest.csharp@2.3.82

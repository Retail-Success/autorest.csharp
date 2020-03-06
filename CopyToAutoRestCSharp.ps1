if (Test-Path "@microsoft.azure_autorest.csharp@2.3.82\node_modules\@microsoft.azure\autorest.csharp\src\azure") {
	Remove-Item -path "@microsoft.azure_autorest.csharp@2.3.82\node_modules\@microsoft.azure\autorest.csharp\src\azure" -recurse
}
if (Test-Path "@microsoft.azure_autorest.csharp@2.3.82\node_modules\@microsoft.azure\autorest.csharp\src\azurefluent") {
	Remove-Item -path "@microsoft.azure_autorest.csharp@2.3.82\node_modules\@microsoft.azure\autorest.csharp\src\azurefluent" -recurse
}
if (Test-Path "@microsoft.azure_autorest.csharp@2.3.82\node_modules\@microsoft.azure\autorest.csharp\src\jsonrpc") {
	Remove-Item -path "@microsoft.azure_autorest.csharp@2.3.82\node_modules\@microsoft.azure\autorest.csharp\src\jsonrpc" -recurse
}
if (Test-Path "@microsoft.azure_autorest.csharp@2.3.82\node_modules\@microsoft.azure\autorest.csharp\src\simplifier") {
	Remove-Item -path "@microsoft.azure_autorest.csharp@2.3.82\node_modules\@microsoft.azure\autorest.csharp\src\simplifier" -recurse
}
if (Test-Path "@microsoft.azure_autorest.csharp@2.3.82\node_modules\@microsoft.azure\autorest.csharp\src\vanilla") {
	Remove-Item -path "@microsoft.azure_autorest.csharp@2.3.82\node_modules\@microsoft.azure\autorest.csharp\src\vanilla" -recurse
}

Copy-Item -Path "src\azure" -Destination "@microsoft.azure_autorest.csharp@2.3.82\node_modules\@microsoft.azure\autorest.csharp\src\azure" -Recurse
Copy-Item -Path "src\azurefluent" -Destination "@microsoft.azure_autorest.csharp@2.3.82\node_modules\@microsoft.azure\autorest.csharp\src\azurefluent" -Recurse
Copy-Item -Path "src\jsonrpc" -Destination "@microsoft.azure_autorest.csharp@2.3.82\node_modules\@microsoft.azure\autorest.csharp\src\jsonrpc" -Recurse
Copy-Item -Path "src\simplifier" -Destination "@microsoft.azure_autorest.csharp@2.3.82\node_modules\@microsoft.azure\autorest.csharp\src\simplifier" -Recurse
Copy-Item -Path "src\vanilla" -Destination "@microsoft.azure_autorest.csharp@2.3.82\node_modules\@microsoft.azure\autorest.csharp\src\vanilla" -Recurse

if (Test-Path "published") {
	Remove-Item -path "published" -recurse
}
dotnet publish -o published
Remove-Item -path "@microsoft.azure_autorest.csharp@2.3.82\node_modules\@microsoft.azure\autorest.csharp\src\bin\netcoreapp2.0\*" -recurse
Copy-Item -Path "published\*" -Destination "@microsoft.azure_autorest.csharp@2.3.82\node_modules\@microsoft.azure\autorest.csharp\src\bin\netcoreapp2.0\" -Recurse

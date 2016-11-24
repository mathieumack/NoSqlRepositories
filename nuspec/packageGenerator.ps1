#$location  = "C:\Sources\NoSqlRepositories";
$location  = $env:APPVEYOR_BUILD_FOLDER

$locationNuspec = $location + "\nuspec"
$locationNuspec
	
Set-Location -Path $locationNuspec

"Packaging to nuget..."
"Build folder : " + $location

$strPath = $location + '\NoSqlRepositories.Core\bin\Release\NoSqlRepositories.Core.dll'

$VersionInfos = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($strPath)
$ProductVersion = $VersionInfos.ProductVersion
"Product version : " + $ProductVersion

"Update nuspec versions ..."	
$nuSpecFile =  $locationNuspec + '\NoSqlRepositories.Core.nuspec'
(Get-Content $nuSpecFile) | 
Foreach-Object {$_ -replace "{BuildNumberVersion}", "$ProductVersion" } | 
Set-Content $nuSpecFile

"Generate nuget package ..."
.\NuGet.exe pack NoSqlRepositories.Core.nuspec

"Update nuspec versions ..."
$nuSpecFile =  $locationNuspec + '\NoSqlRepositories.MvvX.CouchBaseLite.Pcl.nuspec'
(Get-Content $nuSpecFile) | 
Foreach-Object {$_ -replace "{BuildNumberVersion}", "$ProductVersion" } | 
Set-Content $nuSpecFile

"Generate nuget package ..."
.\NuGet.exe pack NoSqlRepositories.MvvX.CouchBaseLite.Pcl.nuspec

"Update nuspec versions ..."
$nuSpecFile =  $locationNuspec + '\NoSqlRepositories.MvvX.JsonFiles.Pcl.nuspec'
(Get-Content $nuSpecFile) | 
Foreach-Object {$_ -replace "{BuildNumberVersion}", "$ProductVersion" } | 
Set-Content $nuSpecFile

"Generate nuget package ..."
.\NuGet.exe pack NoSqlRepositories.MvvX.JsonFiles.Pcl.nuspec

"Update nuspec versions ..."
$nuSpecFile =  $locationNuspec + '\NoSqlRepositories.MvvX.JsonFiles.Net.nuspec'
(Get-Content $nuSpecFile) | 
Foreach-Object {$_ -replace "{BuildNumberVersion}", "$ProductVersion" } | 
Set-Content $nuSpecFile

"Generate nuget package ..."
.\NuGet.exe pack NoSqlRepositories.MvvX.JsonFiles.Net.nuspec

$apiKey = $env:NuGetApiKey
	
#"Publish packages ..."	
.\NuGet push NoSqlRepositories.Core.$ProductVersion.nupkg -Source https://www.nuget.org/api/v2/package -ApiKey $apiKey
.\NuGet push NoSqlRepositories.MvvX.CouchBaseLite.Pcl.$ProductVersion.nupkg -Source https://www.nuget.org/api/v2/package -ApiKey $apiKey
.\NuGet push NoSqlRepositories.MvvX.JsonFiles.Pcl.$ProductVersion.nupkg -Source https://www.nuget.org/api/v2/package -ApiKey $apiKey
.\NuGet push NoSqlRepositories.MvvX.JsonFiles.Net.$ProductVersion.nupkg -Source https://www.nuget.org/api/v2/package -ApiKey $apiKey

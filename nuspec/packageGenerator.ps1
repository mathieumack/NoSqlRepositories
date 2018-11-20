
write-host "**************************" -foreground "Cyan"
write-host "*   Packaging to nuget   *" -foreground "Cyan"
write-host "**************************" -foreground "Cyan"

#$location  = "C:\Sources\NoSqlRepositories";
$location  = $env:APPVEYOR_BUILD_FOLDER

$locationNuspec = $location + "\nuspec"
$locationNuspec
	
Set-Location -Path $locationNuspec

$strPath = $location + '\NoSqlRepositories.Core\bin\Release\NoSqlRepositories.Core.dll'

write-host "Update the nuget.exe file" -foreground "DarkGray"
.\NuGet.exe update -self
	
$VersionInfos = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($strPath)
$ProductVersion = $VersionInfos.ProductVersion
write-host "Product version : " $ProductVersion -foreground "Green"

write-host "Packaging to nuget..." -foreground "Magenta"

write-host "Update nuspec versions" -foreground "Green"

write-host "Update nuspec versions NoSqlRepositories.Core.nuspec" -foreground "DarkGray"
$nuSpecFile =  $locationNuspec + '\NoSqlRepositories.Core.nuspec'
(Get-Content $nuSpecFile) | 
Foreach-Object {$_ -replace "{BuildNumberVersion}", "$ProductVersion" } | 
Set-Content $nuSpecFile

write-host "Update nuspec versions NoSqlRepositories.MvvX.CouchBaseLite.Pcl.nuspec"	 -foreground "DarkGray"
$nuSpecFile =  $locationNuspec + '\NoSqlRepositories.MvvX.CouchBaseLite.Pcl.nuspec'
(Get-Content $nuSpecFile) | 
Foreach-Object {$_ -replace "{BuildNumberVersion}", "$ProductVersion" } | 
Set-Content $nuSpecFile

write-host "Update nuspec versions NoSqlRepositories.MvvX.JsonFiles.Pcl.nuspec"	 -foreground "DarkGray"
$nuSpecFile =  $locationNuspec + '\NoSqlRepositories.MvvX.JsonFiles.Pcl.nuspec'
(Get-Content $nuSpecFile) | 
Foreach-Object {$_ -replace "{BuildNumberVersion}", "$ProductVersion" } | 
Set-Content $nuSpecFile

write-host "Update nuspec versions NoSqlRepositories.JsonFiles.Net.nuspec"	 -foreground "DarkGray"
$nuSpecFile =  $locationNuspec + '\NoSqlRepositories.JsonFiles.Net.nuspec'
(Get-Content $nuSpecFile) | 
Foreach-Object {$_ -replace "{BuildNumberVersion}", "$ProductVersion" } | 
Set-Content $nuSpecFile

write-host "Update nuspec versions NoSqlRepositories.MongoDb.Net.nuspec"	 -foreground "DarkGray"
$nuSpecFile =  $locationNuspec + '\NoSqlRepositories.MongoDb.Net.nuspec'
(Get-Content $nuSpecFile) | 
Foreach-Object {$_ -replace "{BuildNumberVersion}", "$ProductVersion" } | 
Set-Content $nuSpecFile

write-host "Generate nuget packages" -foreground "Green"

write-host "Generate nuget package for NoSqlRepositories.Core.nuspec" -foreground "DarkGray"
.\NuGet.exe pack NoSqlRepositories.Core.nuspec
write-host "Generate nuget package for NoSqlRepositories.MvvX.CouchBaseLite.Pcl.nuspec"	 -foreground "DarkGray"
.\NuGet.exe pack NoSqlRepositories.MvvX.CouchBaseLite.Pcl.nuspec
write-host "Generate nuget package for NoSqlRepositories.MvvX.JsonFiles.Pcl.nuspec"	 -foreground "DarkGray"
.\NuGet.exe pack NoSqlRepositories.MvvX.JsonFiles.Pcl.nuspec
write-host "Generate nuget package for NoSqlRepositories.JsonFiles.Net.nuspec"	 -foreground "DarkGray"
.\NuGet.exe pack NoSqlRepositories.JsonFiles.Net.nuspec
write-host "Generate nuget package for NoSqlRepositories.MongoDb.Net.nuspec"	 -foreground "DarkGray"
.\NuGet.exe pack NoSqlRepositories.MongoDb.Net.nuspec

$apiKey = $env:NuGetApiKey
	
write-host "Publish nuget packages" -foreground "Green"

write-host NoSqlRepositories.Core.$ProductVersion.nupkg -foreground "DarkGray"
.\NuGet push NoSqlRepositories.Core.$ProductVersion.nupkg -Source https://www.nuget.org/api/v2/package -ApiKey $apiKey

write-host NoSqlRepositories.MvvX.CouchBaseLite.Pcl.$ProductVersion.nupkg -foreground "DarkGray"
.\NuGet push NoSqlRepositories.MvvX.CouchBaseLite.Pcl.$ProductVersion.nupkg -Source https://www.nuget.org/api/v2/package -ApiKey $apiKey

write-host NoSqlRepositories.MvvX.JsonFiles.Pcl.$ProductVersion.nupkg -foreground "DarkGray"
.\NuGet push NoSqlRepositories.MvvX.JsonFiles.Pcl.$ProductVersion.nupkg -Source https://www.nuget.org/api/v2/package -ApiKey $apiKey

write-host NoSqlRepositories.JsonFiles.Net.$ProductVersion.nupkg -foreground "DarkGray"
.\NuGet push NoSqlRepositories.JsonFiles.Net.$ProductVersion.nupkg -Source https://www.nuget.org/api/v2/package -ApiKey $apiKey

write-host NoSqlRepositories.MongoDb.Net.$ProductVersion.nupkg -foreground "DarkGray"
.\NuGet push NoSqlRepositories.MongoDb.Net.$ProductVersion.nupkg -Source https://www.nuget.org/api/v2/package -ApiKey $apiKey

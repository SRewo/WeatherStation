testLib:
	dotnet test "WeatherStation.Library.Tests/WeatherStation.Library.Tests.csproj"

testApp:
	dotnet test "WeatherStation.App.Tests/WeatherStation.App.Tests.csproj"

testAll:
	dotnet test

buildApp:
	msbuild "WeatherStation.App/WeatherStation.App.Android/WeatherStation.App.Android.csproj" /t:SignAndroidPackage

installApp:
	msbuild "WeatherStation.App/WeatherStation.App.Android/WeatherStation.App.Android.csproj" /t:Install

restorePackages:
	dotnet restore 

getAndroidLogs:
	rm -R ./Logs
	adb exec-out run-as com.companyname.weatherstation.app cp -R /data/user/0/com.companyname.weatherstation.app/files /storage/self/primary/documents
	adb pull /storage/self/primary/documents/files ./Logs
	adb exec-out rm -R /storage/self/primary/documents/files

clearAndroidLogs:
	adb exec-out run-as com.companyname.weatherstation.app rm -R /data/user/0/com.companyname.weatherstation.app/files
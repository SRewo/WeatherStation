testlib:
	dotnet test "WeatherStation.Library.Tests/WeatherStation.Library.Tests.csproj"
testapp:
	dotnet test "WeatherStation.App.Tests/WeatherStation.App.Tests.csproj"
testall:
	dotnet test
buildApp:
	msbuild "WeatherStation.App/WeatherStation.App.Android/WeatherStation.App.Android.csproj" /t:SignAndroidPackage
installApp:
	msbuild "WeatherStation.App/WeatherStation.App.Android/WeatherStation.App.Android.csproj" /t:Install
restorePackages:
	dotnet restore 
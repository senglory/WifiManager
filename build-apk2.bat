rem .nuget\nuget restore eqHSG.sln
"C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" /t:Rebuild,PackageForAndroid,SignAndroidPackage /p:Configuration=Release C:\prj\Demo\Xamarin2\WiFiManager\WiFiManager.Android\WiFiManager.Android.csproj
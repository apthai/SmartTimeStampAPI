set mypath=%cd%

dotnet script %mypath%\PocosGenerator.csx -- output:Models.cs namespace:com.apthai.SystemNameAPI config:..\appsettings.json connectionstring:ConnectionStrings:DefaultConnection dapper:true

set mypath=%cd%

dotnet script %mypath%\PocosGenerator.csx -- output:DefectModel.cs namespace:com.apthai.SmartTimeStampAPI.Model config:..\appsettings.json connectionstring:ConnectionStrings:DefaultConnection dapper:true

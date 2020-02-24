FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY com.apthai.SmartTimeStampAPI.csproj ./
RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR /src/
RUN dotnet build com.apthai.SmartTimeStampAPI.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish com.apthai.SmartTimeStampAPI.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "com.apthai.SmartTimeStampAPI.dll"]
 
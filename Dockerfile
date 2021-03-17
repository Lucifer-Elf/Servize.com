
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Servize.csproj", ""]
RUN dotnet restore "./Servize.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Servize.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Servize.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Servize.dll"]

ENV newtrestID	= "767916686704-fql4bubmbka31ftnadb70t656pa5kvab.apps.googleusercontent.com",
ENV azurevaultclientid = "81a793e3-cf6b-429c-a0e9-b34c4fc91e1f",
ENV azurevaultclientsecret = ".V-bBy-fKyP6c0S0k.aw-js_eg83U1QHqd",
ENV azurevaulttenantid = "850f4f34-5e1a-4bac-881c-bb5925b1ffed",
ENV azurevaulturl = "https://servizetestkeyvalut.vault.azure.net/"


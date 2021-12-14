FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["BlazorNewsReader.csproj", "BlazorNewsReader/"]
RUN dotnet restore "BlazorNewsReader/BlazorNewsReader.csproj"

WORKDIR "/src/BlazorNewsReader"
COPY . .

RUN dotnet build "BlazorNewsReader.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BlazorNewsReader.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlazorNewsReader.dll"]
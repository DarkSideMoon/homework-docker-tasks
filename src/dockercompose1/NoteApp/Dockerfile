FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["NoteApp/NoteApp.csproj", "NoteApp/"]
RUN dotnet restore "NoteApp/NoteApp.csproj"

COPY . .
WORKDIR "/src/NoteApp"

RUN dotnet build "NoteApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NoteApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT="Prod"
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NoteApp.dll"]
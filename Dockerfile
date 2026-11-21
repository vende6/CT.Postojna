FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
WORKDIR /src

COPY ["CT.Postojna/CT.Postojna.csproj", "CT.Postojna/"]
RUN dotnet restore "CT.Postojna/CT.Postojna.csproj"

COPY . .
WORKDIR /src/CT.Postojna
RUN dotnet publish "CT.Postojna.csproj" --configuration Release --no-restore --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CT.Postojna.dll"]

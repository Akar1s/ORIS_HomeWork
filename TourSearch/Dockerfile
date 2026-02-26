



FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY ["TourSearch/TourSearch.csproj", "TourSearch/"]


RUN dotnet restore "TourSearch/TourSearch.csproj"


COPY . .


WORKDIR "/src/TourSearch"
RUN dotnet build "TourSearch.csproj" -c Release -o /app/build


FROM build AS publish
RUN dotnet publish "TourSearch.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app


COPY --from=publish /app/publish .


COPY TourSearch/Views ./Views
COPY TourSearch/wwwroot ./wwwroot


EXPOSE 5000


ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production


ENTRYPOINT ["dotnet", "TourSearch.dll"]

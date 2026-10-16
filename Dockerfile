FROM node:22-alpine AS frontend-build
WORKDIR /src/frontend
COPY frontend/package*.json ./
RUN npm ci
COPY frontend/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src
COPY backend/SwedenStart.csproj backend/
RUN dotnet restore backend/SwedenStart.csproj
COPY backend/ backend/
RUN rm -rf backend/wwwroot && mkdir -p backend/wwwroot
COPY --from=frontend-build /src/frontend/dist/sweden-start/browser/ backend/wwwroot/
RUN dotnet publish backend/SwedenStart.csproj --configuration Release --no-restore --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=backend-build /app/publish ./
ENTRYPOINT ["dotnet", "SwedenStart.dll"]
# Sử dụng hình ảnh .NET SDK 9.0 để build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy file project và restore
COPY ["AttendanceApi.csproj", "./"]
RUN dotnet restore "AttendanceApi.csproj"

# Copy toàn bộ code và build
COPY . .
RUN dotnet publish "AttendanceApi.csproj" -c Release -o /app/publish

# Sử dụng hình ảnh .NET Runtime 9.0 để chạy
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AttendanceApi.dll"]
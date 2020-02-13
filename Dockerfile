FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY /src .
RUN dotnet restore "SSDTBuilder.csproj"
RUN dotnet build "SSDTBuilder.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SSDTBuilder.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS final
# add to path
ENV PATH="/ssdt-builder:${PATH}"
WORKDIR /ssdt-builder
COPY --from=publish /app/publish .
ENTRYPOINT ["/bin/bash"]
#ENTRYPOINT ["dotnet", "SSDTBuilder.dll"]
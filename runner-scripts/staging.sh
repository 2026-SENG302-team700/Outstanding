fuser -k 9500/tcp || true
cd staging
source .env

ASPNETCORE_ENVIRONMENT=Staging ASPNETCORE_URLS=http://0.0.0.0:9500 PathBase=/test/ ./SENG302Template.Api
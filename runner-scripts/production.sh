fuser -k 10500/tcp || true
cd production
source .env

ASPNETCORE_ENVIRONMENT=Production ASPNETCORE_URLS=http://0.0.0.0:10500 PathBase=/prod/ ./SENG302.Api
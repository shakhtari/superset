docker network create supersetabp --label=supersetabp
docker-compose -f docker-compose.infrastructure.yml up -d
exit $LASTEXITCODE
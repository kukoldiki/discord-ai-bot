# edit pass pls
docker volume create bot-postgres-data

docker run --name bot-postgres \
  -e POSTGRES_USER=bot \
  -e POSTGRES_PASSWORD=pass \
  -e POSTGRES_DB=botdb \
  -p 127.0.0.1:5432:5432 \
  -v bot-postgres-data:/var/lib/postgresql \
  -d postgres
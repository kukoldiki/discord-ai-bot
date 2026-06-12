# edit pass pls
docker run --name bot-postgres \
  -e POSTGRES_USER=bot \
  -e POSTGRES_PASSWORD=pass \
  -e POSTGRES_DB=botdb \
  -p 127.0.0.1:5432:5432 \
  -d postgres
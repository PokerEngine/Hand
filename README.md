# Hand
A microservice which is responsible for playing a separate hand independently on the context

## Cookbook

### How to prepare the environment
- Install Docker: https://docs.docker.com/engine/install/
- Install Docker Compose Plugin: https://docs.docker.com/compose/install/linux/

### How to run the project?
Add the `.env` file to the root folder with the following content:
```env
MONGODB_USERNAME=...
MONGODB_PASSWORD=...
MONGODB_DATABASE=...

RABBITMQ_USERNAME=...
RABBITMQ_PASSWORD=...
```

Then run the following command:
```shell
docker compose up
```
When you need to stop the project, just use `Ctrl+C`

### How to destroy the project?
```shell
docker compose down --remove-orphans --volumes --rmi local
```
The command completely removes images, containers, volumes and networks.
It might be helpful when you need to clear all data and run the project from scratch

### How to run tests?
```shell
docker compose run --rm hand dotnet test
docker compose run --rm hand dotnet test src/Infrastructure.Test
docker compose run --rm hand dotnet test src/Infrastructure.Test --filter "FullyQualifiedName=Infrastructure.Test.WorkerTest.TestHandCreate"
```

### How to run formatting?
```shell
docker compose run --rm hand dotnet format
```

### TODO
- Arrange showdown right after players go all in

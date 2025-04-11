CREATE DATABASE microservices_db;
go

USE microservices_db;
go

CREATE LOGIN microservice_appuser_login WITH PASSWORD='admin@123', DEFAULT_DATABASE=microservices_db;
go

CREATE USER appuser FOR LOGIN microservice_appuser_login WITH DEFAULT_SCHEMA=dbo;
go

ALTER ROLE db_creator ADD MEMBER appuser;
go
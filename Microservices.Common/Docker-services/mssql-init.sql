CREATE DATABASE microservices_db;
go

USE microservices_db;
go

CREATE LOGIN microservice_appuser_login WITH PASSWORD='Admin@123', DEFAULT_DATABASE=microservices_db;
go

CREATE USER microserviceappuser FOR LOGIN microservice_appuser_login WITH DEFAULT_SCHEMA=dbo;
go

ALTER ROLE dbcreator ADD MEMBER microserviceappuser;
go
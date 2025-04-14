USE [master]
GO

CREATE LOGIN [microservice_appuser_login] WITH PASSWORD=N'Appuser#123', DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=ON
GO

ALTER LOGIN [microservice_appuser_login] DISABLE
GO

ALTER SERVER ROLE [dbcreator] ADD MEMBER [microservice_appuser_login]
GO

ALTER LOGIN [microservice_appuser_login] ENABLE
GO


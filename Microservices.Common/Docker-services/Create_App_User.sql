USE [master]
GO
CREATE LOGIN [microservice_app_user] WITH PASSWORD=N'Appuser#123', DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=ON, CHECK_POLICY=ON
GO

ALTER LOGIN [microservice_app_user] DISABLE
GO

ALTER SERVER ROLE [dbcreator] ADD MEMBER [microservice_app_user]
GO

ALTER LOGIN [microservice_app_user] ENABLE
GO



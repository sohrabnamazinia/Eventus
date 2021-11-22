# Eventus

A web application (Back-end side for Eventus) 
* ASP.Net core (C\#), Redis, SQLServer, MinIO, etc  
* The server side contains unit tests to check different parts of the project
* It uses design patterns like dependency Injection  
* JWT Authentication and Authorization
* It also includes techniques for considering security as in important factor. for example, it keeps used tokens after logout in a blacklist by Redis, so that others can login with the same token after someones logs out.

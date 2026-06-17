# Postman Assets

This folder contains a ready-to-import Postman setup for demonstrating the Contact Manager
API.

## Files

- `ContactManager.postman_collection.json`
- `ContactManager.localhost.postman_environment.json`
- `ContactManager.docker.postman_environment.json`

## Environments

### Localhost

Use when running:

- API with `dotnet run` on `http://localhost:5050`
- Angular dev server with `npm start` on `http://localhost:4200`

### Docker

Use when running:

- API in Docker on `http://localhost:8085`
- Web in Docker on `http://localhost:4300`

## Suggested demo order

1. Import the collection and one environment.
2. Run `Auth / Login demo` to get a token.
3. Run `Accounts / Get current account`.
4. Run the `Contacts CRUD` requests in order.
5. Run the `Security Cases` folder in order to show `401` and `403` behavior.
6. Optionally run the `Setup - Dynamic User Flow` folder to demonstrate register,
   login, profile fetch, password change, and login again with the new password.

## Notes

- Login requests automatically store `token`.
- Account fetch requests automatically store `userId`.
- Contact create and list requests store contact ids for later requests.
- The dynamic registration request generates a unique username/email on each run.

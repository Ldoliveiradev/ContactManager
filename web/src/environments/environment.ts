export const environment = {
  production: false,
  // Containerized API (docker compose) is published on host port 8085.
  // For `dotnet run` on the host, the API defaults to https://localhost:7xxx /
  // http://localhost:5xxx — adjust here if running that way.
  apiUrl: 'http://localhost:8085/api',
};

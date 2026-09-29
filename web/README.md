# Reservae Web

Frontend em Next.js com um cliente HTTP tipado, gerado pelo Orval a partir do OpenAPI da API ASP.NET Core.

## Desenvolvimento

Suba a API primeiro, a partir da raiz do repositório:

```powershell
dotnet run --project api/Reservae.csproj --launch-profile http
```

Em outro terminal, configure e execute o frontend:

```powershell
cd web
Copy-Item .env.example .env.local
npm run generate:api
npm run dev
```

O frontend fica disponível em `http://localhost:3000` e a API em `http://localhost:5144`.

## Cliente da API

O comando abaixo lê o documento em `OPENAPI_URL` e recria `lib/api`:

```powershell
npm run generate:api
```

Durante alterações frequentes nos endpoints, também é possível manter o gerador observando o schema:

```powershell
npm run generate:api:watch
```

Importe operações e tipos pelo índice gerado:

```ts
import { getApiSpaces, type CreateSpaceDto } from "@/lib/api";
```

`NEXT_PUBLIC_API_URL` define a URL usada pelas chamadas no navegador. `OPENAPI_URL` define somente de onde o Orval lê o Swagger durante a geração.

Os arquivos de `lib/api` são gerados automaticamente e não devem ser editados manualmente.

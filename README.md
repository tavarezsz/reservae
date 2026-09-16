# Reservae — Entity Framework Core + Identity

API ASP.NET Core (.NET 10) com PostgreSQL, EF Core Code First e ASP.NET Core Identity.

## Estrutura

- `models/User.cs`: herda de `IdentityUser`. `Id`, `Email`, `PasswordHash` e os demais campos de autenticação são herdados. Não gere nem salve hashes manualmente: use `UserManager<User>`.
- `models/Space.cs`: espaço com proprietário obrigatório (`OwnerId`), relação de um usuário para vários espaços e preço em `decimal`.
- `Data/ApplicationDbContext.cs`: herda de `IdentityDbContext<User>` e reúne tabelas do Identity e `Spaces`. Sempre chama `base.OnModelCreating`.
- `Migrations/`: migration inicial com tabelas `AspNet*` e `Spaces`. Excluir um usuário com espaços é bloqueado pela chave estrangeira.
- `Program.cs`: registra PostgreSQL, Identity, autenticação, autorização e endpoints.

Os IDs de usuário continuam sendo strings (o Identity gera um GUID representado como texto). As propriedades C# foram padronizadas em PascalCase. Categoria é persistida como inteiro; preserve os valores do enum ao adicionar categorias.

## Preparar e executar (PowerShell, na pasta do projeto)

Tenha .NET 10 e uma instância PostgreSQL acessível. Substitua usuário, senha, host e banco conforme seu ambiente. O projeto já usava Npgsql; a antiga connection string de SQL Server foi substituída por uma de PostgreSQL.

```powershell
dotnet restore
dotnet tool restore
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=reservae;Username=postgres;Password=SUA_SENHA"
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef database update
dotnet run --launch-profile https
```

A migration inicial já existe; não precisa gerar outra para iniciar. `database update` aplica as migrations e pode criar o banco se o usuário PostgreSQL tiver permissão. A aplicação não aplica migrations automaticamente ao iniciar.

Em produção, configure `ConnectionStrings__DefaultConnection` no ambiente. User Secrets é usado somente em desenvolvimento. Não coloque senhas reais nos arquivos versionados.

## Testar autenticação

Use `Reservae.http` nesta ordem:

1. `POST /auth/register` com `email` e `password`.
2. `POST /auth/login?useCookies=false` com as mesmas credenciais.
3. Copie `accessToken` da resposta e envie `Authorization: Bearer <token>` a `GET /auth/me`.

`/auth/me` exige autenticação e retorna somente ID, nome e e-mail. O token emitido pelo Identity é opaco, não é JWT. O Identity também disponibiliza `/auth/refresh` e endpoints de gerenciamento da conta.

O cadastro padrão recebe apenas e-mail e senha. `Name` é opcional e não é preenchido por esse endpoint; para coletá-lo no cadastro, crie um endpoint próprio com DTO e `UserManager.CreateAsync(user, password)`. O envio de e-mail ainda não está configurado: confirmação de conta e recuperação por e-mail precisam de uma implementação de `IEmailSender<User>` antes de serem usadas. O setup permite login sem confirmação de e-mail.

A documentação JSON fica em `/openapi/v1.json` no ambiente Development.

## Evoluir o modelo

Após alterar as entidades ou o mapeamento:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef migrations add NomeDaAlteracao
dotnet ef database update
```

Revise a migration antes de aplicá-la. Para usar o banco em endpoints, injete `ApplicationDbContext`; para operações de usuário e senha, use `UserManager<User>`.

## Validação deste setup

- Compilação com `dotnet build --no-restore` usando dependências já disponíveis: passou.
- Migration inicial gerada e verificação `has-pending-model-changes`: modelo sem alterações pendentes.
- A restauração completa de pacotes/ferramenta local ficou pendente por restrição de acesso ao NuGet.Config. A migration foi gerada com o dotnet-ef global já instalado.
- Banco e fluxo completo de cadastro/login ainda não testados: dependem da conexão real com PostgreSQL.
- O build apontou NU1903 na dependência preexistente `Microsoft.OpenApi` 2.0.0. A correção na linha 2.x está em 2.7.5 ou superior; atualizar e validar essa dependência quando o restore estiver disponível. Referência: https://github.com/advisories/GHSA-v5pm-xwqc-g5wc

Referências de implementação:
- https://learn.microsoft.com/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-10.0
- https://www.npgsql.org/efcore/

# Reservae — Entity Framework Core + Identity

API ASP.NET Core (.NET 10) com PostgreSQL, EF Core Code First e ASP.NET Core Identity.

## Estrutura

- `api/`: API ASP.NET Core, entidades, serviços, repositórios e migrations.
- `web/`: aplicação Next.js.
- `api/models/`: entidades, DTOs, enums e interfaces.
- `api/Data/ApplicationDbContext.cs`: contexto do Entity Framework e Identity.
- `api/Migrations/`: migrations do banco PostgreSQL.
- `api/Program.cs`: configuração da API, Identity, serviços e Swagger.

Os IDs de usuário continuam sendo strings (o Identity gera um GUID representado como texto). As propriedades C# foram padronizadas em PascalCase. Categoria é persistida como inteiro; preserve os valores do enum ao adicionar categorias.

## Preparar e executar a API

Tenha .NET 10 e uma instância PostgreSQL acessível. Substitua usuário, senha, host e banco conforme seu ambiente. O projeto já usava Npgsql; a antiga connection string de SQL Server foi substituída por uma de PostgreSQL.

```powershell
cd api
dotnet restore
dotnet tool restore
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=reservae;Username=postgres;Password=SUA_SENHA"
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef database update
dotnet run --launch-profile https
```

A migration inicial já existe; não precisa gerar outra para iniciar. `database update` aplica as migrations e pode criar o banco se o usuário PostgreSQL tiver permissão. No fluxo normal, a aplicação não aplica migrations automaticamente; o modo de demonstração descrito abaixo aplica as migrations antes da seed.

Em produção, configure `ConnectionStrings__DefaultConnection` no ambiente. User Secrets é usado somente em desenvolvimento. Não coloque senhas reais nos arquivos versionados.

## Banco de demonstração (seed)

O snapshot em `api/Data/Seed/demo-data.json` contém os usuários, espaços, regras, horários persistidos e reservas exportados do banco de desenvolvimento. As imagens referenciadas pelos espaços estão em `api/Data/Seed/images/`. Os IDs, relações, preços, status e datas são preservados; horários virtuais continuam sendo calculados a partir das regras, sem materializar todos os horários até 2030.

A seed só popula um banco vazio. Se já existir qualquer usuário, espaço, regra, horário ou reserva, a importação é ignorada por completo, sem sobrescrever alterações ou recriar registros excluídos. Ela não pode ser executada em `Production` e permanece desabilitada por padrão.

Para iniciar um **banco separado de demonstração**, configure a conexão e uma senha de demonstração (a mesma para todos os usuários importados):

```powershell
cd api
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=reservae_demo;Username=postgres;Password=SUA_SENHA"
$env:DemoSeed__Password = "ReservaeDemo123!"
$env:DemoSeed__Enabled = "true"
dotnet run --launch-profile http
```

O modo aplica as migrations, importa a seed em uma transação e inicia a API. As imagens são copiadas para `wwwroot/uploads/images/`. Para somente inicializar o banco e encerrar, sem abrir o servidor:

```powershell
dotnet run --no-launch-profile -- --seed-only
```

Os usuários mantêm os IDs, nomes e e-mails originais, mas **não as senhas originais**. O `UserManager` cria novos hashes usando `DemoSeed__Password`. Para testar o login, use `teste@example.com` e a senha configurada acima. Nenhum hash de senha, token ou security stamp é incluído no JSON. A seed não altera as contas do banco de origem.

Para atualizar o snapshot a partir dos dados atuais, configure a conexão do banco de origem e execute:

```powershell
$env:DemoSeed__Enabled = "false"
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=reservae;Username=postgres;Password=SUA_SENHA"
dotnet run --no-launch-profile -- --export-demo-data
```

A exportação só lê o banco; substitui o JSON e copia as imagens referenciadas para a pasta da seed. Revise os dados antes de compartilhá-los: os e-mails e nomes dos usuários estão no snapshot. Regras inválidas são rejeitadas na exportação, com indicação do ID a corrigir. Os arquivos da seed são incluídos no build e no publish da API.

Para verificar a importação em PostgreSQL real, os testes aceitam `RESERVAE_SEED_TEST_CONNECTION`. Essa conexão fornece o servidor e as credenciais; cada teste cria e remove seu próprio banco com prefixo `reservae_seed_test_`, sem usar o banco indicado nela. Sem essa variável, os testes usam EF Core InMemory.

## Testar autenticação

Use `api/Reservae.http` nesta ordem:

1. `POST /auth/register` com `email` e `password`.
2. `POST /auth/login?useCookies=false` com as mesmas credenciais.
3. Copie `accessToken` da resposta e envie `Authorization: Bearer <token>` a `GET /auth/me`.

`/auth/me` exige autenticação e retorna somente ID, nome e e-mail. O token emitido pelo Identity é opaco, não é JWT. O Identity também disponibiliza `/auth/refresh` e endpoints de gerenciamento da conta.

O cadastro padrão recebe apenas e-mail e senha. `Name` é opcional e não é preenchido por esse endpoint; para coletá-lo no cadastro, crie um endpoint próprio com DTO e `UserManager.CreateAsync(user, password)`. O envio de e-mail ainda não está configurado: confirmação de conta e recuperação por e-mail precisam de uma implementação de `IEmailSender<User>` antes de serem usadas. O setup permite login sem confirmação de e-mail.

A interface do Swagger fica em `/swagger` e o documento OpenAPI em
`/swagger/v1/swagger.json` no ambiente Development.

## Evoluir o modelo

Após alterar as entidades ou o mapeamento:

```powershell
cd api
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

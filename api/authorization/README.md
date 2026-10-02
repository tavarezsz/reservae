# Autorização de recursos

As políticas do ASP.NET Core estão em `ResourcePolicies`, e a comparação de identidade está em
`ResourceOwnershipHandler`. `ResourceOwnershipResolver` consulta apenas os IDs de usuário necessários,
obtidos a partir dos vínculos persistidos no banco.

O atributo `ResourceAuthorize` exige autenticação e aciona um filtro após o model binding,
antes de executar a action. O filtro carrega o vínculo e chama `IAuthorizationService.AuthorizeAsync`.
Isso permite proteger tanto IDs na rota quanto IDs em DTOs e formulários sem repetir verificações nos services.

```csharp
[ResourceAuthorize(ResourceKind.Space, "id")]
[HttpPut("{id:int}")]
```

```csharp
[ResourceAuthorize(ResourceKind.Space, "dto.SpaceId")]
[HttpPost]
```

O primeiro argumento identifica o tipo de recurso; o segundo identifica o parâmetro da action,
com a propriedade opcional. A política padrão é `ResourcePolicies.SpaceOwner`.

| Política | Quem pode acessar |
| --- | --- |
| `SpaceOwner` | Dono do espaço, inclusive para gerenciar suas regras, horários e imagem |
| `BookingReader` | Cliente da reserva ou dono do espaço correspondente |
| `BookingUser` | Cliente que fez a reserva, para alteração e exclusão |

`SlotCreation` resolve a regra informada antes de considerar o `SpaceId` do DTO. Assim, um ID de espaço
próprio não permite usar uma regra de outro anunciante. O service continua validando a consistência dos dados.

Consultas públicas de espaços, regras, horários e disponibilidade permanecem públicas.
Criação de reservas exige login e usa o usuário autenticado, sem exigir que ele seja dono do espaço.
As listagens `mine` usam o ID da identidade autenticada.

Respostas: `401` sem autenticação, `403` sem permissão e `404` quando o recurso não existe.

Essa proteção é aplicada na entrada HTTP. Caso um job ou outra aplicação chame os services diretamente,
deve avaliar a mesma política com `IAuthorizationService` antes da operação.

Os testes usam um banco em memória, sem modificar o PostgreSQL:

```sh
dotnet test api.Tests/api.Tests.csproj
```

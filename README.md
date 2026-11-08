# ThunderGames

E-commerce acadêmico de jogos, hardware e periféricos. O catálogo web consome uma API ASP.NET Core, que consulta o PostgreSQL do Supabase. Nenhuma credencial do banco deve ser incluída no frontend ou no repositório.

## Executar a API

Requisitos: .NET 10 SDK e acesso ao PostgreSQL do projeto Supabase.

Configure a variável de ambiente `ConnectionStrings__Supabase` na máquina de desenvolvimento ou no serviço da API no Render. Use o formato Npgsql:

```text
Host=HOST_DO_BANCO;Port=5432;Database=postgres;Username=USUARIO;Password=SENHA;SSL Mode=Require
```

Obtenha os dados em Supabase → Project Settings → Database. No Render, publique a API como um serviço Docker usando a raiz do projeto e o `Dockerfile` deste repositório. Configure a connection string nas variáveis privadas do serviço. O container escuta a porta fornecida pelo Render. Nunca use a chave `service_role` no site ou como senha do PostgreSQL.

Configure também `Jwt__Key` como uma variável privada da API. Use um segredo aleatório com pelo menos 32 bytes; não o inclua no código, no frontend ou em mensagens públicas. A aplicação não inicia sem essa configuração.

```powershell
dotnet restore ThunderGames.slnx
dotnet run --project ThunderGames.API\ThunderGames.API.csproj
```

Swagger local: `http://localhost:5165/swagger`

## Endpoints

- `GET /api/produtos` — lista produtos e nome da categoria.
- `GET /api/categorias` — lista categorias.
- `POST /api/Pessoa` — recebe nome, CPF, e-mail, senha, telefone e endereço; força o perfil `Cliente` e armazena somente o hash da senha.
- `POST /api/auth/register` e `POST /api/auth/login` — cadastro/login e emissão de token JWT.
- `PUT /api/auth/profile` — atualiza nome, telefone e endereço da conta autenticada.
- `PUT /api/auth/password` — altera a senha da conta autenticada após verificar a senha atual; a nova senha é armazenada como hash.
- `POST /api/pedidos` — registra um pedido autenticado; os preços e o usuário são obtidos no servidor.
- `GET /api/pedidos` — lista somente os pedidos do usuário autenticado.

O banco deve ter as tabelas `public.categorias`, `public.pessoas` e `public.produtos` com as colunas utilizadas pelo código. Execute `database/orders-schema.sql` no Supabase SQL Editor para criar as tabelas de pedidos antes de testar o checkout. A origem permitida para CORS é configurada em `Cors:AllowedOrigins` em `ThunderGames.API\appsettings.json`.

Para criar uma conta de demonstração, use `POST /api/Pessoa` no Swagger com senha de pelo menos 8 caracteres. Depois, execute `database/seed-demo.sql` no Supabase SQL Editor para adicionar categorias e produtos de demonstração. O script associa os produtos à primeira pessoa cadastrada e pode ser executado novamente sem duplicar os nomes existentes.

## Conectar o frontend à API

Em `api-config.js`, configure `window.THUNDERGAMES_API_URL` com a URL base pública do serviço da API, sem `/swagger` e sem barra final. Exemplo:

```js
window.THUNDERGAMES_API_URL = "https://URL-DA-API.onrender.com";
```

Depois de publicar o frontend, a página de produtos tentará carregar o catálogo pela API. Se a URL estiver vazia, a API estiver indisponível ou não houver produtos cadastrados, a página informará o estado e usará o catálogo local demonstrativo.

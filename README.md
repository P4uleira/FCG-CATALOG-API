# FCG Catalog API

## Visão Geral

A **FCG Catalog API** é o microsserviço responsável pelo gerenciamento do catálogo de jogos da plataforma **FIAP Cloud Games**.

Além das operações de cadastro e manutenção dos jogos disponíveis, este serviço também é responsável por iniciar o fluxo de compra e manter a biblioteca de jogos adquiridos pelos usuários.

Seu funcionamento combina operações síncronas, expostas através de uma API REST, com processamento assíncrono utilizando **RabbitMQ** e **MassTransit**, permitindo que a conclusão das compras ocorra de forma desacoplada dos demais microsserviços.

Atualmente, a CatalogAPI é responsável por:

* cadastrar jogos;
* consultar jogos disponíveis;
* atualizar informações do catálogo;
* realizar exclusão lógica de jogos;
* iniciar o processo de compra;
* consumir a confirmação do pagamento;
* adicionar jogos à biblioteca do usuário após a aprovação do pagamento.

Dessa forma, a CatalogAPI representa o núcleo funcional da plataforma, concentrando todas as operações relacionadas aos jogos comercializados.

---

# Arquitetura

A CatalogAPI foi desenvolvida seguindo os princípios de **Domain-Driven Design (DDD)**, **Clean Architecture** e **CQRS**, promovendo uma separação clara entre regras de negócio, infraestrutura e integração com os demais microsserviços.

A solução utiliza o **MediatR** para implementação dos casos de uso e o **RabbitMQ**, através do **MassTransit**, para comunicação assíncrona entre os serviços.

As principais tecnologias utilizadas são:

* .NET 10
* ASP.NET Core Web API
* MediatR
* SQL Server
* Entity Framework Core
* RabbitMQ
* MassTransit
* JWT Authentication
* Swagger / OpenAPI
* Docker
* Kubernetes
* ILogger

Diferentemente dos demais microsserviços da solução, a CatalogAPI possui responsabilidades tanto síncronas quanto assíncronas.

Enquanto as operações de gerenciamento do catálogo são realizadas através de endpoints HTTP, o processamento da conclusão das compras ocorre através do consumo de eventos publicados pela PaymentsAPI.

---

# Estrutura da Solução

O projeto está organizado em camadas, seguindo o padrão adotado em todos os microsserviços da solução.

```text
FCG-Catalog-Api
│
├── src
│   ├── FCG.Catalog.Api
│   │   ├── Controllers
│   │   ├── Middleware
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── Dockerfile
│   │
│   ├── FCG.Catalog.Application
│   │   ├── Commands
│   │   ├── Queries
│   │   ├── Consumers
│   │   └── DTOs
│   │
│   ├── FCG.Catalog.Domain
│   │   ├── Entities
│   │   └── Interfaces
│   │
│   ├── FCG.Catalog.Infrastructure
│   │   ├── Data
│   │   ├── Repositories
│   │   └── DependencyInjection
│   │
│   └── FCG.Catalog.Contracts
│
└── tests
```

## Camadas

### FCG.Catalog.Api

Responsável por:

* disponibilizar os endpoints HTTP;
* configurar autenticação JWT;
* configurar Swagger;
* configurar RabbitMQ;
* registrar os Consumers;
* configurar o pipeline da aplicação;
* tratamento global de exceções.

---

### FCG.Catalog.Application

Contém todos os casos de uso da aplicação.

É composta por:

* Commands;
* Queries;
* Handlers;
* Consumers;
* DTOs.

Toda a lógica de aplicação é executada nesta camada, mantendo os Controllers responsáveis apenas por receber e encaminhar as requisições.

---

### FCG.Catalog.Domain

Representa o núcleo do negócio.

Contém as entidades responsáveis pelas regras de domínio, como:

* Game;
* UserLibrary.

Também define os contratos utilizados pela aplicação através dos repositórios.

---

### FCG.Catalog.Infrastructure

Implementa o acesso aos dados da aplicação.

É responsável por:

* Entity Framework Core;
* SQL Server;
* implementação dos repositórios;
* configuração da injeção de dependência.

---

### FCG.Catalog.Contracts

Contém os contratos compartilhados utilizados na comunicação entre os microsserviços.

Atualmente a CatalogAPI publica:

* `OrderPlacedEvent`

e consome:

* `PaymentProcessedEvent`.

Esses contratos garantem que todos os microsserviços utilizem exatamente o mesmo formato de mensagens.

---

# Tecnologias Utilizadas

| Tecnologia            | Finalidade                 |
| --------------------- | -------------------------- |
| .NET 10               | Plataforma principal       |
| ASP.NET Core Web API  | Hospedagem da aplicação    |
| Entity Framework Core | Persistência dos dados     |
| SQL Server            | Banco de dados             |
| MediatR               | Implementação do CQRS      |
| RabbitMQ              | Broker de mensagens        |
| MassTransit           | Comunicação assíncrona     |
| JWT                   | Autenticação e autorização |
| Swagger               | Documentação da API        |
| Docker                | Containerização            |
| Kubernetes            | Orquestração               |
| ILogger               | Registro de logs           |

---

# Fluxo de Funcionamento

A CatalogAPI possui três fluxos principais.

---

## Gerenciamento do Catálogo

Os administradores autenticados podem cadastrar, atualizar e desativar jogos disponíveis para venda.

```text
Administrador
      │
      ▼
CatalogAPI
      │
      ▼
SQL Server
```

Essas operações são protegidas por autenticação JWT e autorização baseada em perfis (**Role = Admin**).

---

## Consulta do Catálogo

Qualquer usuário pode consultar os jogos disponíveis através dos endpoints públicos.

```text
Cliente
      │
      ▼
CatalogAPI
      │
      ▼
SQL Server
```

As consultas não alteram o estado da aplicação e retornam apenas informações do catálogo.

---

## Fluxo de Compra

Quando um usuário autenticado solicita a compra de um jogo, a CatalogAPI inicia o processamento publicando um evento no RabbitMQ.

```text
Usuário
      │
      ▼
CatalogAPI
      │
      ▼
OrderPlacedEvent
      │
      ▼
RabbitMQ
      │
      ▼
PaymentsAPI
```

Nesse momento, o pedido recebe o status inicial **Pending**.

Após o processamento do pagamento, a PaymentsAPI publica um novo evento.

```text
PaymentsAPI
      │
      ▼
PaymentProcessedEvent
      │
      ▼
RabbitMQ
      │
      ▼
CatalogAPI
      │
      ▼
CompletePurchaseCommand
      │
      ▼
Biblioteca do usuário
```

Caso o pagamento seja aprovado, o jogo é adicionado à biblioteca do usuário.

Caso o pagamento seja rejeitado, nenhuma alteração é realizada.

---

## Resumo Geral

```text
Administrador
      │
      ▼
CRUD de Jogos
      │
      ▼
CatalogAPI
      │
      ▼
SQL Server

────────────────────────────────────────────

Usuário
      │
      ▼
Consulta de Jogos
      │
      ▼
CatalogAPI
      │
      ▼
SQL Server

────────────────────────────────────────────

Usuário
      │
      ▼
Compra de Jogo
      │
      ▼
CatalogAPI
      │
      ▼
OrderPlacedEvent
      │
      ▼
RabbitMQ
      │
      ▼
PaymentsAPI
      │
      ▼
PaymentProcessedEvent
      │
      ▼
RabbitMQ
      │
      ▼
CatalogAPI
      │
      ▼
Biblioteca do Usuário
```

Essa arquitetura orientada a eventos mantém os microsserviços desacoplados, permitindo que o processamento da compra evolua de forma independente do gerenciamento do catálogo e da autenticação dos usuários.

# CQRS

A CatalogAPI implementa o padrão **CQRS (Command Query Responsibility Segregation)** para separar claramente as operações de escrita das operações de leitura.

As alterações de estado da aplicação são executadas por **Commands**, enquanto as consultas utilizam **Queries**, mantendo os casos de uso independentes e facilitando a manutenção da solução.

Toda a comunicação entre os Controllers e a camada de aplicação é realizada através do **MediatR**, que encaminha cada solicitação ao seu respectivo Handler.

---

## Commands

Os Commands representam operações que modificam o estado da aplicação.

Atualmente a CatalogAPI possui os seguintes comandos:

```text
CreateGameCommand
UpdateGameCommand
DeleteGameCommand
PurchaseGameCommand
CompletePurchaseCommand
```

Cada comando possui um Handler responsável por executar sua regra de negócio.

### CreateGameCommand

Responsável por cadastrar um novo jogo no catálogo.

Fluxo:

```text
GamesController
      │
      ▼
CreateGameCommand
      │
      ▼
CreateGameCommandHandler
      │
      ▼
IGameRepository
```

---

### UpdateGameCommand

Atualiza parcialmente as informações de um jogo existente.

Permite alterar:

* título;
* descrição;
* preço;
* gênero;
* status de ativação.

---

### DeleteGameCommand

Realiza a exclusão lógica de um jogo.

Ao invés de remover o registro do banco de dados, o Handler altera o atributo:

```text
Active = false
```

preservando o histórico do catálogo.

---

### PurchaseGameCommand

Representa o início do processo de compra.

O Handler executa as seguintes etapas:

* valida a existência do jogo;
* verifica se o jogo está ativo;
* gera um novo `OrderId`;
* publica um `OrderPlacedEvent`;
* retorna o pedido com status **Pending**.

Nesse momento nenhuma alteração é realizada na biblioteca do usuário.

A conclusão da compra depende da confirmação enviada pela PaymentsAPI.

---

### CompletePurchaseCommand

Executado após o recebimento do evento `PaymentProcessedEvent`.

O Handler é responsável por:

* validar o status do pagamento;
* verificar se a compra já existe;
* adicionar o jogo à biblioteca do usuário;
* impedir registros duplicados;
* retornar o resultado do processamento.

---

## Queries

As Queries representam operações somente leitura.

Atualmente existem:

```text
GetGamesQuery
GetGameByIdQuery
GetUserLibraryQuery
```

---

### GetGamesQuery

Retorna todos os jogos cadastrados no catálogo.

Fluxo:

```text
GamesController
      │
      ▼
GetGamesQuery
      │
      ▼
GetGamesQueryHandler
      │
      ▼
IGameRepository
```

---

### GetGameByIdQuery

Recupera um único jogo através do seu identificador.

Caso o jogo não exista, é lançada uma exceção indicando que o recurso não foi encontrado.

---

### GetUserLibraryQuery

Consulta todos os jogos pertencentes à biblioteca de um usuário.

Essa consulta utiliza o `IUserLibraryRepository`, responsável por relacionar usuários e jogos adquiridos.

---

## Por que não existe um GameService?

Embora exista um contrato `IGameService` em versões anteriores do projeto, ele não é utilizado na implementação atual.

Com a adoção de **CQRS** e **MediatR**, cada Handler passou a representar um caso de uso específico da aplicação.

A arquitetura utilizada é:

```text
Controller
      │
      ▼
Command / Query
      │
      ▼
MediatR
      │
      ▼
Handler
      │
      ▼
Repository
```

Dessa forma, não existe necessidade de um serviço genérico contendo todos os métodos do catálogo, evitando duplicação de responsabilidades e mantendo a separação entre comandos e consultas.

---

# Domínio

O domínio da CatalogAPI é composto por duas entidades principais.

---

## Game

Representa um jogo disponível para comercialização na plataforma.

### Propriedades

| Propriedade | Descrição                         |
| ----------- | --------------------------------- |
| Id          | Identificador do jogo             |
| Title       | Nome do jogo                      |
| Description | Descrição                         |
| Price       | Valor                             |
| Genre       | Categoria                         |
| Active      | Indica disponibilidade para venda |
| CreatedAt   | Data de criação                   |
| UpdatedAt   | Última atualização                |

Além das propriedades, a entidade implementa operações de domínio como:

```text
Update()
UpdatePartial()
Disable()
```

Esses métodos garantem que as alterações sejam realizadas através da própria entidade, preservando as regras de negócio.

---

## UserLibrary

Representa a biblioteca de jogos adquiridos por um usuário.

### Propriedades

| Propriedade | Descrição               |
| ----------- | ----------------------- |
| Id          | Identificador da compra |
| UserId      | Usuário proprietário    |
| GameId      | Jogo adquirido          |
| PurchasedAt | Data da compra          |

A criação da entidade ocorre através do método:

```text
UserLibrary.Create()
```

que valida os identificadores antes de permitir sua persistência.

---

# Eventos

A CatalogAPI participa do fluxo de compra tanto publicando quanto consumindo eventos.

---

## Evento Publicado

### OrderPlacedEvent

Quando um usuário inicia uma compra, a CatalogAPI publica:

```text
OrderPlacedEvent
```

Esse evento contém:

* OrderId;
* UserId;
* GameId;
* Price;
* PurchasedAt.

Fluxo:

```text
PurchaseGameCommand
      │
      ▼
OrderPlacedEvent
      │
      ▼
RabbitMQ
      │
      ▼
PaymentsAPI
```

Após sua publicação, o processamento passa a ser responsabilidade da PaymentsAPI.

---

## Evento Consumido

### PaymentProcessedEvent

Após concluir o pagamento, a PaymentsAPI publica:

```text
PaymentProcessedEvent
```

Esse evento contém:

* OrderId;
* UserId;
* GameId;
* Price;
* Status;
* ProcessedAt.

Fluxo:

```text
RabbitMQ
      │
      ▼
PaymentProcessedEventConsumer
      │
      ▼
CompletePurchaseCommand
      │
      ▼
Biblioteca do usuário
```

Caso o pagamento seja aprovado, o jogo é adicionado à biblioteca.

Caso seja rejeitado, nenhuma alteração é realizada.

---

# RabbitMQ e MassTransit

A comunicação entre os microsserviços ocorre através do **RabbitMQ**, utilizando o **MassTransit** como biblioteca de integração.

Durante a inicialização da aplicação, é registrado o Consumer:

```text
PaymentProcessedEventConsumer
```

Esse Consumer permanece aguardando mensagens na fila:

```text
catalog-payment-processed-event
```

Sempre que um novo evento é recebido:

```text
PaymentProcessedEvent
      │
      ▼
PaymentProcessedEventConsumer
      │
      ▼
CompletePurchaseCommand
      │
      ▼
CompletePurchaseCommandHandler
```

O Handler verifica:

* status do pagamento;
* duplicidade da compra;
* inclusão do jogo na biblioteca.

Essa arquitetura desacoplada permite que a CatalogAPI não precise conhecer detalhes internos da PaymentsAPI, dependendo apenas dos contratos compartilhados.

---

# Repositórios

O acesso aos dados é realizado através do padrão **Repository**.

A camada de aplicação depende apenas das abstrações definidas no domínio.

Atualmente existem dois repositórios.

---

## IGameRepository

Responsável pelo gerenciamento do catálogo.

Principais operações:

* criar jogo;
* buscar jogo por identificador;
* listar jogos;
* atualizar jogo.

Sua implementação concreta é:

```text
GameRepository
```

---

## IUserLibraryRepository

Responsável pela biblioteca dos usuários.

Principais operações:

* verificar se um jogo já foi adquirido;
* adicionar uma nova compra;
* consultar a biblioteca do usuário.

Sua implementação concreta é:

```text
UserLibraryRepository
```

A separação em dois repositórios reduz o acoplamento e mantém responsabilidades distintas entre catálogo e biblioteca.

---

# Persistência

A CatalogAPI utiliza **Entity Framework Core** com **SQL Server** para persistência dos dados.

O contexto responsável pelo acesso ao banco é:

```text
FCGCatalogDbContext
```

Atualmente são persistidas duas estruturas principais:

* catálogo de jogos;
* biblioteca dos usuários.

---

## Persistência do Catálogo

As operações de criação, atualização e exclusão lógica utilizam o `GameRepository`.

Fluxo:

```text
Command
      │
      ▼
Handler
      │
      ▼
GameRepository
      │
      ▼
Entity Framework Core
      │
      ▼
SQL Server
```

A exclusão lógica preserva os registros históricos, alterando apenas o atributo `Active`.

---

## Persistência da Biblioteca

Após a confirmação do pagamento, o `CompletePurchaseCommandHandler` utiliza o `UserLibraryRepository` para:

1. verificar se o usuário já possui o jogo;
2. impedir duplicidade;
3. registrar a compra.

Fluxo:

```text
PaymentProcessedEvent
      │
      ▼
CompletePurchaseCommand
      │
      ▼
UserLibraryRepository
      │
      ▼
Entity Framework Core
      │
      ▼
SQL Server
```

Essa abordagem garante que um mesmo usuário não possua múltiplos registros para o mesmo jogo, mesmo em cenários de reprocessamento de mensagens pelo RabbitMQ.

A utilização de um banco de dados próprio mantém a independência da CatalogAPI em relação aos demais microsserviços, respeitando o princípio de **Database per Service** adotado em toda a arquitetura da solução.

# Endpoints

A CatalogAPI disponibiliza endpoints públicos para consulta do catálogo, endpoints administrativos para gerenciamento dos jogos e um endpoint autenticado para iniciar a compra.

A conclusão da compra não ocorre diretamente pela resposta HTTP. O endpoint publica um `OrderPlacedEvent`, e o processamento continua de forma assíncrona através do RabbitMQ.

---

## Resumo dos Endpoints

| Método   | Endpoint                       | Acesso      | Descrição                         |
| -------- | ------------------------------ | ----------- | --------------------------------- |
| `POST`   | `/api/games`                   | Admin       | Cadastra um novo jogo             |
| `GET`    | `/api/games`                   | Público     | Lista os jogos                    |
| `GET`    | `/api/games/{id}`              | Público     | Consulta um jogo por ID           |
| `PUT`    | `/api/games/{id}`              | Admin       | Atualiza um jogo                  |
| `DELETE` | `/api/games/{id}`              | Admin       | Desativa um jogo                  |
| `POST`   | `/api/games/{gameId}/purchase` | Autenticado | Inicia a compra                   |
| `GET`    | `/health`                      | Público     | Verifica a disponibilidade da API |

A consulta da biblioteca do usuário possui Query e Handler implementados, mas não foi apresentado um endpoint HTTP correspondente. Por isso, ela não é documentada como rota pública nesta versão.

---

## POST /api/games

Cadastra um novo jogo no catálogo.

### Autorização

Apenas usuários com role `Admin`.

```http
POST /api/games
Authorization: Bearer <ACCESS_TOKEN>
Content-Type: application/json
```

### Exemplo de requisição

```json
{
  "title": "The Witcher 3",
  "description": "RPG de ação em mundo aberto.",
  "price": 99.90,
  "genre": "RPG"
}
```

### Exemplo com PowerShell

```powershell
curl -X POST http://localhost:8081/api/games `
  -H "Authorization: Bearer <ADMIN_ACCESS_TOKEN>" `
  -H "Content-Type: application/json" `
  -d '{
    "title": "The Witcher 3",
    "description": "RPG de ação em mundo aberto.",
    "price": 99.90,
    "genre": "RPG"
  }'
```

### Resposta esperada

```http
201 Created
```

```json
{
  "id": "6f37dcb5-1f25-4af8-a7e8-dd7d087de899",
  "title": "The Witcher 3",
  "description": "RPG de ação em mundo aberto.",
  "price": 99.90,
  "genre": "RPG",
  "active": true,
  "createdAt": "2026-07-14T18:00:00Z",
  "updatedAt": null
}
```

### Validações

* título obrigatório;
* preço não pode ser negativo.

---

## GET /api/games

Retorna todos os jogos cadastrados.

### Autorização

Endpoint público.

```http
GET /api/games
```

### Exemplo com PowerShell

```powershell
curl http://localhost:8081/api/games
```

### Resposta esperada

```http
200 OK
```

```json
[
  {
    "id": "6f37dcb5-1f25-4af8-a7e8-dd7d087de899",
    "title": "The Witcher 3",
    "description": "RPG de ação em mundo aberto.",
    "price": 99.90,
    "genre": "RPG",
    "active": true,
    "createdAt": "2026-07-14T18:00:00Z",
    "updatedAt": null
  }
]
```

Na implementação atual, a consulta não filtra automaticamente jogos inativos.

---

## GET /api/games/{id}

Retorna um jogo pelo identificador.

### Autorização

Endpoint público.

```http
GET /api/games/{id}
```

### Exemplo com PowerShell

```powershell
curl http://localhost:8081/api/games/<GAME_ID>
```

### Resposta esperada

```http
200 OK
```

Caso o jogo não exista:

```http
404 Not Found
```

---

## PUT /api/games/{id}

Atualiza parcialmente um jogo existente.

### Autorização

Apenas usuários com role `Admin`.

```http
PUT /api/games/{id}
Authorization: Bearer <ADMIN_ACCESS_TOKEN>
Content-Type: application/json
```

### Exemplo de requisição

```json
{
  "title": "The Witcher 3 Complete Edition",
  "price": 119.90,
  "active": true
}
```

### Exemplo com PowerShell

```powershell
curl -X PUT http://localhost:8081/api/games/<GAME_ID> `
  -H "Authorization: Bearer <ADMIN_ACCESS_TOKEN>" `
  -H "Content-Type: application/json" `
  -d '{
    "title": "The Witcher 3 Complete Edition",
    "price": 119.90,
    "active": true
  }'
```

### Resposta esperada

```http
200 OK
```

Os campos não enviados permanecem inalterados.

---

## DELETE /api/games/{id}

Realiza a exclusão lógica do jogo.

### Autorização

Apenas usuários com role `Admin`.

```http
DELETE /api/games/{id}
Authorization: Bearer <ADMIN_ACCESS_TOKEN>
```

### Exemplo com PowerShell

```powershell
curl -X DELETE http://localhost:8081/api/games/<GAME_ID> `
  -H "Authorization: Bearer <ADMIN_ACCESS_TOKEN>"
```

### Resposta esperada

```http
204 No Content
```

O registro não é removido fisicamente. O atributo `Active` passa a ser `false`.

---

## POST /api/games/{gameId}/purchase

Inicia a compra de um jogo para o usuário autenticado.

### Autorização

Usuário autenticado com role `User` ou `Admin`.

```http
POST /api/games/{gameId}/purchase
Authorization: Bearer <ACCESS_TOKEN>
```

O endpoint não recebe body.

O `UserId` é obtido diretamente do token JWT.

### Exemplo com PowerShell

```powershell
curl -X POST http://localhost:8081/api/games/<GAME_ID>/purchase `
  -H "Authorization: Bearer <ACCESS_TOKEN>"
```

### Resposta esperada

```http
202 Accepted
```

```json
{
  "orderId": "52054ee1-74c6-468b-9bcf-910f9b44405c",
  "userId": "d97256e9-3cc4-4f61-b071-af728067f540",
  "gameId": "6f37dcb5-1f25-4af8-a7e8-dd7d087de899",
  "price": 99.90,
  "purchasedAt": "2026-07-14T18:20:00Z",
  "status": "Pending"
}
```

O status `Pending` indica que o pedido foi publicado e aguarda o processamento da PaymentsAPI.

### Respostas possíveis

| Código                      | Situação                                      |
| --------------------------- | --------------------------------------------- |
| `202 Accepted`              | Compra iniciada                               |
| `401 Unauthorized`          | Token ausente, inválido ou sem usuário válido |
| `404 Not Found`             | Jogo não encontrado                           |
| `409 Conflict`              | Jogo inativo                                  |
| `500 Internal Server Error` | Erro inesperado                               |

---

## GET /health

Verifica se a CatalogAPI está respondendo.

```http
GET /health
```

### Exemplo

```powershell
curl http://localhost:8081/health
```

### Resposta esperada

```json
{
  "service": "CatalogAPI",
  "status": "Healthy"
}
```

O health check atual confirma a disponibilidade HTTP da aplicação, mas não executa uma verificação profunda do SQL Server ou RabbitMQ.

---

# Autenticação e Autorização

A CatalogAPI valida tokens JWT gerados pela UsersAPI.

As configurações precisam utilizar os mesmos valores de:

* issuer;
* audience;
* chave de assinatura.

---

## Claims Utilizadas

A aplicação utiliza:

```text
ClaimTypes.Name
ClaimTypes.Role
ClaimTypes.NameIdentifier
```

No endpoint de compra, o identificador do usuário é obtido por:

```text
ClaimTypes.NameIdentifier
```

com fallback para:

```text
sub
```

Se o token não contiver um `Guid` válido, a API retorna:

```http
401 Unauthorized
```

---

## Regras de Acesso

| Operação       | Público | User | Admin |
| -------------- | ------: | ---: | ----: |
| Listar jogos   |     Sim |  Sim |   Sim |
| Buscar jogo    |     Sim |  Sim |   Sim |
| Criar jogo     |     Não |  Não |   Sim |
| Atualizar jogo |     Não |  Não |   Sim |
| Excluir jogo   |     Não |  Não |   Sim |
| Comprar jogo   |     Não |  Sim |   Sim |

---

## Obter Token

O login é realizado na UsersAPI:

```http
POST /api/auth/login
```

### Exemplo

```powershell
curl -X POST http://localhost:8080/api/auth/login `
  -H "Content-Type: application/json" `
  -d '{
    "email": "usuario@fcg.com",
    "password": "123456"
  }'
```

Copie o valor de:

```json
{
  "accessToken": "<TOKEN>"
}
```

---

## Utilizar Token

```powershell
-H "Authorization: Bearer <ACCESS_TOKEN>"
```

No Swagger, clique em **Authorize** e informe somente o token, conforme a configuração apresentada na interface.

---

# Configuração

As configurações não sensíveis permanecem no `appsettings.json`.

Exemplo:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "MassTransit": "Warning",
      "RabbitMQ.Client": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Jwt": {
    "Issuer": "FCG.Users.Api",
    "Audience": "FCG.Client"
  },
  "RabbitMq": {
    "Host": "localhost"
  }
}
```

Credenciais e chaves sensíveis devem ser fornecidas externamente.

---

## Variáveis de Ambiente

| Variável                               | Finalidade                       |
| -------------------------------------- | -------------------------------- |
| `ConnectionStrings__DefaultConnection` | Connection string do SQL Server  |
| `Jwt__Key`                             | Chave de assinatura do token     |
| `Jwt__Issuer`                          | Emissor esperado                 |
| `Jwt__Audience`                        | Público esperado                 |
| `RabbitMq__Host`                       | Host do RabbitMQ                 |
| `RabbitMq__Username`                   | Usuário do RabbitMQ              |
| `RabbitMq__Password`                   | Senha do RabbitMQ                |
| `ASPNETCORE_ENVIRONMENT`               | Ambiente da aplicação            |
| `ASPNETCORE_HTTP_PORTS`                | Porta HTTP interna               |
| `DOTNET_RUNNING_IN_CONTAINER`          | Identifica execução em container |

### Exemplo no PowerShell

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost,1433;Database=FCGCatalogDb;User Id=sa;Password=<SQLSERVER_PASSWORD>;TrustServerCertificate=True;"

$env:Jwt__Key = "<JWT_KEY>"
$env:Jwt__Issuer = "FCG.Users.Api"
$env:Jwt__Audience = "FCG.Client"

$env:RabbitMq__Host = "localhost"
$env:RabbitMq__Username = "<RABBITMQ_USER>"
$env:RabbitMq__Password = "<RABBITMQ_PASSWORD>"
```

A `Jwt__Key` deve ser exatamente a mesma utilizada pela UsersAPI.

---

# Execução Local

## Pré-requisitos

* .NET SDK 10;
* SQL Server;
* RabbitMQ;
* banco configurado;
* migrações aplicadas;
* credenciais válidas;
* configurações JWT compatíveis com a UsersAPI.

---

## Acessar o Projeto

```powershell
cd D:\FIAP-FCG-MICROSERVICOS\FCG-Catalog-Api
```

---

## Restaurar Dependências

```powershell
dotnet restore
```

---

## Compilar

```powershell
dotnet build
```

---

## Aplicar Migrations

```powershell
dotnet ef database update `
  --project src/FCG.Catalog.Infrastructure `
  --startup-project src/FCG.Catalog.Api `
  --context FCGCatalogDbContext
```

---

## Executar

```powershell
dotnet run --project src/FCG.Catalog.Api
```

O console deverá apresentar informações semelhantes a:

```text
Configured endpoint catalog-payment-processed-event
Bus started
Application started
```

A porta local é definida pelo `launchSettings.json`.

No ambiente Docker, a API é disponibilizada em:

```text
http://localhost:8081
```

---

# Docker

A CatalogAPI deve ser executada juntamente com SQL Server, RabbitMQ e os demais microsserviços através do repositório de orquestração.

---

## Criar a Imagem

Na raiz do repositório:

```powershell
docker build -t fcg-catalog-api:1.0 .
```

---

## Executar Isoladamente

```powershell
docker run --rm `
  --name fcg-catalog-api `
  -p 8081:8081 `
  -e ConnectionStrings__DefaultConnection="<CATALOG_CONNECTION_STRING>" `
  -e Jwt__Key="<JWT_KEY>" `
  -e Jwt__Issuer="FCG.Users.Api" `
  -e Jwt__Audience="FCG.Client" `
  -e RabbitMq__Host="<RABBITMQ_HOST>" `
  -e RabbitMq__Username="<RABBITMQ_USER>" `
  -e RabbitMq__Password="<RABBITMQ_PASSWORD>" `
  fcg-catalog-api:1.0
```

A execução isolada exige que o container consiga acessar SQL Server e RabbitMQ.

---

## Executar com Docker Compose

```powershell
cd D:\FIAP-FCG-MICROSERVICOS\FCG-Orchestration-Api
docker compose config
docker compose build
docker compose up -d
docker compose ps
```

URL da aplicação:

```text
http://localhost:8081
```

---

## Health Check

```powershell
curl http://localhost:8081/health
```

---

## Logs

```powershell
docker compose logs -f catalog-api
```

Para acompanhar o fluxo de compra:

```powershell
docker compose logs -f --tail=0 catalog-api payments-api notifications-api
```

---

## Parar o Ambiente

```powershell
docker compose down
```

Para remover também os volumes:

```powershell
docker compose down -v
```

O parâmetro `-v` remove os dados persistidos.

---

# Kubernetes

Os manifestos ficam centralizados no repositório:

```text
FCG-Orchestration-Api
```

Docker Compose e Kubernetes são alternativas de execução. Não é necessário usar ambos ao mesmo tempo.

---

## Aplicar os Manifestos

```powershell
cd D:\FIAP-FCG-MICROSERVICOS\FCG-Orchestration-Api
kubectl apply -f k8s/
```

---

## Verificar os Recursos

```powershell
kubectl get pods
kubectl get deployments
kubectl get services
kubectl get configmaps
kubectl get secrets
```

---

## Acompanhar os Pods

```powershell
kubectl get pods -w
```

---

## Consultar Logs

```powershell
kubectl logs deployment/catalog-api
```

Para acompanhar continuamente:

```powershell
kubectl logs -f deployment/catalog-api
```

---

## Reiniciar o Deployment

```powershell
kubectl rollout restart deployment catalog-api
```

---

## Acompanhar o Reinício

```powershell
kubectl rollout status deployment/catalog-api
```

---

## Remover o Ambiente

```powershell
kubectl delete -f k8s/
```

---

## ConfigMap

Configurações não sensíveis:

```text
ASPNETCORE_ENVIRONMENT
RabbitMq__Host
Jwt__Issuer
Jwt__Audience
```

---

## Secret

Configurações sensíveis:

```text
RabbitMq__Username
RabbitMq__Password
Jwt__Key
CatalogConnectionString
MSSQL_SA_PASSWORD
```

Não versione valores reais.

---

# Swagger

A CatalogAPI possui suporte a Swagger e autenticação JWT.

O esquema de segurança configurado é do tipo Bearer:

```text
Bearer JWT
```

O Swagger é habilitado apenas em ambiente `Development`.

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

---

## Acesso

Em ambiente local, utilize a URL exibida no console.

Quando executado em desenvolvimento na porta 8081:

```text
http://localhost:8081/swagger
```

---

## Autenticar no Swagger

1. Faça login na UsersAPI.
2. Copie o `accessToken`.
3. Abra o Swagger da CatalogAPI.
4. Clique em **Authorize**.
5. Informe o token JWT.
6. Confirme a autorização.
7. Execute os endpoints protegidos.

---

## Swagger no Docker

Se o Dockerfile utilizar:

```text
ASPNETCORE_ENVIRONMENT=Production
```

o Swagger não será exibido, pois a configuração atual o habilita somente em desenvolvimento.

Para validar o container, utilize:

```powershell
curl http://localhost:8081/health
curl http://localhost:8081/api/games
```
# Logs Esperados

A CatalogAPI utiliza `ILogger` para registrar as principais etapas do gerenciamento do catálogo e, principalmente, do fluxo assíncrono de compra.

Os logs permitem acompanhar:

* início de uma compra;
* validação do jogo;
* publicação do `OrderPlacedEvent`;
* recebimento do `PaymentProcessedEvent`;
* conclusão da compra;
* inclusão do jogo na biblioteca;
* prevenção de duplicidade;
* pagamento rejeitado;
* erros de processamento.

---

## Início da Compra

Quando um usuário autenticado solicita a compra de um jogo, o `PurchaseGameCommandHandler` registra:

```text
Compra iniciada.
UserId: ...
GameId: ...
```

Esse log confirma que:

* o token foi aceito;
* o `UserId` foi obtido corretamente;
* o comando chegou ao Handler;
* o fluxo de compra foi iniciado.

---

## Jogo Inexistente

Caso o jogo informado não seja encontrado:

```text
Compra rejeitada: jogo inexistente.
UserId: ...
GameId: ...
```

A API retorna:

```http
404 Not Found
```

---

## Jogo Inativo

Caso o jogo exista, mas esteja desativado:

```text
Compra rejeitada: jogo inativo.
UserId: ...
GameId: ...
```

A API retorna:

```http
409 Conflict
```

Esse comportamento depende do mapeamento de `InvalidOperationException` para `Conflict` no middleware global.

---

## Publicação do OrderPlacedEvent

Após validar o jogo, a CatalogAPI publica o evento:

```text
Evento OrderPlacedEvent publicado.
OrderId: ...
UserId: ...
GameId: ...
Price: ...
```

Esse log confirma que o pedido foi criado e encaminhado para o RabbitMQ.

A resposta HTTP retorna o status inicial:

```text
Pending
```

---

## Recebimento do PaymentProcessedEvent

Após o processamento na PaymentsAPI, a CatalogAPI recebe:

```text
PaymentProcessedEvent recebido.
OrderId: ...
UserId: ...
GameId: ...
Price: ...
Status: ...
ProcessedAt: ...
```

Esse log confirma que:

* a PaymentsAPI publicou o resultado;
* o RabbitMQ entregou a mensagem;
* o Consumer iniciou a conclusão da compra.

---

## Pagamento Aprovado

Quando o status recebido é `Approved`:

```text
Pagamento aprovado.
OrderId: ...
UserId: ...
GameId: ...
```

Em seguida, o Handler verifica se o usuário já possui o jogo.

---

## Jogo Adicionado à Biblioteca

Quando a compra ainda não existe:

```text
Jogo adicionado à biblioteca.
OrderId: ...
UserId: ...
GameId: ...
UserLibraryId: ...
```

O Consumer registra também:

```text
PaymentProcessedEvent processado com sucesso.
Jogo adicionado à biblioteca.
OrderId: ...
UserId: ...
GameId: ...
```

---

## Compra Já Existente

Quando o usuário já possui o jogo:

```text
Compra já existente.
OrderId: ...
UserId: ...
GameId: ...
```

O Consumer registra:

```text
PaymentProcessedEvent processado com sucesso.
Compra já existente e nenhuma duplicidade foi criada.
OrderId: ...
UserId: ...
GameId: ...
```

Esse comportamento evita registros duplicados na biblioteca.

---

## Pagamento Rejeitado

Quando o status recebido não é `Approved`:

```text
Pagamento rejeitado.
OrderId: ...
UserId: ...
GameId: ...
Status: Rejected
```

O Consumer registra:

```text
PaymentProcessedEvent processado.
Pagamento rejeitado e o jogo não foi adicionado.
OrderId: ...
UserId: ...
GameId: ...
```

Nesse cenário, nenhuma alteração é realizada na biblioteca do usuário.

---

## Consultando os Logs

### Docker Compose

```powershell
docker compose logs -f catalog-api
```

Para acompanhar todo o fluxo de compra:

```powershell
docker compose logs -f --tail=0 catalog-api payments-api notifications-api
```

---

### Kubernetes

```powershell
kubectl logs deployment/catalog-api
```

Para acompanhar continuamente:

```powershell
kubectl logs -f deployment/catalog-api
```

---

# Troubleshooting

## Jwt:Key não configurada

Sintoma:

```text
A configuração Jwt:Key não foi encontrada.
```

Confirme a variável:

```text
Jwt__Key
```

Exemplo no PowerShell:

```powershell
$env:Jwt__Key = "<JWT_KEY>"
```

A chave precisa ser igual à utilizada pela UsersAPI.

---

## Jwt:Issuer ou Jwt:Audience inválidos

Sintomas:

* token rejeitado;
* resposta `401 Unauthorized`;
* falha mesmo utilizando token válido.

Confirme:

```text
Jwt__Issuer
Jwt__Audience
```

Os valores precisam ser os mesmos utilizados na geração do token pela UsersAPI.

---

## Erro 401 Unauthorized

Possíveis causas:

* token ausente;
* token expirado;
* token inválido;
* chave JWT diferente;
* issuer incompatível;
* audience incompatível;
* claim de usuário ausente.

Valide o token através do endpoint:

```http
GET /api/auth/me
```

na UsersAPI.

---

## Erro 403 Forbidden

Ocorre quando um usuário autenticado sem role `Admin` tenta executar operações administrativas.

Exemplos:

```text
POST /api/games
PUT /api/games/{id}
DELETE /api/games/{id}
```

Para essas operações, utilize um token com:

```text
Role = Admin
```

---

## Erro 404 Not Found

Pode ocorrer quando:

* o jogo não existe;
* o identificador está incorreto;
* o banco consultado não é o esperado.

Valide:

```powershell
curl http://localhost:8081/api/games
```

---

## Erro 409 Conflict

Ocorre ao tentar comprar um jogo inativo.

Confirme se o middleware contém:

```csharp
InvalidOperationException => HttpStatusCode.Conflict
```

Sem esse mapeamento, a aplicação poderá retornar `500`.

---

## RabbitMQ indisponível

Sintomas:

* erro `Broker Unreachable`;
* aplicação não inicia corretamente;
* evento não é publicado;
* Consumer não recebe mensagens.

Verifique:

```powershell
docker compose ps
docker compose logs rabbitmq
```

Confirme:

```text
RabbitMq__Host
RabbitMq__Username
RabbitMq__Password
```

No Docker Compose, o host deve ser o nome do serviço RabbitMQ, e não `localhost`.

---

## SQL Server indisponível

Sintomas:

* erro ao consultar jogos;
* falha ao criar ou atualizar registros;
* exceções do Entity Framework Core.

Verifique:

```powershell
docker compose logs sqlserver
```

Confirme:

* connection string;
* senha;
* banco correto;
* porta 1433;
* `TrustServerCertificate=True`.

---

## Migration não aplicada

Sintomas:

* tabela inexistente;
* erro ao consultar `Games`;
* erro ao consultar `UserLibrary`.

Execute:

```powershell
dotnet ef database update `
  --project src/FCG.Catalog.Infrastructure `
  --startup-project src/FCG.Catalog.Api `
  --context FCGCatalogDbContext
```

---

## Evento de Compra não Publicado

Se a PaymentsAPI não receber o pedido:

1. verifique os logs da CatalogAPI;
2. confirme a mensagem `Evento OrderPlacedEvent publicado`;
3. consulte o RabbitMQ Management;
4. verifique exchanges e filas;
5. confirme a compatibilidade do contrato.

---

## PaymentProcessedEvent não Consumido

Verifique:

```powershell
docker compose logs catalog-api
```

Confirme se a fila existe:

```text
catalog-payment-processed-event
```

Também confirme:

* Consumer conectado;
* contrato compatível;
* PaymentsAPI publicou o evento;
* fila de erro não contém mensagens.

---

## Jogo não é Adicionado à Biblioteca

Verifique:

1. se o pagamento foi aprovado;
2. se o evento foi recebido;
3. se o `CompletePurchaseCommand` foi executado;
4. se o jogo já existia na biblioteca;
5. se houve erro no SQL Server;
6. se a migration da tabela `UserLibrary` foi aplicada.

---

## Compra Duplicada

A aplicação verifica a existência pelo conjunto:

```text
UserId + GameId
```

Se o mesmo usuário tentar adquirir o mesmo jogo novamente, nenhum novo registro será criado.

O log esperado é:

```text
Compra já existente.
```

---

## Swagger não abre no Docker

Se o ambiente estiver configurado como:

```text
ASPNETCORE_ENVIRONMENT=Production
```

o Swagger não será habilitado.

Valide a aplicação através de:

```powershell
curl http://localhost:8081/health
curl http://localhost:8081/api/games
```

---

## Porta 8081 em uso

Localize o processo:

```powershell
netstat -ano | findstr :8081
```

Encerre:

```powershell
taskkill /PID <PID> /F
```

Ou altere o mapeamento externo no Docker Compose.

---

## Banco antigo em volume

Para parar sem remover os dados:

```powershell
docker compose down
```

Para remover também os volumes:

```powershell
docker compose down -v
```

Atenção: o parâmetro `-v` remove os dados persistidos do SQL Server e RabbitMQ.

---

## ImagePullBackOff no Kubernetes

Possíveis causas:

* imagem não criada;
* nome da imagem incorreto;
* tag incompatível;
* imagem indisponível para o cluster;
* política de pull inadequada.

Verifique:

```powershell
kubectl describe pod <POD_NAME>
```

Se estiver utilizando cluster local, confirme se a imagem está disponível no ambiente utilizado pelo Kubernetes.

---

# Checklist de Validação

## Infraestrutura

* [ ] SQL Server em execução.
* [ ] RabbitMQ em execução.
* [ ] Banco da CatalogAPI criado.
* [ ] Migrations aplicadas.
* [ ] Credenciais configuradas.
* [ ] Configurações JWT compatíveis com a UsersAPI.

---

## Aplicação

* [ ] Projeto compila sem erros.
* [ ] CatalogAPI inicia corretamente.
* [ ] Health check retorna `Healthy`.
* [ ] Swagger disponível em ambiente `Development`.
* [ ] Middleware global está ativo.

---

## Catálogo Público

* [ ] `GET /api/games` retorna os jogos.
* [ ] `GET /api/games/{id}` retorna um jogo.
* [ ] Jogo inexistente retorna `404`.

---

## Operações Administrativas

* [ ] Admin consegue criar jogo.
* [ ] Admin consegue atualizar jogo.
* [ ] Admin consegue desativar jogo.
* [ ] Usuário comum recebe `403`.
* [ ] Requisição sem token recebe `401`.

---

## Compra

* [ ] Usuário autenticado consegue iniciar compra.
* [ ] `UserId` é obtido do JWT.
* [ ] Endpoint não exige body.
* [ ] Compra retorna `202 Accepted`.
* [ ] Resposta retorna status `Pending`.
* [ ] Jogo inexistente retorna `404`.
* [ ] Jogo inativo retorna `409`.
* [ ] `OrderPlacedEvent` é publicado.

---

## Processamento Assíncrono

* [ ] PaymentsAPI recebe `OrderPlacedEvent`.
* [ ] CatalogAPI recebe `PaymentProcessedEvent`.
* [ ] Pagamento aprovado adiciona o jogo.
* [ ] Pagamento rejeitado não altera a biblioteca.
* [ ] Compra duplicada não cria novo registro.

---

## Persistência

* [ ] Jogos são persistidos.
* [ ] Atualizações são persistidas.
* [ ] Exclusão é lógica.
* [ ] Biblioteca do usuário é persistida.
* [ ] Registros duplicados são evitados.

---

## Docker

* [ ] Imagem criada com sucesso.
* [ ] Container inicia corretamente.
* [ ] Porta 8081 está acessível.
* [ ] Variáveis de ambiente foram carregadas.
* [ ] Logs não apresentam falhas.

---

## Kubernetes

* [ ] Deployment está disponível.
* [ ] Pod está `Running`.
* [ ] Service foi criado.
* [ ] ConfigMap foi aplicado.
* [ ] Secret foi aplicado.
* [ ] Logs do deployment estão acessíveis.
* [ ] Rollout concluído com sucesso.

---

# Conclusão

A **FCG Catalog API** é o microsserviço responsável pelo gerenciamento do catálogo e pela coordenação do fluxo de compra da plataforma **FIAP Cloud Games**.

A solução combina:

* endpoints REST;
* autenticação JWT;
* autorização por roles;
* CQRS;
* MediatR;
* Entity Framework Core;
* SQL Server;
* RabbitMQ;
* MassTransit;
* Docker;
* Kubernetes.

No fluxo implementado, a CatalogAPI:

* disponibiliza consultas públicas do catálogo;
* restringe alterações administrativas à role `Admin`;
* realiza exclusão lógica dos jogos;
* obtém o usuário autenticado diretamente do JWT;
* publica o `OrderPlacedEvent`;
* recebe o `PaymentProcessedEvent`;
* adiciona jogos à biblioteca após pagamento aprovado;
* rejeita conclusões de pagamentos não aprovados;
* evita duplicidade por usuário e jogo.

A comunicação orientada a eventos mantém o serviço desacoplado da PaymentsAPI e da NotificationsAPI, permitindo que cada microsserviço evolua de forma independente.

A separação entre Commands, Queries, Handlers, Consumers, entidades e repositórios mantém a arquitetura organizada e aderente aos princípios aplicados em toda a solução.

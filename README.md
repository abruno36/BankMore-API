# BankMore - Banco Digital

API bancária com cadastro, autenticação, movimentações e consulta de saldo.

## 🚀 Tecnologias
- **.NET 8.0** - Framework principal
- **SQLite** - Banco de dados leve
- **JWT Authentication** - Autenticação segura
- **Docker + Docker Compose** - Containerização
- **Swagger/OpenAPI** - Documentação automática

## 🚀 ARQUITETURA DE MICROSSERVIÇOS

### 📍 ENDPOINTS PRINCIPAIS

#### 1. 🏢 BANKMORE.API (API Principal)
**Porta:** 5294  
**Swagger:** http://localhost:5294/swagger

**Funcionalidades:**
- ✅ Autenticação JWT
- ✅ Cadastro seguro de usuários (CPF criptografado)
- ✅ Gerenciamento de contas correntes
- ✅ Inativação/reativação de contas
- ✅ Consulta de saldo e extrato

#### 2. 💸 TRANSFERENCIA.API (Microsserviço)
**Porta:** 5134  
**Swagger:** http://localhost:5134/swagger

**Funcionalidades:**
- ✅ Transferências entre contas
- ✅ Idempotência nativa (Idempotency-Key)
- ✅ API Key obrigatória para segurança
- ✅ Registro automático de movimentações

#### 3. 📊 CONTACORRENTE.API (Microsserviço - opcional)
**Porta:** [definir]  
**Swagger:** http://localhost:[porta]/swagger

---

## 🔐 SEGURANÇA E RESILIÊNCIA

#### Headers Obrigatórios para Transferências:

| Header            | Obrigatório     | Descrição                        | Exemplo                         |
|-------------------|-----------------|----------------------------------|---------------------------------|
| `X-API-Key`       | ✅ **SIM**      | Autenticação do microsserviço    | `BankMore-Transfer-2024-Secure` |
| `Content-Type`    | ✅ **SIM**      | Tipo do conteúdo                 | `application/json`              |
| `Idempotency-Key` | ⚠️ **OPCIONAL** | Idempotência (evita duplicações) | `transfer-123-unique`           |

#### Exemplo Completo de Request:
```bash
curl -X POST http://localhost:5134/api/transferencia \
  -H "Content-Type: application/json" \
  -H "X-API-Key: BankMore-Transfer-2024-Secure" \
  -H "Idempotency-Key: minha-transferencia-unica-001" \
  -d '{
    "NumeroContaDestino": "000002",
    "Valor": 100.00,
    "Descricao": "Pagamento mensal"
  }'


4. 🔐 SEGURANÇA IMPLEMENTADA

### Criptografia de Dados Sensíveis
- CPF armazenado criptografado no banco
- Hash BCrypt para senhas
- Chaves de criptografia em ambiente seguro

### Autenticação e Autorização
- JWT tokens com expiração
- API Key para microsserviços
- Middleware de validação customizado

### Idempotência
- Header `Idempotency-Key` em transferências
- Evita processamento duplicado
- Retorno da transação original em caso de retry

---

### 1. Criptografia do CPF
**Problema**: CPF é dado sensível que não pode vazar
**Solução**: Criptografia AES-256 no banco
**Implementação**: `CryptoService.Encrypt()/Decrypt()`

### 2. Proteção de Senha
**Problema**: Senhas em texto claro são risco crítico
**Solução**: Hash BCrypt com salt
**Implementação**: `BCrypt.Net.BCrypt.HashPassword()/Verify()`

### 3. Autenticação
**Problema**: Acesso não autorizado a endpoints
**Solução**: JWT com validação automática
**Implementação**: `[Authorize]` attribute + JWT middleware

### 4. Validações de Negócio
- Conta deve estar ativa para operações
- Saldo suficiente para saques
- Apenas créditos para terceiros
- Valores positivos apenas

### 5. Como executar via DOCKER
- Parar container
  - docker-compose down -v
- Construa a imagem (primeira vez ou após alterações)
  - docker-compose build --no-cache
- Subir o Container
  - docker-compose up
### 📊 Banco de Dados SQLite

#### Localização:
- **Desenvolvimento local:** `src/BankMore.API/bankmore.db`
- **Docker (padrão):** `/app/data/bankmore.db`

#### Para usar o mesmo banco em local e Docker:

5. 🔐 SEGURANÇA IMPLEMENTADA

## 📊 Modelo de Dados

### Tabelas: ContasCorrente e Movimentos
```sql
CREATE TABLE ContasCorrente (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    NumeroConta TEXT NOT NULL UNIQUE,
    CpfCriptografado TEXT NOT NULL UNIQUE,
    SenhaHash TEXT NOT NULL,
    NomeTitular TEXT NOT NULL,
    Ativo BOOLEAN NOT NULL DEFAULT 1,
    DataCriacao DATETIME NOT NULL
);

CREATE TABLE Movimentos (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Tipo TEXT NOT NULL,  -- 'C' (Crédito) ou 'D' (Débito)
    Valor DECIMAL(18,2) NOT NULL,
    DataMovimento DATETIME NOT NULL,
    Descricao TEXT,
    ContaCorrenteId INTEGER NOT NULL,
    FOREIGN KEY (ContaCorrenteId) REFERENCES ContasCorrente(Id)
);

CREATE TABLE Movimentacoes (
    Id TEXT PRIMARY KEY,
    ContaId TEXT NOT NULL,
    Tipo TEXT NOT NULL,  -- 'C' (Crédito) ou 'D' (Débito)
    Valor DECIMAL(18,2) NOT NULL,
    DataMovimentacao DATETIME NOT NULL,
    Descricao TEXT,
    FOREIGN KEY (ContaId) REFERENCES ContasCorrentes(Id)
);

CREATE TABLE IdempotencyKeys (
    Id TEXT PRIMARY KEY,
    RequisicaoId TEXT NOT NULL UNIQUE,
    DataCriacao DATETIME NOT NULL,
    DataExpiracao DATETIME NOT NULL
);

CREATE TABLE VersionInfo (
    Version BIGINT NOT NULL PRIMARY KEY,
    AppliedOn DATETIME,
    Description TEXT
);

## 🐳 DOCKER COMPOSE

```bash
# Subir todos os serviços
docker-compose up -d

# Verificar logs
docker-compose logs -f

# Parar serviços
docker-compose down

6. 📁 ESTRUTURA DO PROJETO

- **BankMore.API/** - API principal com autenticação, cadastro e gerenciamento de contas
- **BankMore.Transferencia.API/** - Microsserviço especializado em transferências  
- **BankMore.Shared/** - Classes compartilhadas (DTOs, interfaces, modelos)
- **BankMore.Infrastructure/** - Serviços, repositórios e configurações de infraestrutura
- **docker-compose.yml** - Configuração para orquestração de containers

BankMore/
├── docs/ 
│ ├── BankMore.postman_collection.json
│ └── BankMore Local.postman_environment.json
├── BankMore.API/                 # API Principal
├── BankMore.Transferencia.API/   # Microsserviço
├── BankMore.Shared/              # DTOs e Interfaces
├── BankMore.Infrastructure/      # Serviços e Data
└── docker-compose.yml

7. 🧪 TESTES E COLEÇÕES POSTMAN

### Coleções Disponíveis:
1. **`BankMore.postman_collection.json`** - Coleção completa com todos os endpoints
2. **`BankMore Local.postman_environment.json`** - Ambiente de desenvolvimento local

### Como importar:
1. Abra o Postman
2. Clique em **Import** → **Upload Files**
3. Selecione ambos os arquivos JSON
4. Selecione o ambiente **"BankMore Local"**

### Endpoints na Collection:
- ✅ **Auth** - Login, registro, validação
- ✅ **Contas** - Criar, consultar, inativar
- ✅ **Transferências** - Com API Key e Idempotência
- ✅ **Movimentações** - Extrato e histórico

### Variáveis de Ambiente Configuradas:
```json
{
  "base_url": "http://localhost:5294",
  "transferencia_url": "http://localhost:5134",
  "api_key": "BankMore-Transfer-2024-Secure",
  "token": "{{automaticamente gerado}}"
}
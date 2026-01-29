# BankMore - Banco Digital

API bancária com cadastro, autenticação, movimentações e consulta de saldo.

## 🚀 Tecnologias
- .NET 8.0
- Entity Framework Core + SQLite
- JWT Authentication
- Docker + Docker Compose
- Swagger/OpenAPI

## 🔧 Passos de Desenvolvimento

### Fase 1: Configuração Inicial
1. Criar solução com 3 projetos (API, Domain, Infrastructure)
2. Configurar Entity Framework com SQLite
3. Implementar autenticação JWT
4. Configurar Swagger com autenticação

### Fase 2: Entidades e Banco de Dados
1. Criar entidade `ContaCorrente` com:
   - CPF criptografado
   - Senha com hash BCrypt
   - Número da conta único
2. Criar entidade `Movimento` para transações
3. Configurar índices e relações no DbContext

### Fase 3: Endpoints da API
1. **POST /api/Conta/cadastrar**
   - Validação de CPF (11 dígitos)
   - Criptografia do CPF com AES
   - Hash da senha com BCrypt
   - Geração de número de conta aleatório

2. **POST /api/Conta/login**
   - Aceita número da conta ou CPF
   - Descriptografa CPF para comparação
   - Verifica senha com BCrypt
   - Retorna token JWT com claims

3. **POST /api/Movimentacao**
   - Validações de segurança:
     - Conta existe e está ativa
     - Valor positivo
     - Tipo válido (C/D)
     - Saldo suficiente para débitos
     - Apenas crédito para contas diferentes
   - Persiste movimento no banco

4. **GET /api/Conta/saldo**
   - Calcula saldo: Σ(Créditos) - Σ(Débitos)
   - Traz para memória para compatibilidade SQLite

### Fase 4: Segurança
1. **Criptografia do CPF**:
   - AES-256 com chave de 32 bytes
   - IV fixo para simplicidade (em produção usar IV único)

2. **Hash da Senha**:
   - BCrypt com salt automático
   - Resistente a ataques rainbow table

3. **JWT Authentication**:
   - Tokens com claims de contaId, numeroConta, cpf
   - Validação automática via middleware
   - Proteção em todos endpoints (exceto cadastro/login)

### Fase 5: Containerização
1. **Dockerfile**:
   - Multi-stage build
   - Imagem base otimizada (aspnet:8.0)
   - Volume para dados persistentes

2. **Docker Compose**:
   - Serviço único (API + SQLite)
   - Mapeamento de porta 5000:8080
   - Volume para persistência do banco

## 🛡️ Decisões de Segurança

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
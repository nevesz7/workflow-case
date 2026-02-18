# Sistema de Workflow de Solicitações

Este projeto é um sistema simples para gerenciar solicitações internas, construído como uma aplicação full-stack. Ele permite que usuários criem solicitações e que gestores as aprovem ou reprovem, mantendo um histórico completo para auditoria.

## Funcionalidades

- **Autenticação:** Login seguro utilizando JWT.
- **Autorização:** Controle de acesso baseado em papéis (User vs. Manager).
- **Gerenciamento de Workflow:** Solicitações seguem um fluxo de estados claro (Pendente -> Aprovado/Reprovado).
- **Trilha de Auditoria:** Todas as mudanças de status são registradas em uma linha do tempo histórica.
- **Filtragem:** A lista de solicitações pode ser filtrada por status.

## Tecnologias Utilizadas

- **Backend:** .NET 6+ (Web API)
- **Frontend:** Angular 16+
- **Banco de Dados:** PostgreSQL

---

## Pré-requisitos

- **.NET SDK:** Versão 6.0 ou superior.
- **Node.js:** Versão 16.x ou 18.x (inclui o npm).
- **Editor de Código:** VS Code ou sua IDE de preferência.
- **Git:** Para controle de versão.

---

## Configuração do Ambiente

Esta seção explica como configurar as variáveis de ambiente para conectar o frontend, o backend e o banco de dados, especialmente ao rodar o projeto em uma nova máquina.

### 1. Configurar a Conexão do Banco de Dados (Backend)

O backend usa um arquivo `.env` para gerenciar a string de conexão do banco de dados.

**a. Gere o arquivo `.env`:**
Na raiz do projeto, execute o script de setup:
```bash
chmod +x setup-env.sh
./setup-env.sh
```
Isso criará um arquivo `.env` dentro de `backend/Workflow.Api/` com a string de conexão padrão.

**b. Edite o `.env` (se necessário):**
Se o seu banco de dados não estiver rodando em `localhost:5432` ou se as credenciais forem diferentes, edite o arquivo `backend/Workflow.Api/.env`.

### 2. Configurar a URL da API (Frontend)

O frontend precisa saber o endereço de rede onde o backend está rodando. Esta configuração é gerenciada pelos arquivos de ambiente do Angular, localizados em `frontend/src/environments/`.

**a. Para Desenvolvimento Local:**
Edite o arquivo `frontend/src/environments/environment.development.ts`.

Por padrão, ele aponta para `localhost`. Se o seu backend estiver rodando em outra máquina na mesma rede, você precisará encontrar o endereço IP daquela máquina e atualizar a URL.

- **Para encontrar o IP (na máquina do backend):**
  - **Windows:** `ipconfig` (procure por "Endereço IPv4")
  - **Linux/macOS:** `ip addr` (procure por "inet")

- **Exemplo de atualização:**
  ```typescript
  export const environment = {
    production: false,
    // Substitua 'localhost' pelo IP da máquina do backend, se necessário
    apiUrl: 'http://192.168.1.15:5166/api' 
  };
  ```

**b. Para Produção:**
Quando for fazer o deploy da aplicação, edite o arquivo `frontend/src/environments/environment.ts` com o domínio ou IP do seu servidor de produção.
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://sua-api-em-producao.com/api'
};
```

---

## Opção 1: Ambiente Windows Puro

Esta configuração executa o banco de dados, o backend e o frontend diretamente no Windows.

### 1. Instalar o PostgreSQL no Windows

- Baixe e instale o PostgreSQL para Windows.
- Durante a instalação, será solicitado que você defina uma senha para o usuário padrão `postgres`.
- Usando uma ferramenta como o `pgAdmin` (que vem com o instalador) ou `DBeaver`, crie:
  - Um novo banco de dados chamado `workflow_db`.
  - Um novo usuário (Login/Group Role) chamado `admin` com a senha `password123`.
  - Conceda ao usuário `admin` todos os privilégios sobre o banco de dados `workflow_db`.

### 2. Executar o Backend

- Abra um terminal (PowerShell ou Command Prompt).
- Navegue até o diretório do backend: `cd backend/Workflow.Api`
- Execute a aplicação: `dotnet run`
- A API será iniciada, criará as tabelas necessárias e populará o banco com dados de teste. Anote a porta em que ela está rodando (ex: `http://localhost:5166`).

### 3. Executar o Frontend

- Abra um **novo** terminal.
- Navegue até o diretório do frontend: `cd frontend`
- Instale as dependências: `npm install`
- Execute a aplicação: `npm start`
- O frontend estará disponível em **http://localhost:4200**.

---

## Opção 2: Ambiente Windows + WSL (com Docker)

Esta configuração é ideal para desenvolvedores que preferem executar serviços como bancos de dados em um ambiente Linux usando Docker.

### 1. Pré-requisitos para WSL

- WSL2 instalado no Windows.
- Docker Desktop instalado, com a integração WSL2 habilitada.

### 2. Executar o Banco de Dados no Docker

- O arquivo `docker-compose.yml` deve estar localizado na pasta `infra` do projeto. Exemplo:
  ```yaml
  version: '3.8'
  services:
    db:
      image: postgres:14
      restart: always
      environment:
        - POSTGRES_DB=workflow_db
        - POSTGRES_USER=admin
        - POSTGRES_PASSWORD=password123
      ports:
        - "5432:5432"
      volumes:
        - workflow_db_data:/var/lib/postgresql/data
  volumes:
    workflow_db_data:
  ```
- Abra um terminal WSL na pasta infra do projeto.
- Execute o comando: `docker-compose up -d`
- Isso iniciará um contêiner PostgreSQL. O banco de dados estará acessível a partir do Windows em `localhost:5432`.

### 3. Executar Backend e Frontend no Windows

- Siga exatamente os mesmos passos da **"Opção 1: Ambiente Windows Puro"** para executar o backend e o frontend. A API .NET rodando no Windows se conectará a `localhost:5432`, que o Docker encaminha para o contêiner no WSL.

---

## Uso e Demonstração

O banco de dados é populado com os seguintes usuários. Você pode usá-los para testar o fluxo da aplicação.

- **Usuários (Users):**
  - `User1` / `HashedPassword1word`
  - `User2` / `HashedPassword2test`
  - `User3` / `HashedPassword3user`
- **Gestores (Managers):**
  - `Manager1` / `AdminPassword1deal`
  - `Manager2` / `AdminPassword2maker`
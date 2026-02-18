#!/bin/bash
# Script para gerar um arquivo .env padrão para o backend.

# Navega para a pasta da API do backend
cd backend/Workflow.Api

# Define o caminho do arquivo .env
ENV_FILE=".env"

# Verifica se o arquivo .env já existe
if [ -f "$ENV_FILE" ]; then
    echo "$ENV_FILE já existe. Nenhuma alteração foi feita."
else
    echo "Criando arquivo $ENV_FILE padrão..."
    # Cria o arquivo .env com os valores padrão
    cat > $ENV_FILE << EOL
# Configurações de Conexão do PostgreSQL
# Esta variável sobrescreve a que está em appsettings.json
ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=workflow_db;Username=admin;Password=password123"
EOL
    echo "$ENV_FILE criado com sucesso em backend/Workflow.Api/"
fi
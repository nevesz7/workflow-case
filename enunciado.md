

1. Contexto 
A empresa precisa de um sistema simples para registrar solicitações internas (ex.: compras, 
acessos, reembolsos, demandas de TI) e permitir que gestores aprovem ou reprovem essas 
solicitações. 
O objetivo é entregar um MVP funcional, com autenticação e autorização, e um fluxo claro de estados 
(workflow). 
  
 
Este desafio será implementado end-to-end: 
• Front-end: Angular 
• Back-end: .NET (Web API) 
• Banco de Dados: relacional (à escolha do time — ex.: SQL Server/Postgres/MySQL) 
 
2. Objetivo do desafio 
Construir uma aplicação web com: 
• Login com JWT 
• Controle de acesso por roles 
• Criação de solicitações (User) 
• Aprovação/Reprovação (Manager) 
• Histórico de status / auditoria 
• Listagem com filtro por status, incluindo aprovadas e rejeitadas 
 
3. Usuários e permissões (sem tela de gestão) 
Não é necessário criar tela de administração/gestão de usuários. 
Os usuários serão criados “na mão” no banco (via seed, script SQL ou migration), já com suas roles 
definidas. 
Para o MVP, devem existir pelo menos dois usuários: 
• User (Solicitante): cria solicitações e vê as próprias 
• Manager (Aprovador): aprova/reprova e consegue ver solicitações (conforme regras abaixo) 
O foco é: um usuário cria e outro aprova/rejeita. 
Roles 
• User 
• Manager 
 
  
 
4. Requisitos funcionais 
4.1 Autenticação 
• Tela de login. 
• Backend retorna um JWT. 
• JWT deve conter claims relevantes (ex.: userId, role). 
• Front deve armazenar o token e enviá-lo no header Authorization. 
O login pode ser simples (email/senha) validando contra o banco. 
 
 
4.2 Solicitações (Requests) 
Uma solicitação deve conter, no mínimo: 
• Título (obrigatório) 
• Descrição (obrigatório) 
• Categoria (ex.: Compras / TI / Reembolso / Outros) 
• Prioridade (Baixa / Média / Alta) 
• Status 
• Criado por (UserId) 
• Data de criação 
• Última atualização 
Workflow obrigatório 
• Pending (Pendente) – estado inicial ao criar 
• Approved (Aprovada) 
• Rejected (Reprovada) 
Regras: 
• Ao criar, sempre inicia como Pending. 
• Uma solicitação Approved ou Rejected não pode voltar para Pending. 
• Somente Manager pode transicionar Pending → Approved/Rejected. 
  
4.3 Aprovar / Reprovar 
• Manager deve ter ação de Aprovar e Reprovar na solicitação Pending. 
• A ação deve permitir comentário/justificativa: 
o Obrigatório para Reprovar 
o Opcional para Aprovar 
• Ao aprovar/reprovar, registrar: 
o actionBy (UserId do Manager) 
o actionAt (data/hora) 
o comentário 
 
4.4 Histórico de status (Audit / Timeline) 
Toda mudança de status deve gerar registro em histórico, contendo: 
• requestId 
• fromStatus 
• toStatus 
• changedBy 
• changedAt 
• comment (se houver) 
No front, exibir o histórico na tela de detalhe da solicitação como “Histórico / Timeline”. 
 
4.5 Listagem e filtros (incluindo aprovadas/rejeitadas) 
A aplicação deve ter listagem de solicitações com: 
• Filtro por Status: Pending, Approved, Rejected 
• (Opcional) filtro por Categoria e Prioridade 
• (Opcional) busca por texto (título/descrição) 
Regras de visibilidade: 
• User: lista somente as próprias solicitações (em qualquer status: pendentes, aprovadas e 
rejeitadas). 
• Manager: deve conseguir ver solicitações e filtrar por status, incluindo Approved e Rejected. 
o Para simplificar no MVP: Manager pode ver todas as solicitações. 
o (Opcional avançado) restringir para “solicitações do time”. 
 
5. Requisitos não funcionais (mínimos) 
• API REST bem definida. 
• Validações de entrada no backend (e feedback no front). 
• Tratamento de erro (mensagens amigáveis). 
• Organização em camadas (Controllers → Services → Repositories). 
• Persistência via ORM (ex.: Entity Framework Core). 
• Migrations ou script SQL versionado. 
 
6. Telas mínimas no Angular 
1. Login 
2. Listagem de Solicitações 
o filtros por status (ver Approved/Rejected) 
o visualização conforme role 
3. Nova Solicitação (User) 
4. Detalhe da Solicitação 
o dados principais 
o histórico/timeline 
o botões Aprovar/Reprovar (apenas Manager e apenas quando status = Pending) 
 
7. Endpoints sugeridos (exemplo) 
• POST /auth/login 
• GET /requests (com query params para filtros) 
• POST /requests 
• GET /requests/{id} 
• POST /requests/{id}/approve 
• POST /requests/{id}/reject 
• GET /requests/{id}/history 
  
8. Regras de negócio obrigatórias 
• Solicitação sempre começa como Pending. 
• Apenas Pending pode ser aprovada/reprovada. 
• Apenas Manager pode aprovar/reprovar. 
• User só enxerga suas próprias solicitações (incluindo Approved/Rejected). 
• Manager deve conseguir visualizar e filtrar Approved/Rejected. 
• Histórico deve registrar toda mudança de status. 
 
 
9. Entregáveis esperados 
• Repositório com: 
o Front Angular 
o Backend .NET 
o Scripts/migrations do banco (incluindo seed de usuários: pelo menos 1 User e 1 
Manager) 
o README com instruções para rodar 
 
• Demo (5–10 min): 
o Login com User → criar solicitação → ver status 
o Login com Manager → aprovar/reprovar → ver histórico 
o Voltar no User → ver solicitação aprovada/rejeitada + timeline 
 
Apresentação 18/02/2026 15:00  
 
 
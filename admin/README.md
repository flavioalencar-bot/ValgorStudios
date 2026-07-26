# Valgor Admin

Painel administrativo da Valgor Studios, desenvolvido com React, Vite e TypeScript.

## Como executar

```bash
npm install
npm run dev
```

O ambiente local abre em `http://localhost:5173`. Configure a URL da API em
`.env`, usando `.env.example` como referência.

## Autenticação

O login utiliza `POST /api/auth/login` e mantém a sessão JWT no armazenamento
local do navegador. Rotas administrativas exigem uma sessão autenticada.

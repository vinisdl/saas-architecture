# Scripts

## keycloak-setup.mjs

Configura o Keycloak para o projeto SaaS via **Admin REST API**: realm `saas`, clients (saas-frontend, saas-backend), client scope `tenant_claim`, role `user`, service account do backend e opções de realm.

**Requisito:** Node.js 18+ (fetch nativo). Keycloak deve estar rodando.

**Uso:**

```bash
# Com defaults (Keycloak em http://localhost:8080, admin/admin, frontend em http://localhost:3000)
node scripts/keycloak-setup.mjs

# Customizado
KEYCLOAK_URL=http://localhost:8080 KEYCLOAK_ADMIN=admin KEYCLOAK_PASSWORD=admin FRONTEND_URL=http://localhost:3000 node scripts/keycloak-setup.mjs
```

**Variáveis de ambiente:**

| Variável           | Descrição              | Default               |
|--------------------|------------------------|------------------------|
| `KEYCLOAK_URL`     | URL base do Keycloak   | `http://localhost:8080` |
| `KEYCLOAK_ADMIN`   | Usuário admin         | `admin`               |
| `KEYCLOAK_PASSWORD`| Senha admin           | `admin`               |
| `FRONTEND_URL`     | Origem do frontend    | `http://localhost:3000` |

**O que o script cria/ajusta:**

- Realm `saas` (se não existir)
- Client **saas-frontend** (público, redirect URIs e web origins com `FRONTEND_URL`)
- Client **saas-backend** (confidencial, service account) e roles `view-users` e `manage-users` no realm-management
- Client scope **tenant_claim** com mapper do atributo `tenant_id` para o token
- Scope `tenant_claim` como default do client saas-frontend
- Realm role **user**
- User profile: unmanaged attributes habilitado (Keycloak 24+)
- Required action "Update profile" desmarcada como default

**Depois de rodar:** copie o **Client secret** do client `saas-backend` no Keycloak (Clients → saas-backend → Credentials) e configure no backend (`Keycloak:Admin:ClientSecret`).

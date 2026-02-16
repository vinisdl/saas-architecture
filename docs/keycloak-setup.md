# Configuração Keycloak (multi-tenant)

Após subir os containers (`docker compose up -d`), acesse o Keycloak em http://localhost:8080 (admin / admin).

**Importante:** siga a ordem abaixo. Se você tentar fazer login no frontend **antes** de criar o realm e o client, a página do Keycloak pode exibir **"We're sorry"** (realm ou rota inexistente).

## 1. Criar o realm

1. No menu, clique em **Create realm**.
2. Nome: `saas`.
3. Salve.

## 2. Cliente para o frontend

1. Em **Clients**, clique em **Create client**.
2. **Client ID**: `saas-frontend`.
3. **Client type**: `OpenID Connect`.
4. **Client authentication**: OFF (public).
5. Avançar. Em **Capability config**:
   - **Authorization**: OFF.
   - **Authentication flow**: marque **Standard flow** e **Direct access grants** se quiser.
6. Em **Login settings** (ou na tela do client, dependendo da versão):
   - **Root URL**: `http://localhost:3000`.
   - **Valid redirect URIs**: `http://localhost:3000/*` (obrigatório para F5 em rotas como `/dashboard` e callback em `/login`).
   - **Valid post logout redirect URIs**: `http://localhost:3000/*`.
   - **Web origins** (obrigatório para CORS): coloque **`http://localhost:3000`** (exatamente, sem barra no final).  
     Esse campo define quais origens podem chamar o Keycloak (ex.: `/token`) a partir do navegador. Se estiver vazio, você verá erro de CORS ao fazer login.  
     Alternativa em dev: **`+`** (usa as origens dos redirect URIs).
7. Salve.

## 3. Claim `tenant_id` (multi-tenant)

Para o backend identificar o tenant pelo token:

1. No **menu lateral** (mesmo nível que Clients, Users), abra **Client scopes**.
2. Clique em **Create client scope**.
   - **Name**: `tenant_claim`.
   - **Type**: deixe Default (ou Optional, se preferir solicitar no login).
   - Salve.
3. Abra o scope `tenant_claim` e vá na aba **Mappers**.
4. Clique em **Add mapper** → **By configuration** → escolha **User Attribute**:
   - **Name**: `tenant_id`.
   - **User Attribute**: `tenant_id` (atributo que você preencher em cada usuário).
   - **Token Claim Name**: `tenant_id`.
   - Marque **Add to ID token** e **Add to access token**.
   - Salve.
5. Atribua o scope ao client: **Clients** → **saas-frontend** → aba **Client scopes**.
   - Em **Optional client scopes** (ou **Default client scopes**), clique em **Add client scope** e selecione `tenant_claim`.

### Atribuir `tenant_id` a um usuário

No Keycloak 24+ a aba **Attributes** pode não aparecer ao abrir um usuário (perfil de usuário declarativo). Duas opções:

**Opção A – Exibir a aba Attributes**

1. **Realm settings** (menu lateral) → aba **User profile**.
2. Ative **Unmanaged attributes** (permite atributos customizados além do perfil).
3. Salve. Ao editar um usuário (**Users** → clique no usuário), a aba **Attributes** passa a aparecer.
4. Na aba **Attributes**, adicione a chave `tenant_id` e o valor (GUID do tenant ou slug). Salve.

**Opção B – Incluir no perfil (aparece em Details)**

1. **Realm settings** → **User profile** → na lista de atributos, **Create attribute**.
2. **Name**: `tenant_id`, **Display name**: Tenant ID (ou outro). Salve.
3. Em **Users** → usuário → aba **Details** (ou **Attributes**, se existir): preencha o campo **tenant_id** e salve.

## 4. Cliente para a API (opcional)

Se a API validar audience, crie um client **saas-api** (confidential) e configure no **Keycloak:Audience** da API como `saas-api`. Para testes com o realm padrão, o audience `account` já funciona.

## 5. Administração (tenant e usuários)

A área **Administração** do frontend (rota `/admin`) e os endpoints de admin da API (criar/editar tenants, listar usuários, atribuir usuário a tenant) são restritos ao **usuário administrador**.

### 5.1 Usuário administrador

O backend considera administrador o usuário do Keycloak cujo **ID (sub)** é igual ao configurado em `Keycloak:AdminUserId`.

- **Valor usado no projeto**: `0852fb08-0233-4b09-869f-ed55655ba12c`.
- **Configuração**: em `appsettings.json` (ou variáveis de ambiente) defina:
  - `Keycloak:AdminUserId`: ID do usuário no realm `saas` que será admin (ex.: o valor acima).
- Garanta que esse usuário exista no realm: **Users** → criar ou localizar o usuário → o **ID** exibido na URL ou na aba Details é o que deve constar em `AdminUserId`.

Somente esse usuário verá o link "Administração" e poderá acessar `/admin` (tenants e usuários).

### 5.2 Cliente para a API chamar o Keycloak (Admin API)

Para a API listar usuários e definir o atributo `tenant_id` nos usuários, ela usa a **Keycloak Admin REST API**. Para isso é necessário um client confidencial no realm com permissão de administração.

1. Em **Clients**, clique em **Create client**.
2. **Client ID**: `saas-backend`.
3. **Client type**: `OpenID Connect`.
4. **Client authentication**: **ON** (confidential).
5. Avançar. Em **Capability config**, marque **Service accounts roles** (ou **Service accounts** / opção que permita client credentials). Salve.
6. Vá na aba **Credentials** e copie o **Client secret** (configure em `Keycloak:Admin:ClientSecret` no backend).
7. **Permissões da Admin API (evita 403 Forbidden):** abra a aba **Service account roles** (ou **Service account** → **Role mapping**). Clique em **Assign role**.
   - Filtre por **realm roles** (ou selecione o client **realm-management**).
   - Atribua as roles:
     - **view-users** — obrigatório para a API listar usuários (`GET /api/admin/users`).
     - **manage-users** — obrigatório para a API alterar atributos (ex.: atribuir tenant ao usuário).
   - Confirme. Sem essas roles, a API retornará 403 ao acessar a Admin API do Keycloak.

Configure no backend (variáveis de ambiente ou `appsettings.json`):

- `Keycloak:Admin:ServerUrl`: URL do Keycloak (ex.: `http://localhost:8080`).
- `Keycloak:Admin:Realm`: `saas`.
- `Keycloak:Admin:ClientId`: `saas-backend`.
- `Keycloak:Admin:ClientSecret`: o secret copiado do client.

Exemplo em `appsettings.Development.json` (não versionar o secret em produção):

```json
{
  "Keycloak": {
    "Admin": {
      "ServerUrl": "http://localhost:8080",
      "Realm": "saas",
      "ClientId": "saas-backend",
      "ClientSecret": "<secret do client saas-backend>"
    }
  }
}
```

Sem o client **saas-backend** e o secret configurados, a listagem de usuários e a atribuição de tenant na área Administração falharão (a API não conseguirá obter token para a Admin API).

---

## Erro "We're sorry" na página de login

Se ao clicar em "Entrar" no frontend a página do Keycloak mostra **"We're sorry"**:

1. **Realm e client existem?** O frontend usa o realm `saas` e o client `saas-frontend`. Crie o realm (secção 1) e o client (secção 2) **antes** de testar o login. A URL de login é `http://localhost:8080/realms/saas/...` — se o realm não existir, o Keycloak devolve erro.

2. **Keycloak com hostname correto:** No `docker-compose.yml` o Keycloak está com `KC_HOSTNAME=localhost` e `KC_HOSTNAME_PORT=8080` para que todas as URLs geradas usem `http://localhost:8080`. Se você alterou isso ou acessa por outro host/porta, ajuste de volta ou reinicie os containers (`docker compose up -d --force-recreate keycloak`).

3. **Redirect URI:** No client **saas-frontend**, em **Valid redirect URIs** deve constar exatamente a origem do frontend, por exemplo `http://localhost:3000/*` (ou `https://...` se usar HTTPS). Qualquer diferença (porta, protocolo, barra no final) pode causar erro após o login.

4. **Logs do Keycloak:** Para ver o motivo exato do erro, execute `docker logs saas-keycloak` e tente o login de novo. A mensagem no log costuma indicar realm inexistente, redirect_uri inválido ou falha interna.

---

## Erro de CORS ao chamar /token ("No 'Access-Control-Allow-Origin' header")

Se o navegador bloquear a requisição para `http://localhost:8080/realms/saas/protocol/openid-connect/token` com mensagem de CORS:

1. Abra o client **saas-frontend** no Keycloak (**Clients** → **saas-frontend**).
2. Vá na aba **Settings** (ou na tela principal do client).
3. Localize o campo **Web origins**.
4. Preencha com **`http://localhost:3000`** (a origem do frontend, sem barra no final). Salve.
5. Se a interface tiver **"Web origins"** em outra etapa (ex.: "Capability config" ou "Login settings"), preencha no mesmo lugar em que estão Redirect URIs.
6. Tente o login de novo; o Keycloak passará a enviar o header `Access-Control-Allow-Origin: http://localhost:3000` nas respostas do token e o navegador permitirá a requisição.

Se o frontend rodar em outra origem (ex.: `http://localhost:5173`), use essa origem em **Web origins**.

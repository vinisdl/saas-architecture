#!/usr/bin/env node
/**
 * Configura o Keycloak para o projeto SaaS (realm, clients, scope tenant_id, role user).
 * Uso: node scripts/keycloak-setup.mjs
 * Variáveis de ambiente:
 *   KEYCLOAK_URL      - base do Keycloak (default: http://localhost:8080)
 *   KEYCLOAK_ADMIN    - usuário admin (default: admin)
 *   KEYCLOAK_PASSWORD - senha admin (default: admin)
 *   FRONTEND_URL      - origem do frontend (default: http://localhost:3000)
 */

const KEYCLOAK_URL = (process.env.KEYCLOAK_URL || 'http://localhost:8080').replace(/\/$/, '');
const KEYCLOAK_ADMIN = process.env.KEYCLOAK_ADMIN || 'admin';
const KEYCLOAK_PASSWORD = process.env.KEYCLOAK_PASSWORD || 'admin';
const FRONTEND_URL = (process.env.FRONTEND_URL || 'http://localhost:3000').replace(/\/$/, '');
const REALM = 'saas';

async function getAdminToken() {
  const url = `${KEYCLOAK_URL}/realms/master/protocol/openid-connect/token`;
  const body = new URLSearchParams({
    grant_type: 'password',
    client_id: 'admin-cli',
    username: KEYCLOAK_ADMIN,
    password: KEYCLOAK_PASSWORD,
  });
  const res = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: body.toString(),
  });
  if (!res.ok) {
    throw new Error(`Failed to get admin token: ${res.status} ${await res.text()}`);
  }
  const data = await res.json();
  return data.access_token;
}

async function api(token, method, path, body = null) {
  const url = path.startsWith('http') ? path : `${KEYCLOAK_URL}${path}`;
  const opts = {
    method,
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  };
  if (body) opts.body = JSON.stringify(body);
  const res = await fetch(url, opts);
  return { ok: res.ok, status: res.status, text: await res.text(), res };
}

function log(msg) {
  console.log('[keycloak-setup]', msg);
}

async function main() {
  log('Getting admin token...');
  const token = await getAdminToken();

  // 1. Create realm
  const realmExists = (await api(token, 'GET', `/admin/realms/${REALM}`)).ok;
  if (realmExists) {
    log(`Realm "${REALM}" already exists.`);
  } else {
    await api(token, 'POST', '/admin/realms', {
      realm: REALM,
      enabled: true,
      sslRequired: 'external',
      registrationAllowed: false,
      loginWithEmailAllowed: true,
      duplicateEmailsAllowed: false,
      resetPasswordAllowed: true,
      editUsernameAllowed: false,
      bruteForceProtected: true,
    });
    log(`Realm "${REALM}" created.`);
  }

  const realmPath = `/admin/realms/${REALM}`;

  // 2. Client saas-frontend
  const clientsRes = await api(token, 'GET', `${realmPath}/clients?clientId=saas-frontend`);
  const clients = clientsRes.ok ? JSON.parse(clientsRes.text) : [];
  if (clients.length === 0) {
    await api(token, 'POST', `${realmPath}/clients`, {
      clientId: 'saas-frontend',
      name: 'SaaS Frontend',
      enabled: true,
      publicClient: true,
      directAccessGrantsEnabled: true,
      standardFlowEnabled: true,
      rootUrl: FRONTEND_URL,
      baseUrl: '/',
      redirectUris: [FRONTEND_URL, FRONTEND_URL + '/*'],
      webOrigins: [FRONTEND_URL],
      attributes: { 'post.logout.redirect.uris': FRONTEND_URL + '/*' },
    });
    log('Client "saas-frontend" created.');
  } else {
    log('Client "saas-frontend" already exists.');
  }

  // 3. Client saas-backend (confidential, service account)
  const backendRes = await api(token, 'GET', `${realmPath}/clients?clientId=saas-backend`);
  const backendClients = backendRes.ok ? JSON.parse(backendRes.text) : [];
  let backendClientId = null;
  if (backendClients.length === 0) {
    await api(token, 'POST', `${realmPath}/clients`, {
      clientId: 'saas-backend',
      name: 'SaaS Backend',
      enabled: true,
      publicClient: false,
      clientAuthenticatorType: 'client-secret',
      serviceAccountsEnabled: true,
      standardFlowEnabled: false,
      directAccessGrantsEnabled: true,
    });
    log('Client "saas-backend" created.');
    const list = await api(token, 'GET', `${realmPath}/clients?clientId=saas-backend`);
    const arr = JSON.parse(list.text);
    if (arr.length) backendClientId = arr[0].id;
  } else {
    backendClientId = backendClients[0].id;
    log('Client "saas-backend" already exists.');
  }

  // 4. Service account roles for saas-backend (view-users, manage-users)
  if (backendClientId) {
    const serviceAccountRes = await api(token, 'GET', `${realmPath}/clients/${backendClientId}/service-account-user`);
    if (serviceAccountRes.ok) {
      const serviceAccount = JSON.parse(serviceAccountRes.text);
      const realmManagementRes = await api(token, 'GET', `${realmPath}/clients?clientId=realm-management`);
      const realmManagementClients = realmManagementRes.ok ? JSON.parse(realmManagementRes.text) : [];
      if (realmManagementClients.length > 0) {
        const realmManagementId = realmManagementClients[0].id;
        const rolesRes = await api(token, 'GET', `${realmPath}/clients/${realmManagementId}/roles`);
        const roles = rolesRes.ok ? JSON.parse(rolesRes.text) : [];
        const viewUsers = roles.find((r) => r.name === 'view-users');
        const manageUsers = roles.find((r) => r.name === 'manage-users');
        const toAssign = [viewUsers, manageUsers].filter(Boolean);
        if (toAssign.length) {
          await api(token, 'POST', `${realmPath}/users/${serviceAccount.id}/role-mappings/clients/${realmManagementId}`, toAssign);
          log('Service account roles (view-users, manage-users) assigned.');
        }
      }
    }
  }

  // 5. Client scope tenant_claim + mapper
  const scopesRes = await api(token, 'GET', `${realmPath}/client-scopes`);
  const scopes = scopesRes.ok ? JSON.parse(scopesRes.text) : [];
  let tenantScopeId = scopes.find((s) => s.name === 'tenant_claim')?.id;
  if (!tenantScopeId) {
    const createScope = await api(token, 'POST', `${realmPath}/client-scopes`, {
      name: 'tenant_claim',
      description: 'Adds tenant_id from user attribute to token',
      protocol: 'openid-connect',
      attributes: { 'include.in.token.scope': 'true', 'display.on.consent.screen': 'false' },
    });
    if (createScope.ok) {
      const list = await api(token, 'GET', `${realmPath}/client-scopes`);
      const arr = JSON.parse(list.text);
      const scope = arr.find((s) => s.name === 'tenant_claim');
      if (scope) tenantScopeId = scope.id;
    }
    if (tenantScopeId) {
      await api(token, 'POST', `${realmPath}/client-scopes/${tenantScopeId}/protocol-mappers/models`, {
        name: 'tenant_id',
        protocol: 'openid-connect',
        protocolMapper: 'oidc-usermodel-attribute-mapper',
        consentRequired: false,
        config: {
          'userinfo.token.claim': 'true',
          'user.attribute': 'tenant_id',
          'id.token.claim': 'true',
          'access.token.claim': 'true',
          'claim.name': 'tenant_id',
          'jsonType.label': 'String',
        },
      });
      log('Client scope "tenant_claim" and mapper created.');
    }
  } else {
    log('Client scope "tenant_claim" already exists.');
  }

  // 6. Add tenant_claim to saas-frontend and saas-backend default client scopes
  if (tenantScopeId) {
    const frontendList = await api(token, 'GET', `${realmPath}/clients?clientId=saas-frontend`);
    const frontendClients = JSON.parse(frontendList.text);
    if (frontendClients.length > 0) {
      const defaultScopes = await api(token, 'GET', `${realmPath}/clients/${frontendClients[0].id}/default-client-scopes`);
      const defaultIds = defaultScopes.ok ? JSON.parse(defaultScopes.text).map((s) => s.id) : [];
      if (!defaultIds.includes(tenantScopeId)) {
        await api(token, 'PUT', `${realmPath}/clients/${frontendClients[0].id}/default-client-scopes/${tenantScopeId}`);
        log('tenant_claim added to saas-frontend default scopes.');
      }
    }
    if (backendClientId) {
      const backendDefaultScopes = await api(token, 'GET', `${realmPath}/clients/${backendClientId}/default-client-scopes`);
      const backendDefaultIds = backendDefaultScopes.ok ? JSON.parse(backendDefaultScopes.text).map((s) => s.id) : [];
      if (!backendDefaultIds.includes(tenantScopeId)) {
        await api(token, 'PUT', `${realmPath}/clients/${backendClientId}/default-client-scopes/${tenantScopeId}`);
        log('tenant_claim added to saas-backend default scopes.');
      }
    }
  }

  // 7. Realm role "user"
  const rolesRes = await api(token, 'GET', `${realmPath}/roles`);
  const realmRoles = rolesRes.ok ? JSON.parse(rolesRes.text) : [];
  if (!realmRoles.find((r) => r.name === 'user')) {
    await api(token, 'POST', `${realmPath}/roles`, { name: 'user', description: 'Default role for new users' });
    log('Realm role "user" created.');
  } else {
    log('Realm role "user" already exists.');
  }

  // 8. User profile: enable unmanaged attributes (Keycloak 24+), so tenant_id can be set on users
  try {
    const profileRes = await api(token, 'GET', `${realmPath}/users/profile`);
    if (profileRes.ok) {
      const profile = JSON.parse(profileRes.text);
      if (profile.unmanagedAttributePolicy !== 'ENABLED') {
        await api(token, 'PUT', `${realmPath}/users/profile`, { ...profile, unmanagedAttributePolicy: 'ENABLED' });
        log('User profile: unmanaged attributes enabled.');
      }
    }
  } catch (_) {
    // Keycloak < 24 or different API
  }

  // 9. Required action Update profile: disable as default (optional)
  try {
    const requiredActionsRes = await api(token, 'GET', `${realmPath}/authentication/required-actions/UPDATE_PROFILE`);
    if (requiredActionsRes.ok) {
      const action = JSON.parse(requiredActionsRes.text);
      if (action.defaultAction) {
        await api(token, 'PUT', `${realmPath}/authentication/required-actions/UPDATE_PROFILE`, { ...action, defaultAction: false });
        log('Required action "Update profile" set to not default.');
      }
    }
  } catch (_) {}

  // 10. Ativar tema de login saas-login (keycloak-themes/saas-login)
  try {
    const realmGet = await api(token, 'GET', realmPath);
    if (realmGet.ok) {
      const realm = JSON.parse(realmGet.text);
      if (realm.loginTheme !== 'saas-login') {
        await api(token, 'PUT', realmPath, { ...realm, loginTheme: 'saas-login' });
        log('Login theme set to "saas-login".');
      } else {
        log('Login theme "saas-login" already set.');
      }
    }
  } catch (_) {
    log('Could not set login theme (ensure keycloak-themes volume is mounted).');
  }

  log('Keycloak setup finished. Get saas-backend client secret from Keycloak UI (Clients -> saas-backend -> Credentials) for backend config.');
}

main().catch((err) => {
  console.error('[keycloak-setup]', err.message);
  process.exit(1);
});

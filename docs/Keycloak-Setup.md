# Keycloak Setup Script

## Step 1: Access Keycloak Admin Console

1. Open browser: http://localhost:8888
2. Click "Administration Console"
3. Login with:
   - Username: `admin`
   - Password: `admin123`

## Step 2: Create Realm

1. Click dropdown at top-left (says "master")
2. Click "Create Realm"
3. Enter:
   - **Realm name:** `invoice-platform`
4. Click "Create"

## Step 3: Create Client

1. In left menu, click "Clients"
2. Click "Create client"
3. Enter:
   - **Client ID:** `invoice-api`
   - **Client type:** OpenID Connect
4. Click "Next"
5. Enable:
   - ✅ Client authentication
   - ✅ Authorization
   - ✅ Standard flow
   - ✅ Direct access grants
6. Click "Next"
7. Set URLs:
   - **Root URL:** `http://localhost:5000`
   - **Valid redirect URIs:** `http://localhost:5000/*`, `http://localhost:4200/*`
   - **Web origins:** `http://localhost:5000`, `http://localhost:4200`
8. Click "Save"

## Step 4: Get Client Secret

1. Go to "Clients" → "invoice-api"
2. Click "Credentials" tab
3. Copy the **Client Secret** (you'll need this for appsettings.json)

## Step 5: Create Roles

1. In left menu, click "Realm roles"
2. Click "Create role"
3. Create these roles:
   - **admin** (Description: Administrator)
   - **manager** (Description: Invoice Manager)
   - **user** (Description: Regular User)

## Step 6: Create Users

1. In left menu, click "Users"
2. Click "Add user"
3. Create admin user:
   - **Username:** `admin@invoice.com`
   - **Email:** `admin@invoice.com`
   - **First name:** Admin
   - **Last name:** User
   - ✅ Email verified
4. Click "Create"
5. Go to "Credentials" tab
6. Click "Set password"
   - **Password:** `Admin123!`
   - **Temporary:** OFF
7. Click "Save"
8. Go to "Role mapping" tab
9. Click "Assign role"
10. Select "admin" role
11. Click "Assign"

Repeat for:
- **manager@invoice.com** / `Manager123!` (manager role)
- **user@invoice.com** / `User123!` (user role)

## Step 7: Configure Audience Mapper (Critical)

1. Go to **Clients** → **invoice-api**
2. Click **Client scopes** tab
3. Click the link **invoice-api-dedicated** (first row)
4. Click **Add mapper** (or "Configure a new mapper" → "Audience")
5. Select **Audience**
6. Fill in:
   - **Name:** `api-audience`
   - **Included Client Audience:** `invoice-api`
   - **Add to access token:** `On`
7. Click **Save**

## Step 8: Test Token Endpoint

Use this curl command to get a token:

```bash
curl -X POST http://localhost:8888/realms/invoice-platform/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=invoice-api" \
  -d "client_secret=i9ofKABziTruVQoCc1S0ovJiKXuJZSbE" \
  -d "username=admin@invoice.com" \
  -d "password=Admin123!" \
  -d "grant_type=password"
```

You should get a JSON response with `access_token`.

## Configuration Complete! ✅

Now update your API's `appsettings.json` with the client secret.

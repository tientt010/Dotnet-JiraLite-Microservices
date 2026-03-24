# Identity API — Tài liệu cho Frontend

**Base URL:** `http://localhost:5000` (qua Gateway)  
**Content-Type:** `application/json`  
**API Version:** `v1` — truyền qua URL path: `/api/v1/...`

---

## Xác thực (Authentication)

Tất cả endpoints có ký hiệu 🔒 yêu cầu header:

```
Authorization: Bearer <accessToken>
```

---

## Kiểu dữ liệu chung

### `Error`

Trả về khi có lỗi (`400`, `401`, `403`, `404`):

```json
{
    "errorCode": "string",
    "description": "string",
    "errors": null,
    "isValidationError": true
}
```

### `UserDto`

```json
{
    "id": "guid",
    "email": "string",
    "fullName": "string",
    "role": "User | Admin",
    "isActive": true,
    "avatarUrl": "string | null",
    "createdAt": "2026-01-01T00:00:00Z",
    "updatedAt": "2026-01-01T00:00:00Z"
}
```

---

## Auth Endpoints

### POST `/api/v1/auth/register` — Đăng ký

**Request Body:**

```json
{
    "email": "user@example.com",
    "password": "string",
    "fullName": "Nguyen Van A"
}
```

**Responses:**

| Status            | Mô tả                                          |
| ----------------- | ---------------------------------------------- |
| `200 OK`          | Đăng ký thành công                             |
| `400 Bad Request` | Validation lỗi hoặc email đã tồn tại → `Error` |

---

### POST `/api/v1/auth/login` — Đăng nhập

**Request Body:**

```json
{
    "email": "user@example.com",
    "password": "string"
}
```

**Response `200 OK`:**

```json
{
    "accessToken": "eyJhbGciOi...",
    "refreshToken": "string",
    "expiresIn": 900,
    "userInfo": {
        "id": "guid",
        "email": "user@example.com",
        "fullName": "Nguyen Van A",
        "role": "User",
        "isActive": true,
        "avatarUrl": null,
        "createdAt": "2026-01-01T00:00:00Z",
        "updatedAt": "2026-01-01T00:00:00Z"
    }
}
```

> `expiresIn`: số giây access token còn hiệu lực (mặc định `900` = 15 phút)

**Responses:**

| Status             | Mô tả                    |
| ------------------ | ------------------------ |
| `200 OK`           | Đăng nhập thành công     |
| `400 Bad Request`  | Validation lỗi → `Error` |
| `401 Unauthorized` | Sai email hoặc mật khẩu  |
| `403 Forbidden`    | Tài khoản bị khóa        |

---

### POST `/api/v1/auth/refresh-token` — Làm mới Access Token

**Request Body:**

```json
{
    "accessToken": "eyJhbGciOi...",
    "refreshToken": "string"
}
```

**Response `200 OK`:**

```json
{
    "accessToken": "eyJhbGciOi...",
    "refreshToken": "string",
    "expiresIn": 900
}
```

**Responses:**

| Status             | Mô tả                                             |
| ------------------ | ------------------------------------------------- |
| `200 OK`           | Token mới                                         |
| `400 Bad Request`  | Refresh token hết hạn hoặc không hợp lệ → `Error` |
| `401 Unauthorized` | Access token không hợp lệ                         |
| `403 Forbidden`    | Tài khoản bị khóa                                 |

---

### POST `/api/v1/auth/revoke-token` — Đăng xuất

**Request Body:**

```json
{
    "accessToken": "eyJhbGciOi...",
    "refreshToken": "string"
}
```

**Responses:**

| Status             | Mô tả                    |
| ------------------ | ------------------------ |
| `200 OK`           | Đăng xuất thành công     |
| `400 Bad Request`  | Validation lỗi → `Error` |
| `401 Unauthorized` | Token không hợp lệ       |

---

## User Endpoints

### GET `/api/v1/users/{id}` 🔒 — Lấy thông tin User theo ID

**Path Parameters:**

| Tham số | Kiểu   | Mô tả       |
| ------- | ------ | ----------- |
| `id`    | `guid` | ID của user |

**Response `200 OK`:** → `UserDto`

**Responses:**

| Status             | Mô tả                         |
| ------------------ | ----------------------------- |
| `200 OK`           | `UserDto`                     |
| `400 Bad Request`  | Validation lỗi → `Error`      |
| `401 Unauthorized` | Chưa đăng nhập                |
| `404 Not Found`    | Không tìm thấy user → `Error` |

---

### GET `/api/v1/users/{email}` 🔒 — Lấy thông tin User theo Email

**Path Parameters:**

| Tham số | Kiểu     | Mô tả          |
| ------- | -------- | -------------- |
| `email` | `string` | Email của user |

**Response `200 OK`:** → `UserDto`

**Responses:**

| Status             | Mô tả                         |
| ------------------ | ----------------------------- |
| `200 OK`           | `UserDto`                     |
| `400 Bad Request`  | Validation lỗi → `Error`      |
| `401 Unauthorized` | Chưa đăng nhập                |
| `404 Not Found`    | Không tìm thấy user → `Error` |

---

### PUT `/api/v1/users/me/profile` 🔒 — Cập nhật thông tin cá nhân

**Request Body:**

```json
{
    "fullName": "Nguyen Van B",
    "avatarUrl": "https://example.com/avatar.png"
}
```

> Cả hai field đều optional. Chỉ gửi field muốn cập nhật.

**Responses:**

| Status             | Mô tả                    |
| ------------------ | ------------------------ |
| `200 OK`           | Cập nhật thành công      |
| `400 Bad Request`  | Validation lỗi → `Error` |
| `401 Unauthorized` | Chưa đăng nhập           |
| `403 Forbidden`    | Không có quyền           |

---

### PATCH `/api/v1/users/me/password` 🔒 — Đổi mật khẩu

**Request Body:**

```json
{
    "currentPassword": "string",
    "newPassword": "string"
}
```

**Responses:**

| Status             | Mô tả                                               |
| ------------------ | --------------------------------------------------- |
| `200 OK`           | Đổi mật khẩu thành công                             |
| `400 Bad Request`  | Mật khẩu hiện tại sai hoặc validation lỗi → `Error` |
| `401 Unauthorized` | Chưa đăng nhập                                      |
| `403 Forbidden`    | Không có quyền                                      |

---

## Luồng xử lý Token (Frontend)

```
1. POST /auth/login
      → Lưu accessToken + refreshToken vào memory/localStorage

2. Mỗi request → gắn: Authorization: Bearer <accessToken>

3. Khi nhận 401 từ bất kỳ API nào:
      → POST /auth/refresh-token (gửi kèm cả 2 token)
      → Lưu token mới
      → Retry request gốc

4. Khi logout:
      → POST /auth/revoke-token
      → Xóa token khỏi storage
```

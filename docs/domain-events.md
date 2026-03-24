# JiraLite Domain Events — Message Contract Specification

**Version:** 1.0
**Last Updated:** 2026-03-19
**Message Broker:** RabbitMQ 3.x
**Serialization:** JSON (UTF-8)
**Exchange Type:** Topic Exchange

---

## Table of Contents

1. [Overview](#overview)
2. [Message Contract Principles](#message-contract-principles)
3. [Common Data Types](#common-data-types)
4. [Routing & Exchange Configuration](#routing--exchange-configuration)
5. [Issue Events](#issue-events)
6. [Project Events](#project-events)
7. [User Events](#user-events)
8. [Publisher Implementation Guide](#publisher-implementation-guide)
9. [Consumer Implementation Guide](#consumer-implementation-guide)
10. [Error Handling & Retries](#error-handling--retries)
11. [Message Versioning](#message-versioning)
12. [Testing & Validation](#testing--validation)

---

## Overview

JiraLite sử dụng **event-driven architecture** để giao tiếp giữa các microservices. Các service độc lập về ngôn ngữ/framework, chỉ cần tuân thủ message contract này.

### Architecture

```
┌──────────────┐         ┌──────────────┐         ┌──────────────┐
│   Tracking   │────────▶│   RabbitMQ   │◀────────│   Logging    │
│   Service    │ Publish │   Exchange   │ Consume │   Service    │
│  (Any Lang)  │         │              │         │  (Any Lang)  │
└──────────────┘         └──────────────┘         └──────────────┘
                                │
                                ▼
                         ┌──────────────┐
                         │ Notification │
                         │   Service    │
                         │  (Any Lang)  │
                         └──────────────┘
```

### Key Characteristics

- **Protocol-agnostic:** Publisher/consumer có thể dùng bất kỳ ngôn ngữ nào (C#, Python, Go, Node.js)
- **Schema-driven:** Contract dựa trên JSON schema, không phụ thuộc vào type system của ngôn ngữ
- **Self-contained:** Messages chứa đủ thông tin, không cần query giữa services
- **Immutable:** Messages đại diện cho facts đã xảy ra, không bao giờ thay đổi

---

## Message Contract Principles

### 1. JSON Serialization

Tất cả messages được serialize thành **JSON UTF-8**. Field names dùng **camelCase**:

```json
{
    "issueId": "550e8400-e29b-41d4-a716-446655440000",
    "issueName": "Fix login bug",
    "occurredAt": "2026-03-19T10:30:00.000Z"
}
```

**Quy tắc:**

- Field names: `camelCase` (không phải `PascalCase` hay `snake_case`)
- Timestamps: ISO 8601 UTC format (`YYYY-MM-DDTHH:mm:ss.sssZ`)
- GUIDs: Lowercase string với dấu gạch ngang
- Nullable fields: `null` hoặc omit (tùy implementation)

---

### 2. Actor Snapshot Pattern

Mọi event đều chứa **full snapshot** của actor (người thực hiện hành động):

```json
{
    "actorId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
    "actorCode": "john.doe",
    "actorName": "John Doe",
    "actorAvatarUrl": "https://cdn.example.com/avatars/john.jpg"
}
```

**Rationale:** Consumer không cần gọi Identity Service để lấy thông tin actor. Snapshot lưu lại state tại thời điểm event xảy ra (immutable history).

---

### 3. Target Snapshot Pattern

Event chứa snapshot của resource bị tác động:

```json
{
    "issueId": "550e8400-e29b-41d4-a716-446655440000",
    "issueCode": "BUG-123",
    "issueName": "Fix login crash on iOS"
}
```

**Rationale:** Consumer biết "issue BUG-123: Fix login crash" mà không cần query Tracking Service.

---

### 4. String-Based Enums

Status, Priority, Role → dùng **string literals** thay vì numeric enums:

```json
{
    "oldStatus": "ToDo",
    "newStatus": "InProgress"
}
```

**Rationale:**

- Forward-compatible: Thêm status mới không break consumers cũ
- Readable: Dễ debug logs/messages
- Cross-language: Không depend vào enum definitions của từng ngôn ngữ

**Predefined Values:**

| Field        | Allowed Values                                    |
| ------------ | ------------------------------------------------- |
| `status`     | `"ToDo"`, `"InProgress"`, `"Done"`, `"Cancelled"` |
| `priority`   | `"Low"`, `"Medium"`, `"High"`, `"Critical"`       |
| `userStatus` | `"Active"`, `"Locked"`, `"Deactivated"`           |

---

### 5. Changes as Array

Update events chứa array of changes:

```json
{
    "changes": [
        {
            "field": "status",
            "oldValue": "ToDo",
            "newValue": "InProgress"
        },
        {
            "field": "priority",
            "oldValue": "Low",
            "newValue": "High"
        }
    ]
}
```

**Best Practice:** Publisher chỉ gửi fields **thực sự** thay đổi (delta), không gửi toàn bộ entity.

---

### 6. Timestamp Field

```json
{
    "occurredAt": "2026-03-19T10:30:00.000Z"
}
```

**Field name:** `occurredAt` (không phải `createdAt`, `publishedAt`, `timestamp`)

**Format:** ISO 8601 UTC with milliseconds

**Semantics:** Thời điểm domain event xảy ra trong business logic (có thể khác với thời điểm message được publish nếu có delay/retry)

---

## Common Data Types

### Actor Object

Xuất hiện trong **tất cả** events:

```json
{
    "actorId": "string (UUID)",
    "actorCode": "string (username/employee code)",
    "actorName": "string (display name)",
    "actorAvatarUrl": "string | null (avatar URL)"
}
```

| Field            | Type           | Required | Max Length | Example          |
| ---------------- | -------------- | -------- | ---------- | ---------------- |
| `actorId`        | UUID string    | ✅       | 36         | `"a1b2c3d4-..."` |
| `actorCode`      | string         | ✅       | 50         | `"john.doe"`     |
| `actorName`      | string         | ✅       | 100        | `"John Doe"`     |
| `actorAvatarUrl` | string \| null | ❌       | 500        | `"https://..."`  |

---

### Change Object

Dùng trong `IssueUpdatedEvent`, `ProjectUpdatedEvent`, `UserProfileUpdatedEvent`:

```json
{
    "field": "string (field name)",
    "oldValue": "string | null",
    "newValue": "string | null"
}
```

| Field      | Type           | Required | Description                             |
| ---------- | -------------- | -------- | --------------------------------------- |
| `field`    | string         | ✅       | Field name (e.g., `"status"`, `"name"`) |
| `oldValue` | string \| null | ❌       | Previous value (display text)           |
| `newValue` | string \| null | ❌       | New value (display text)                |

**Note:** `oldValue`/`newValue` là text để hiển thị cho người dùng, không dùng cho business logic.

---

## Routing & Exchange Configuration

### Exchange

**Name:** `jiralite.events`
**Type:** `topic`
**Durable:** `true`
**Auto-delete:** `false`

---

### Routing Keys Pattern

```
{domain}.{entity}.{action}
```

**Examples:**

| Event                        | Routing Key                         |
| ---------------------------- | ----------------------------------- |
| `IssueCreatedEvent`          | `tracking.issue.created`            |
| `IssueUpdatedEvent`          | `tracking.issue.updated`            |
| `IssueStatusUpdatedEvent`    | `tracking.issue.statusUpdated`      |
| `IssuePriorityUpdatedEvent`  | `tracking.issue.priorityUpdated`    |
| `IssueAssignedEvent`         | `tracking.issue.assigned`           |
| `IssueDeletedEvent`          | `tracking.issue.deleted`            |
| `ProjectCreatedEvent`        | `tracking.project.created`          |
| `ProjectUpdatedEvent`        | `tracking.project.updated`          |
| `ProjectDeletedEvent`        | `tracking.project.deleted`          |
| `ProjectMemberAddedEvent`    | `tracking.project.memberAdded`      |
| `ProjectMemberRemovedEvent`  | `tracking.project.memberRemoved`    |
| `ProjectManagerUpdatedEvent` | `tracking.project.managerUpdated`   |
| `UserCreatedEvent`           | `identity.user.created`             |
| `UserProfileUpdatedEvent`    | `identity.user.profileUpdated`      |
| `UserPasswordChangedEvent`   | `identity.user.passwordChanged`     |
| `UserStatusUpdatedEvent`     | `identity.user.statusUpdated`       |

---

### Queue Binding Examples

**Logging Service** (consume tất cả events):

```
Queue: logging.all-events
Binding: #
Exchange: jiralite.events
```

**Notification Service** (chỉ consume issue events):

```
Queue: notification.issue-events
Binding: tracking.issue.*
Exchange: jiralite.events
```

**Analytics Service** (chỉ consume created/deleted events):

```
Queue: analytics.lifecycle-events
Binding: *.*.created
Binding: *.*.deleted
Exchange: jiralite.events
```

---

## Issue Events

**Routing Key Prefix:** `tracking.issue.*`

---

### IssueCreatedEvent

**Routing Key:** `tracking.issue.created`

**Trigger:** User tạo issue mới

**Message Schema:**

```json
{
    "issueId": "550e8400-e29b-41d4-a716-446655440000",
    "issueCode": "BUG-123",
    "issueName": "Fix login crash on iOS",
    "actorId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
    "actorCode": "john.doe",
    "actorName": "John Doe",
    "actorAvatarUrl": "https://cdn.example.com/avatars/john.jpg",
    "occurredAt": "2026-03-19T10:30:00.000Z"
}
```

**Field Specification:**

| Field            | Type           | Required | Max Length | Description                           |
| ---------------- | -------------- | -------- | ---------- | ------------------------------------- |
| `issueId`        | UUID string    | ✅       | 36         | Unique issue identifier               |
| `issueCode`      | string         | ✅       | 20         | Human-readable code (e.g., `BUG-123`) |
| `issueName`      | string         | ✅       | 200        | Issue title/summary                   |
| `actorId`        | UUID string    | ✅       | 36         | User who created the issue            |
| `actorCode`      | string         | ✅       | 50         | Actor username                        |
| `actorName`      | string         | ✅       | 100        | Actor display name                    |
| `actorAvatarUrl` | string \| null | ❌       | 500        | Actor avatar URL                      |
| `occurredAt`     | ISO 8601       | ✅       | -          | Event timestamp (UTC)                 |

**Consumers:**

- **Logging:** Create activity log (action: CREATE, target: ISSUE)
- **Notification:** Notify project members
- **Analytics:** Track issue creation metrics

---

### IssueUpdatedEvent

**Routing Key:** `tracking.issue.updated`

**Trigger:** User sửa issue (title, description, hoặc các field không phải status/priority/assignee)

**Message Schema:**

```json
{
    "issueId": "550e8400-e29b-41d4-a716-446655440000",
    "issueCode": "BUG-123",
    "issueName": "Fix login crash on iOS (updated title)",
    "actorId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
    "actorCode": "john.doe",
    "actorName": "John Doe",
    "actorAvatarUrl": "https://cdn.example.com/avatars/john.jpg",
    "changes": [
        {
            "field": "name",
            "oldValue": "Fix login crash",
            "newValue": "Fix login crash on iOS"
        },
        {
            "field": "description",
            "oldValue": "...",
            "newValue": "..."
        }
    ],
    "occurredAt": "2026-03-19T10:35:00.000Z"
}
```

**Additional Fields:**

| Field     | Type            | Required | Description                        |
| --------- | --------------- | -------- | ---------------------------------- |
| `changes` | Array\<Change\> | ✅       | List of field changes (min 1 item) |

**Common `field` values:**

- `"name"` — Issue title
- `"description"` — Issue description

**Consumers:**

- **Logging:** Create activity log with changes
- **Search:** Update search index
- **Notification:** Notify watchers

---

### IssueStatusUpdatedEvent

**Routing Key:** `tracking.issue.statusUpdated`

**Trigger:** User thay đổi issue status (ToDo → InProgress → Done)

**Message Schema:**

```json
{
    "issueId": "550e8400-e29b-41d4-a716-446655440000",
    "issueCode": "BUG-123",
    "issueName": "Fix login crash on iOS",
    "actorId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
    "actorCode": "john.doe",
    "actorName": "John Doe",
    "actorAvatarUrl": "https://cdn.example.com/avatars/john.jpg",
    "oldStatus": "ToDo",
    "newStatus": "InProgress",
    "occurredAt": "2026-03-19T10:40:00.000Z"
}
```

**Additional Fields:**

| Field       | Type   | Required | Allowed Values                                    |
| ----------- | ------ | -------- | ------------------------------------------------- |
| `oldStatus` | string | ✅       | `"ToDo"`, `"InProgress"`, `"Done"`, `"Cancelled"` |
| `newStatus` | string | ✅       | `"ToDo"`, `"InProgress"`, `"Done"`, `"Cancelled"` |

**Consumers:**

- **Logging:** Create activity log (field: `"status"`)
- **Notification:** Notify assignee if status → Done
- **Analytics:** Track cycle time metrics

---

### IssuePriorityUpdatedEvent

**Routing Key:** `tracking.issue.priorityUpdated`

**Trigger:** User thay đổi issue priority

**Message Schema:**

```json
{
    "issueId": "550e8400-e29b-41d4-a716-446655440000",
    "issueCode": "BUG-123",
    "issueName": "Fix login crash on iOS",
    "actorId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
    "actorCode": "john.doe",
    "actorName": "John Doe",
    "actorAvatarUrl": "https://cdn.example.com/avatars/john.jpg",
    "oldPriority": "Low",
    "newPriority": "High",
    "occurredAt": "2026-03-19T10:45:00.000Z"
}
```

**Additional Fields:**

| Field         | Type   | Required | Allowed Values                              |
| ------------- | ------ | -------- | ------------------------------------------- |
| `oldPriority` | string | ✅       | `"Low"`, `"Medium"`, `"High"`, `"Critical"` |
| `newPriority` | string | ✅       | `"Low"`, `"Medium"`, `"High"`, `"Critical"` |

---

### IssueAssignedEvent

**Routing Key:** `tracking.issue.assigned`

**Trigger:** User assign/reassign/unassign issue

**Message Schema:**

```json
{
    "issueId": "550e8400-e29b-41d4-a716-446655440000",
    "issueCode": "BUG-123",
    "issueName": "Fix login crash on iOS",
    "actorId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
    "actorCode": "john.doe",
    "actorName": "John Doe",
    "actorAvatarUrl": "https://cdn.example.com/avatars/john.jpg",
    "oldAssigneeId": "old-user-guid",
    "oldAssigneeCode": "alice",
    "oldAssigneeName": "Alice Nguyen",
    "newAssigneeId": "new-user-guid",
    "newAssigneeCode": "bob",
    "newAssigneeName": "Bob Tran",
    "occurredAt": "2026-03-19T10:50:00.000Z"
}
```

**Additional Fields:**

| Field             | Type                | Required | Description                                 |
| ----------------- | ------------------- | -------- | ------------------------------------------- |
| `oldAssigneeId`   | UUID string \| null | ❌       | Previous assignee ID (`null` if unassigned) |
| `oldAssigneeCode` | string \| null      | ❌       | Previous assignee code                      |
| `oldAssigneeName` | string \| null      | ❌       | Previous assignee name                      |
| `newAssigneeId`   | UUID string \| null | ❌       | New assignee ID (`null` if unassigning)     |
| `newAssigneeCode` | string \| null      | ❌       | New assignee code                           |
| `newAssigneeName` | string \| null      | ❌       | New assignee name                           |

**Scenarios:**

| Scenario | `oldAssignee*` | `newAssignee*` |
| -------- | -------------- | -------------- |
| Assign   | `null`         | `{user}`       |
| Reassign | `{user1}`      | `{user2}`      |
| Unassign | `{user}`       | `null`         |

---

### IssueDeletedEvent

**Routing Key:** `tracking.issue.deleted`

**Trigger:** User xóa issue

**Message Schema:**

```json
{
    "issueId": "550e8400-e29b-41d4-a716-446655440000",
    "issueCode": "BUG-123",
    "issueName": "Fix login crash on iOS",
    "actorId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
    "actorCode": "john.doe",
    "actorName": "John Doe",
    "actorAvatarUrl": "https://cdn.example.com/avatars/john.jpg",
    "occurredAt": "2026-03-19T11:00:00.000Z"
}
```

**Consumers:**

- **Logging:** Create activity log (action: DELETE)
- **Search:** Remove from search index
- **Notification:** Notify watchers

---

## Project Events

**Routing Key Prefix:** `tracking.project.*`

---

### ProjectCreatedEvent

**Routing Key:** `tracking.project.created`

**Message Schema:**

```json
{
    "projectId": "proj-550e8400-e29b-41d4",
    "projectCode": "MOBILE-APP",
    "projectName": "Mobile App Redesign",
    "actorId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
    "actorCode": "john.doe",
    "actorName": "John Doe",
    "actorAvatarUrl": "https://cdn.example.com/avatars/john.jpg",
    "occurredAt": "2026-03-19T09:00:00.000Z"
}
```

---

### ProjectUpdatedEvent

**Routing Key:** `tracking.project.updated`

**Message Schema:**

```json
{
    "projectId": "proj-550e8400-e29b-41d4",
    "projectCode": "MOBILE-APP",
    "projectName": "Mobile App Redesign Q2",
    "actorId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
    "actorCode": "john.doe",
    "actorName": "John Doe",
    "actorAvatarUrl": "https://cdn.example.com/avatars/john.jpg",
    "changes": [
        {
            "field": "name",
            "oldValue": "Mobile App Redesign",
            "newValue": "Mobile App Redesign Q2"
        }
    ],
    "occurredAt": "2026-03-19T09:30:00.000Z"
}
```

---

### ProjectDeletedEvent

**Routing Key:** `tracking.project.deleted`

**Message Schema:** Giống `ProjectCreatedEvent` (không có `changes`)

---

### ProjectMemberAddedEvent

**Routing Key:** `tracking.project.memberAdded`

**Message Schema:**

```json
{
    "projectId": "proj-550e8400-e29b-41d4",
    "projectCode": "MOBILE-APP",
    "projectName": "Mobile App Redesign",
    "actorId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
    "actorCode": "john.doe",
    "actorName": "John Doe",
    "actorAvatarUrl": "https://cdn.example.com/avatars/john.jpg",
    "memberId": "member-guid",
    "memberCode": "jane.doe",
    "memberName": "Jane Doe",
    "occurredAt": "2026-03-19T10:00:00.000Z"
}
```

---

### ProjectMemberRemovedEvent

**Routing Key:** `tracking.project.memberRemoved`

**Message Schema:** Giống `ProjectMemberAddedEvent`

---

### ProjectManagerUpdatedEvent

**Routing Key:** `tracking.project.managerUpdated`

**Message Schema:**

```json
{
    "projectId": "proj-550e8400-e29b-41d4",
    "projectCode": "MOBILE-APP",
    "projectName": "Mobile App Redesign",
    "actorId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
    "actorCode": "john.doe",
    "actorName": "John Doe",
    "actorAvatarUrl": "https://cdn.example.com/avatars/john.jpg",
    "oldManagerId": "old-mgr-guid",
    "oldManagerCode": "alice",
    "oldManagerName": "Alice Nguyen",
    "newManagerId": "new-mgr-guid",
    "newManagerCode": "bob",
    "newManagerName": "Bob Tran",
    "occurredAt": "2026-03-19T11:00:00.000Z"
}
```

---

## User Events

**Routing Key Prefix:** `identity.user.*`

---

### UserCreatedEvent

**Routing Key:** `identity.user.created`

**Message Schema:**

```json
{
    "userId": "user-550e8400-e29b-41d4",
    "userCode": "john.doe",
    "userName": "John Doe",
    "actorId": "admin-guid",
    "actorCode": "admin",
    "actorName": "System Admin",
    "actorAvatarUrl": null,
    "occurredAt": "2026-03-19T08:00:00.000Z"
}
```

**Note:** `actorId` có thể = `userId` nếu user tự đăng ký.

---

### UserProfileUpdatedEvent

**Routing Key:** `identity.user.profileUpdated`

**Message Schema:**

```json
{
    "userId": "user-550e8400-e29b-41d4",
    "userCode": "john.doe",
    "userName": "John Smith",
    "actorId": "user-550e8400-e29b-41d4",
    "actorCode": "john.doe",
    "actorName": "John Smith",
    "actorAvatarUrl": "https://cdn.example.com/avatars/john.jpg",
    "changes": [
        {
            "field": "name",
            "oldValue": "John Doe",
            "newValue": "John Smith"
        }
    ],
    "occurredAt": "2026-03-19T12:00:00.000Z"
}
```

**Common `field` values:** `"name"`, `"email"`, `"avatarUrl"`

---

### UserPasswordChangedEvent

**Routing Key:** `identity.user.passwordChanged`

**Message Schema:** Giống `UserCreatedEvent` (không có `changes`)

**Security:** Không bao giờ gửi password trong message.

---

### UserStatusUpdatedEvent

**Routing Key:** `identity.user.statusUpdated`

**Message Schema:**

```json
{
    "userId": "user-550e8400-e29b-41d4",
    "userCode": "john.doe",
    "userName": "John Doe",
    "actorId": "admin-guid",
    "actorCode": "admin",
    "actorName": "System Admin",
    "actorAvatarUrl": null,
    "oldStatus": "Active",
    "newStatus": "Locked",
    "occurredAt": "2026-03-19T13:00:00.000Z"
}
```

**Allowed Status Values:** `"Active"`, `"Locked"`, `"Deactivated"`

---

### UserLoginSucceededEvent

**Routing Key:** `identity.user.loginSucceeded`

**Message Schema:** Giống `UserCreatedEvent`

**Use Case:** Audit logging (không dùng cho business logic)

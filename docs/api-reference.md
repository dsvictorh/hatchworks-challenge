# API Reference

Base URL: `/api/v1`  
Auth: Bearer token (where noted)

## Quick Reference

### User Referrals

- `GET /referrals` (auth) - Get my referral code and list
- `POST /referrals/link` (auth) - Generate shareable link  
- `POST /referrals/share-message` (auth) - Get pre-filled message text
- `POST /referrals/share` (auth) - Track share event

### Public/Webhook

- `POST /referrals/events` - Process vendor events (click, install, etc)
- `POST /referrals/verify` - Check if referral code is valid
- `POST /referrals/session` - Create temp session before signup

### Redemption

- `PATCH /referrals/redeem` (auth) - Complete the referral

## GET /referrals

Get your referral code and list of people you've invited.

**Parameters:** page, size, status (optional filter)

**Returns:**

```json
{
  "referralCode": "XY7G4D",
  "summary": { "total": 3, "complete": 2, "pending": 1 },
  "items": [
    { "id": "ref_1001", "name": "Jenny S.", "status": "complete", "channel": "sms" }
  ]
}
```

## POST /referrals/link

Generate a shareable link for a specific channel.

Send: `{ "channel": "sms" }` (sms, email, or generic)

Returns: `{ "referralLink": "https://cartoncaps.link/...", "expiresAt": "..." }`

## POST /referrals/share-message

Get pre-filled message text for sharing.

Send: `{ "channel": "email", "locale": "en" }`

Returns: `{ "subject": "...", "message": "...", "link": "..." }`

## 3) `POST /referrals/share-message`

**Body**  

```json
{ "channel": "email", "locale": "en-US" }
```

**Response 200**  

```json
{
  "subject": "You’re invited to try the Carton Caps app!",
  "message": "Hey! Join me in earning cash for our school...",
  "link": "https://cartoncaps.link/ab1fefa09p?ref=XY7G4D"
}
```

**Errors:** `400`, `401`.

## POST /referrals/share

Track when someone shares their referral link.

Send: `{ "channel": "sms", "link": "...", "deviceInfo": {...} }`

## POST /referrals/events

Process events from the deep link vendor (no auth required).

Send: `{ "event": "click", "referralCode": "XY7G4D", "eventId": "..." }`

Events: click, install, open, redeemed

## POST /referrals/verify

Check if a referral code is valid (used on first app launch).

Send: `{ "referralCode": "XY7G4D", "deviceId": "dev-123" }`

Returns: `{ "isValid": true, "referrer": {...}, "campaign": {...} }`

## POST /referrals/session

Create a temporary session before user signs up.

Send: `{ "referralCode": "XY7G4D", "deviceId": "dev-123" }`

Returns: `{ "sessionId": "ses-001", "expiresAt": "..." }`

## PATCH /referrals/redeem

Complete the referral after new user creates account.

Send: `{ "referralCode": "XY7G4D", "refereeUserId": "usr_1234" }`

Returns: `{ "status": "redeemed", "rewardEligible": true }`

## Common Errors

- 400 - Bad request (invalid data)
- 401 - Need authentication  
- 403 - Access denied
- 409 - Conflict (already used)
- 410 - Expired
- 429 - Rate limited

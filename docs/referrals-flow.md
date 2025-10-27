# Referral Flow

Quick overview of how the referral system works for both users, taken from the screens in the PDF file `20240813-LivefrontCodingChallenge-UIDesign`.

## Sharing a Referral (Existing User)

1. User opens "Invite Friends" screen
   - App calls `GET /api/v1/referrals` to get their code and referral list

2. User taps "Share" button (SMS/Email/Generic)
   - App calls `POST /api/v1/referrals/link` to generate a shareable link
   - App calls `POST /api/v1/referrals/share-message` to get pre-filled message text
   - App records the share event with `POST /api/v1/referrals/share`

## New User Journey (Friend Being Invited)

1. **Link Click**: Friend taps the referral link
   - OS resolves the link through deep link vendor
   - Vendor logs click event and redirects to app store

2. **App Install**: Friend downloads and opens the app
   - App fetches referral info from vendor on first launch
   - Gets referral code, campaign details, etc.

3. **Code Verification**: App verifies the referral code
   - Calls `POST /referrals/verify` with the code and device ID
   - API returns referrer info if valid

4. **Sign Up & Redeem**: Friend completes registration
   - App calls `PATCH /referrals/redeem` to complete the referral
   - Both users become eligible for rewards

## Implementation Notes

- Events API (`POST /referrals/events`) handles install, open, and redeem events
- Rate limiting on link generation and sharing to prevent abuse
- Self-referral protection in verify and redeem endpoints
- Currently using mock deep link vendor (no external service calls)

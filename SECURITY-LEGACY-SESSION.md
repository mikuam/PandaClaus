# Legacy-session access hotfix (alternative branch)

This branch keeps the existing `/?adminCode=...` login that sets `IsAdmin` in the ASP.NET session. It does **not** add `VolunteerLogin` or require `VolunteerAccessCode`. It is an alternative to the environment-secret branch, not a follow-up commit to it.

Anonymous requests cannot use `earlyAccess=true` or an old `IsEarlyAccess` session flag to see unpublished letters. The status page and its write handlers require `IsAdmin`; hidden letter details and photos require the same session. Public pages omit photo URLs. Reservations check eligibility and match the requested row to the URL hash; confirmation pages require a matching operation session; receipt GET/POST require `IsAdmin`.

Anonymous QR scans temporarily retain a bounded receipt destination in the server-side session, and opening the legacy admin link in the same browser returns the volunteer to that receipt form after login. A copied admin link or a different browser cannot restore it.

**Do not mistake this for a safe login design:** the shared `AdminCode` remains accepted in a URL and may enter browser history, logs and copied links. All holders have full admin permissions. The code must be nonempty; rotate it before use. `Referrer-Policy: no-referrer`, no-store on sensitive pages, and secure session cookies reduce some secondary leakage but do not remove the URL risk. No real code belongs in Git or this document.

Direct photo links previously disclosed may still work if Azure Blob/CDN permits public reads. This branch does not sanitize original-image EXIF, change storage permissions, configure production secrets, or deploy anything. The operator must assess the hosting and blob configuration independently before merge. The repository workflow may deploy automatically on a push to `main`.

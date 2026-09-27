const roleClaimNames = [
  "role",
  "roles",
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
];

export function getAccessTokenRoles(token = localStorage.getItem("accessToken")) {
  if (!token) return [];

  try {
    const payload = token.split(".")[1];
    if (!payload) return [];
    const base64 = payload.replace(/-/g, "+").replace(/_/g, "/");
    const padded = base64.padEnd(Math.ceil(base64.length / 4) * 4, "=");
    const bytes = Uint8Array.from(atob(padded), (character) => character.charCodeAt(0));
    const claims = JSON.parse(new TextDecoder().decode(bytes));

    return roleClaimNames.flatMap((name) => {
      const value = claims[name];
      if (Array.isArray(value)) return value.map(String);
      return value == null ? [] : [String(value)];
    });
  } catch {
    return [];
  }
}

export function getAccessTokenSubject(token = localStorage.getItem("accessToken")) {
  if (!token) return null;

  try {
    const base64 = token.split(".")[1]?.replace(/-/g, "+").replace(/_/g, "/");
    if (!base64) return null;
    const padded = base64.padEnd(Math.ceil(base64.length / 4) * 4, "=");
    const bytes = Uint8Array.from(atob(padded), (character) => character.charCodeAt(0));
    const claims = JSON.parse(new TextDecoder().decode(bytes));
    return claims.sub ?? claims["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] ?? null;
  } catch {
    return null;
  }
}

export function isAdmin(token) {
  return getAccessTokenRoles(token).some((role) => role.toLowerCase() === "admin");
}

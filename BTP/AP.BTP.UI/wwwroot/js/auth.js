export async function login(email, password) {
  try {
    const resp = await fetch('/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ email, password })
    });
    if (resp.ok) return { ok: true };
    let message = 'Login failed.';
    try {
      const data = await resp.json();
      message = data?.message || message;
    } catch {}
    return { ok: false, message };
  } catch (e) {
    return { ok: false, message: e?.message || 'Network error' };
  }
}

export async function api
() {
  try {
    // Note: This function is not currently used. Components should use IHttpClientFactory instead.
      // If needed, call the frontend logout endpoint which redirects, or navigate to /logout
      globalThis.location.href = '/logout';
    return true;
  } catch {
    return false;
  }
}

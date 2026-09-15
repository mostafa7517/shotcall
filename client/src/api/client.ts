const API_BASE = "https://localhost:7102"

function getAccessToken() {
  return localStorage.getItem("accessToken")
}

function getRefreshToken() {
  return localStorage.getItem("refreshToken")
}

function setTokens(accessToken: string, refreshToken: string) {
  localStorage.setItem("accessToken", accessToken)
  localStorage.setItem("refreshToken", refreshToken)
}

function clearTokens() {
  localStorage.removeItem("accessToken")
  localStorage.removeItem("refreshToken")
  localStorage.removeItem("user")
}

async function refreshAccessToken(): Promise<string | null> {
  const refreshToken = getRefreshToken()
  if (!refreshToken) return null

  const res = await fetch(`${API_BASE}/api/auth/refresh`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ refreshToken }),
  })

  if (!res.ok) {
    clearTokens()
    return null
  }

  const data = await res.json()
  setTokens(data.accessToken, data.refreshToken)
  localStorage.setItem("user", JSON.stringify(data))
  return data.accessToken
}

export async function apiFetch(path: string, options: RequestInit = {}): Promise<any> {
  let token = getAccessToken()

  const doFetch = async (accessToken: string | null) => {
    const headers: Record<string, string> = {
      "Content-Type": "application/json",
      ...(options.headers as Record<string, string> | undefined),
    }
    if (accessToken) headers["Authorization"] = `Bearer ${accessToken}`

    return fetch(`${API_BASE}${path}`, { ...options, headers })
  }

  let res = await doFetch(token)

  if (res.status === 401 && token) {
    const newToken = await refreshAccessToken()
    if (newToken) {
      res = await doFetch(newToken)
    }
  }

  if (!res.ok) {
    const errorBody = await res.json().catch(() => ({ error: res.statusText }))
    throw new Error(errorBody.error || `Request failed (${res.status})`)
  }

  if (res.status === 204) return null
  return res.json()
}

export { setTokens, clearTokens, getAccessToken }

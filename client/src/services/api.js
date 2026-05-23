import axios from 'axios'

const BASE = import.meta.env.VITE_API_URL ?? ''

const api = axios.create({
  baseURL: `${BASE}/api`,
  headers: { 'Content-Type': 'application/json' },
})

// Attach JWT on every request
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('turnosToken')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// On 401, try refresh token once
api.interceptors.response.use(
  (res) => res,
  async (error) => {
    const original = error.config
    if (error.response?.status === 401 && !original._retry) {
      original._retry = true
      try {
        const refreshToken = localStorage.getItem('turnosRefresh')
        if (!refreshToken) throw new Error('No refresh token')
        const { data } = await axios.post(`${BASE}/api/auth/refresh-token`, { refreshToken })
        localStorage.setItem('turnosToken', data.token)
        localStorage.setItem('turnosRefresh', data.refreshToken)
        original.headers.Authorization = `Bearer ${data.token}`
        return api(original)
      } catch {
        localStorage.clear()
        window.location.href = '/login'
      }
    }
    return Promise.reject(error)
  }
)

export default api

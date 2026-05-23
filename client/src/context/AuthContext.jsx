import { createContext, useContext, useState, useCallback } from 'react'
import api from '../services/api'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    try {
      const stored = localStorage.getItem('turnosUser')
      return stored ? JSON.parse(stored) : null
    } catch {
      return null
    }
  })

  const login = useCallback(async (email, password) => {
    const { data } = await api.post('/auth/login', { email, password })
    localStorage.setItem('turnosToken', data.token)
    localStorage.setItem('turnosRefresh', data.refreshToken)
    localStorage.setItem('turnosUser', JSON.stringify(data.user))
    setUser(data.user)
    return data.user
  }, [])

  const register = useCallback(async (payload) => {
    const { data } = await api.post('/auth/register', payload)
    localStorage.setItem('turnosToken', data.token)
    localStorage.setItem('turnosRefresh', data.refreshToken)
    localStorage.setItem('turnosUser', JSON.stringify(data.user))
    setUser(data.user)
    return data.user
  }, [])

  const logout = useCallback(() => {
    localStorage.removeItem('turnosToken')
    localStorage.removeItem('turnosRefresh')
    localStorage.removeItem('turnosUser')
    setUser(null)
  }, [])

  return (
    <AuthContext.Provider value={{ user, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export const useAuth = () => useContext(AuthContext)

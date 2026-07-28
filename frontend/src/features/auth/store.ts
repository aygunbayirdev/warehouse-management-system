import { create } from 'zustand'
import { persist } from 'zustand/middleware'

import type { AuthenticatedUser, AuthTokens } from './types'

type AuthState = {
  accessToken: string | null
  refreshToken: string | null
  user: AuthenticatedUser | null
  setTokens: (tokens: AuthTokens) => void
  setUser: (user: AuthenticatedUser) => void
  clear: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      accessToken: null,
      refreshToken: null,
      user: null,
      setTokens: (tokens) =>
        set({
          accessToken: tokens.accessToken,
          refreshToken: tokens.refreshToken,
        }),
      setUser: (user) => set({ user }),
      clear: () => set({ accessToken: null, refreshToken: null, user: null }),
    }),
    { name: 'wms-auth' },
  ),
)

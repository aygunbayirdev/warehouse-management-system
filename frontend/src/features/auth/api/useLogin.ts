import { useMutation } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

import { useAuthStore } from '../store'
import type { AuthTokens } from '../types'

type LoginPayload = {
  email: string
  password: string
}

export function useLogin() {
  const setTokens = useAuthStore((state) => state.setTokens)

  return useMutation({
    mutationFn: async (payload: LoginPayload) => {
      const response = await apiClient.post<AuthTokens>('/auth/login', payload)
      return response.data
    },
    onSuccess: setTokens,
  })
}

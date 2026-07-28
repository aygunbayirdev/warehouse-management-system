import { useQuery } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

import { useAuthStore } from '../store'
import type { AuthenticatedUser } from '../types'

export function useCurrentUser() {
  const accessToken = useAuthStore((state) => state.accessToken)

  return useQuery({
    queryKey: ['auth', 'me'],
    queryFn: async () => {
      const response = await apiClient.get<AuthenticatedUser>('/auth/me')
      return response.data
    },
    enabled: Boolean(accessToken),
    retry: false,
    staleTime: 5 * 60 * 1000,
  })
}

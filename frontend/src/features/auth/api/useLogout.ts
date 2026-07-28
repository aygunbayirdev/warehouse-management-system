import { useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'

import { useAuthStore } from '../store'

export function useLogout() {
  const clear = useAuthStore((state) => state.clear)
  const queryClient = useQueryClient()
  const navigate = useNavigate()

  return () => {
    clear()
    queryClient.clear()
    navigate('/login', { replace: true })
  }
}

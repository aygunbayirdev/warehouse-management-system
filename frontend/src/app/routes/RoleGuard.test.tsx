import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it } from 'vitest'

import { useAuthStore } from '@/features/auth/store'
import { RoleNames } from '@/features/auth/types'

import { RoleGuard } from './RoleGuard'

function renderWithQueryClient(allowedRoles: string[], roles: string[]) {
  const queryClient = new QueryClient()
  queryClient.setQueryData(['auth', 'me'], {
    id: '1',
    email: 'a@b.com',
    firstName: 'A',
    lastName: 'B',
    roles,
  })

  render(
    <QueryClientProvider client={queryClient}>
      <RoleGuard allowedRoles={allowedRoles}>
        <div>Korumalı içerik</div>
      </RoleGuard>
    </QueryClientProvider>,
  )
}

describe('RoleGuard', () => {
  beforeEach(() => {
    useAuthStore.getState().setTokens({
      accessToken: 'test-access-token',
      accessTokenExpiresAtUtc: new Date().toISOString(),
      refreshToken: 'test-refresh-token',
      refreshTokenExpiresAtUtc: new Date().toISOString(),
    })
  })

  it('renders children when the user has an allowed role', () => {
    renderWithQueryClient([RoleNames.Admin], ['Admin'])

    expect(screen.getByText('Korumalı içerik')).toBeInTheDocument()
  })

  it('shows an unauthorized message when the user lacks an allowed role', () => {
    renderWithQueryClient([RoleNames.Admin], ['DepoPersoneli'])

    expect(
      screen.getByText('Bu sayfayı görüntüleme yetkiniz yok.'),
    ).toBeInTheDocument()
  })
})

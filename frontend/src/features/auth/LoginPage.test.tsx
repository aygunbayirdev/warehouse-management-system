import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { AxiosError } from 'axios'
import { MemoryRouter } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { useAuthStore } from '@/features/auth/store'

vi.mock('@/lib/axios', () => ({
  apiClient: {
    post: vi.fn(),
  },
}))

import { apiClient } from '@/lib/axios'

import { LoginPage } from './LoginPage'

function renderLoginPage() {
  const queryClient = new QueryClient()
  render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('LoginPage', () => {
  beforeEach(() => {
    useAuthStore.getState().clear()
    vi.mocked(apiClient.post).mockReset()
  })

  it('stores tokens after a successful login using the prefilled demo credentials', async () => {
    vi.mocked(apiClient.post).mockResolvedValueOnce({
      data: {
        accessToken: 'test-access-token',
        accessTokenExpiresAtUtc: new Date().toISOString(),
        refreshToken: 'test-refresh-token',
        refreshTokenExpiresAtUtc: new Date().toISOString(),
      },
    })

    renderLoginPage()

    expect(screen.getByLabelText('E-posta')).toHaveValue('admin@wms.local')
    expect(screen.getByLabelText('Şifre')).toHaveValue('ChangeMe123!')

    const user = userEvent.setup()
    await user.click(screen.getByRole('button', { name: 'Giriş Yap' }))

    await waitFor(() =>
      expect(useAuthStore.getState().accessToken).toBe('test-access-token'),
    )
  })

  it('shows an error message when login fails with invalid credentials', async () => {
    vi.mocked(apiClient.post).mockRejectedValueOnce(
      Object.assign(new AxiosError('Unauthorized'), {
        response: { status: 401, data: { title: 'Auth.InvalidCredentials' } },
      }),
    )

    renderLoginPage()

    const user = userEvent.setup()
    await user.clear(screen.getByLabelText('E-posta'))
    await user.type(screen.getByLabelText('E-posta'), 'admin@wms.local')
    await user.clear(screen.getByLabelText('Şifre'))
    await user.type(screen.getByLabelText('Şifre'), 'wrong-password')
    await user.click(screen.getByRole('button', { name: 'Giriş Yap' }))

    expect(
      await screen.findByText('E-posta veya şifre hatalı.'),
    ).toBeInTheDocument()
    expect(useAuthStore.getState().accessToken).toBeNull()
  })
})

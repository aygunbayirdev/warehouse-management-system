import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { beforeEach, describe, expect, it } from 'vitest'

import { useAuthStore } from '@/features/auth/store'

import { ProtectedRoute } from './ProtectedRoute'

function renderWithRouter() {
  render(
    <MemoryRouter initialEntries={['/']}>
      <Routes>
        <Route path="/login" element={<div>Login sayfası</div>} />
        <Route element={<ProtectedRoute />}>
          <Route path="/" element={<div>Dashboard</div>} />
        </Route>
      </Routes>
    </MemoryRouter>,
  )
}

describe('ProtectedRoute', () => {
  beforeEach(() => {
    useAuthStore.getState().clear()
  })

  it('redirects to /login when there is no access token', () => {
    renderWithRouter()

    expect(screen.getByText('Login sayfası')).toBeInTheDocument()
  })

  it('renders the protected content when an access token is present', () => {
    useAuthStore.getState().setTokens({
      accessToken: 'test-access-token',
      accessTokenExpiresAtUtc: new Date().toISOString(),
      refreshToken: 'test-refresh-token',
      refreshTokenExpiresAtUtc: new Date().toISOString(),
    })

    renderWithRouter()

    expect(screen.getByText('Dashboard')).toBeInTheDocument()
  })
})

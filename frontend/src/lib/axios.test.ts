import type { InternalAxiosRequestConfig } from 'axios'
import { beforeEach, describe, expect, it } from 'vitest'

import { useAuthStore } from '@/features/auth/store'

import { attachAuthHeader } from './axios'

function fakeConfig(): InternalAxiosRequestConfig {
  return { headers: {} } as InternalAxiosRequestConfig
}

describe('attachAuthHeader', () => {
  beforeEach(() => {
    useAuthStore.getState().clear()
  })

  it('does not attach an Authorization header when there is no access token', () => {
    const result = attachAuthHeader(fakeConfig())

    expect(result.headers.Authorization).toBeUndefined()
  })

  it('attaches a Bearer Authorization header when an access token is present', () => {
    useAuthStore.getState().setTokens({
      accessToken: 'test-access-token',
      accessTokenExpiresAtUtc: new Date().toISOString(),
      refreshToken: 'test-refresh-token',
      refreshTokenExpiresAtUtc: new Date().toISOString(),
    })

    const result = attachAuthHeader(fakeConfig())

    expect(result.headers.Authorization).toBe('Bearer test-access-token')
  })
})

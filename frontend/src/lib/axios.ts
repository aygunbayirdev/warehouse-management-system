import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios'

import { useAuthStore } from '@/features/auth/store'
import type { AuthTokens } from '@/features/auth/types'

const baseURL = import.meta.env.VITE_API_URL

export const apiClient = axios.create({ baseURL })

export function attachAuthHeader(
  config: InternalAxiosRequestConfig,
): InternalAxiosRequestConfig {
  const { accessToken } = useAuthStore.getState()
  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`
  }
  return config
}

apiClient.interceptors.request.use(attachAuthHeader)

type RetriableRequestConfig = InternalAxiosRequestConfig & { _retry?: boolean }

let refreshPromise: Promise<AuthTokens> | null = null

async function refreshAccessToken(): Promise<AuthTokens> {
  const { refreshToken } = useAuthStore.getState()
  if (!refreshToken) {
    throw new Error('No refresh token available')
  }

  const response = await axios.post<AuthTokens>(`${baseURL}/auth/refresh`, {
    refreshToken,
  })
  return response.data
}

function redirectToLogin() {
  useAuthStore.getState().clear()
  window.location.assign('/login')
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as RetriableRequestConfig | undefined
    const status = error.response?.status
    const isRefreshCall = originalRequest?.url?.includes('/auth/refresh')
    const isLoginCall = originalRequest?.url?.includes('/auth/login')

    if (status !== 401 || !originalRequest || isLoginCall) {
      return Promise.reject(error)
    }

    if (isRefreshCall || originalRequest._retry) {
      redirectToLogin()
      return Promise.reject(error)
    }

    originalRequest._retry = true

    try {
      refreshPromise ??= refreshAccessToken()
      const tokens = await refreshPromise
      useAuthStore.getState().setTokens(tokens)
      originalRequest.headers.Authorization = `Bearer ${tokens.accessToken}`
      return apiClient(originalRequest)
    } catch (refreshError) {
      redirectToLogin()
      return Promise.reject(refreshError)
    } finally {
      refreshPromise = null
    }
  },
)

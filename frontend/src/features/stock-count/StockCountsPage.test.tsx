import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'

vi.mock('@/lib/axios', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
  },
}))

vi.mock('@/features/auth/api/useHasAnyRole', () => ({
  useHasAnyRole: vi.fn(),
}))

import { useHasAnyRole } from '@/features/auth/api/useHasAnyRole'
import { apiClient } from '@/lib/axios'

import { StockCountsPage } from './StockCountsPage'
import type { StockCountDto } from './types'

const DRAFT_COUNT: StockCountDto = {
  id: 'sc1',
  warehouseId: 'w1',
  warehouseName: 'Ankara Depo',
  status: 'Draft',
  createdByUserId: 'u1',
  createdAtUtc: '2026-01-01T10:00:00',
  closedAtUtc: null,
  lines: [],
}

const WAREHOUSE = { id: 'w1', code: 'ANK-01', name: 'Ankara Depo', address: null }

function mockListEndpoints() {
  vi.mocked(apiClient.get).mockImplementation((url: string) => {
    if (url === '/stock-counts') {
      return Promise.resolve({ data: [DRAFT_COUNT] })
    }
    if (url === '/warehouses') {
      return Promise.resolve({ data: [WAREHOUSE] })
    }
    return Promise.reject(new Error(`Unexpected GET ${url}`))
  })
}

function renderPage() {
  const queryClient = new QueryClient()
  render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <StockCountsPage />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('StockCountsPage', () => {
  beforeEach(() => {
    vi.mocked(apiClient.get).mockReset()
    vi.mocked(apiClient.post).mockReset()
    vi.mocked(useHasAnyRole).mockReset()
    mockListEndpoints()
  })

  it('renders the list with a status badge and line count', async () => {
    vi.mocked(useHasAnyRole).mockReturnValue(false)

    renderPage()

    expect(await screen.findByText('Ankara Depo')).toBeInTheDocument()
    expect(screen.getByText('Taslak')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Detay' })).toBeInTheDocument()
  })

  it('hides Yeni Sayım for a non-manager role and shows it for a manager role', async () => {
    vi.mocked(useHasAnyRole).mockReturnValue(false)

    const { rerender } = render(
      <QueryClientProvider client={new QueryClient()}>
        <MemoryRouter>
          <StockCountsPage />
        </MemoryRouter>
      </QueryClientProvider>,
    )
    await screen.findByText('Ankara Depo')
    expect(
      screen.queryByRole('button', { name: /Yeni Sayım/ }),
    ).not.toBeInTheDocument()

    vi.mocked(useHasAnyRole).mockReturnValue(true)
    rerender(
      <QueryClientProvider client={new QueryClient()}>
        <MemoryRouter>
          <StockCountsPage />
        </MemoryRouter>
      </QueryClientProvider>,
    )
    expect(
      await screen.findByRole('button', { name: /Yeni Sayım/ }),
    ).toBeInTheDocument()
  })
})

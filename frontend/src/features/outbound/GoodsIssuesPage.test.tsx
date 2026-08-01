import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
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

import { GoodsIssuesPage } from './GoodsIssuesPage'
import type { GoodsIssueDto } from './types'

const DRAFT_ISSUE: GoodsIssueDto = {
  id: 'gi1',
  warehouseId: 'w1',
  warehouseName: 'Ankara Depo',
  destination: 'Müşteri A',
  status: 'Draft',
  createdByUserId: 'u1',
  createdAtUtc: '2026-01-01T10:00:00',
  approvedAtUtc: null,
  lines: [
    { productId: 'p1', productSku: 'SKU-1', productName: 'Ürün 1', quantity: 5 },
  ],
}

const WAREHOUSE = { id: 'w1', code: 'ANK-01', name: 'Ankara Depo', address: null }

function mockListEndpoints() {
  vi.mocked(apiClient.get).mockImplementation((url: string) => {
    if (url === '/goods-issues') {
      return Promise.resolve({
        data: { items: [DRAFT_ISSUE], totalCount: 1, page: 1, pageSize: 20 },
      })
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
        <GoodsIssuesPage />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('GoodsIssuesPage', () => {
  beforeEach(() => {
    vi.mocked(apiClient.get).mockReset()
    vi.mocked(apiClient.post).mockReset()
    vi.mocked(useHasAnyRole).mockReset()
    mockListEndpoints()
  })

  it('renders the list with a status badge, destination, and always shows the create link', async () => {
    vi.mocked(useHasAnyRole).mockReturnValue(false)

    renderPage()

    expect(await screen.findByText('Ankara Depo')).toBeInTheDocument()
    expect(screen.getByText('Müşteri A')).toBeInTheDocument()
    expect(screen.getByText('Taslak')).toBeInTheDocument()
    expect(
      screen.getByRole('link', { name: /Yeni Sevkiyat/ }),
    ).toBeInTheDocument()
  })

  it('hides the approve button in the detail dialog for a non-approver role', async () => {
    vi.mocked(useHasAnyRole).mockReturnValue(false)

    renderPage()
    await screen.findByText('Ankara Depo')

    const user = userEvent.setup()
    await user.click(screen.getByRole('button', { name: 'Detay' }))

    expect(await screen.findByText('SKU-1')).toBeInTheDocument()
    expect(
      screen.queryByRole('button', { name: 'Onayla' }),
    ).not.toBeInTheDocument()
  })

  it('shows the pagination summary and disables both buttons on a single page', async () => {
    vi.mocked(useHasAnyRole).mockReturnValue(true)

    renderPage()

    expect(await screen.findByText('1–1 / 1')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Önceki' })).toBeDisabled()
    expect(screen.getByRole('button', { name: 'Sonraki' })).toBeDisabled()
  })

  it('requests the next page when Sonraki is clicked', async () => {
    vi.mocked(useHasAnyRole).mockReturnValue(true)
    vi.mocked(apiClient.get).mockImplementation((url: string, config) => {
      if (url === '/goods-issues') {
        return Promise.resolve({
          data: {
            items: [DRAFT_ISSUE],
            totalCount: 25,
            page: (config?.params as { page?: number })?.page ?? 1,
            pageSize: 20,
          },
        })
      }
      if (url === '/warehouses') {
        return Promise.resolve({ data: [WAREHOUSE] })
      }
      return Promise.reject(new Error(`Unexpected GET ${url}`))
    })

    renderPage()

    await screen.findByText('1–20 / 25')
    const nextButton = screen.getByRole('button', { name: 'Sonraki' })
    expect(nextButton).not.toBeDisabled()

    const user = userEvent.setup()
    await user.click(nextButton)

    await waitFor(() =>
      expect(apiClient.get).toHaveBeenCalledWith(
        '/goods-issues',
        expect.objectContaining({ params: expect.objectContaining({ page: 2 }) }),
      ),
    )
  })

  it('approves a draft issue after confirming', async () => {
    vi.mocked(useHasAnyRole).mockReturnValue(true)
    vi.mocked(apiClient.post).mockResolvedValueOnce({ data: undefined })

    renderPage()
    await screen.findByText('Ankara Depo')

    const user = userEvent.setup()
    await user.click(screen.getByRole('button', { name: 'Detay' }))
    await user.click(await screen.findByRole('button', { name: 'Onayla' }))
    await user.click(await screen.findByRole('button', { name: 'Onayla' }))

    await waitFor(() =>
      expect(apiClient.post).toHaveBeenCalledWith('/goods-issues/gi1/approve'),
    )
  })
})

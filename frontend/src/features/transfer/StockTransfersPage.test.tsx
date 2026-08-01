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

import { StockTransfersPage } from './StockTransfersPage'
import type { StockTransferDto } from './types'

const DRAFT_TRANSFER: StockTransferDto = {
  id: 'st1',
  sourceWarehouseId: 'w1',
  sourceWarehouseName: 'Ankara Depo',
  destinationWarehouseId: 'w2',
  destinationWarehouseName: 'Istanbul Ana Depo',
  status: 'Draft',
  createdByUserId: 'u1',
  createdAtUtc: '2026-01-01T10:00:00',
  shippedAtUtc: null,
  receivedAtUtc: null,
  lines: [
    { productId: 'p1', productSku: 'SKU-1', productName: 'Ürün 1', quantity: 5 },
  ],
}

const SHIPPED_TRANSFER: StockTransferDto = {
  ...DRAFT_TRANSFER,
  id: 'st2',
  status: 'Shipped',
  shippedAtUtc: '2026-01-02T10:00:00',
}

const WAREHOUSES = [
  { id: 'w1', code: 'ANK-01', name: 'Ankara Depo', address: null },
  { id: 'w2', code: 'IST-01', name: 'Istanbul Ana Depo', address: null },
]

function mockListEndpoints(transfers: StockTransferDto[]) {
  vi.mocked(apiClient.get).mockImplementation((url: string) => {
    if (url === '/stock-transfers') {
      return Promise.resolve({
        data: { items: transfers, totalCount: transfers.length, page: 1, pageSize: 20 },
      })
    }
    if (url === '/warehouses') {
      return Promise.resolve({ data: WAREHOUSES })
    }
    return Promise.reject(new Error(`Unexpected GET ${url}`))
  })
}

function renderPage() {
  const queryClient = new QueryClient()
  render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <StockTransfersPage />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('StockTransfersPage', () => {
  beforeEach(() => {
    vi.mocked(apiClient.get).mockReset()
    vi.mocked(apiClient.post).mockReset()
    vi.mocked(useHasAnyRole).mockReset()
  })

  it('renders the list with source/destination, a status badge, and always shows the create link', async () => {
    vi.mocked(useHasAnyRole).mockReturnValue(false)
    mockListEndpoints([DRAFT_TRANSFER])

    renderPage()

    expect(await screen.findByText('Ankara Depo')).toBeInTheDocument()
    expect(screen.getByText('Istanbul Ana Depo')).toBeInTheDocument()
    expect(screen.getByText('Taslak')).toBeInTheDocument()
    expect(
      screen.getByRole('link', { name: /Yeni Transfer/ }),
    ).toBeInTheDocument()
  })

  it('hides the Gönder button in the detail dialog for a non-approver role', async () => {
    vi.mocked(useHasAnyRole).mockReturnValue(false)
    mockListEndpoints([DRAFT_TRANSFER])

    renderPage()
    await screen.findByText('Ankara Depo')

    const user = userEvent.setup()
    await user.click(screen.getByRole('button', { name: 'Detay' }))

    expect(await screen.findByText('SKU-1')).toBeInTheDocument()
    expect(
      screen.queryByRole('button', { name: 'Gönder' }),
    ).not.toBeInTheDocument()
  })

  it('shows the pagination summary and disables both buttons on a single page', async () => {
    vi.mocked(useHasAnyRole).mockReturnValue(true)
    mockListEndpoints([DRAFT_TRANSFER])

    renderPage()

    expect(await screen.findByText('1–1 / 1')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Önceki' })).toBeDisabled()
    expect(screen.getByRole('button', { name: 'Sonraki' })).toBeDisabled()
  })

  it('requests the next page when Sonraki is clicked', async () => {
    vi.mocked(useHasAnyRole).mockReturnValue(true)
    vi.mocked(apiClient.get).mockImplementation((url: string, config) => {
      if (url === '/stock-transfers') {
        return Promise.resolve({
          data: {
            items: [DRAFT_TRANSFER],
            totalCount: 25,
            page: (config?.params as { page?: number })?.page ?? 1,
            pageSize: 20,
          },
        })
      }
      if (url === '/warehouses') {
        return Promise.resolve({ data: WAREHOUSES })
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
        '/stock-transfers',
        expect.objectContaining({ params: expect.objectContaining({ page: 2 }) }),
      ),
    )
  })

  it('ships a draft transfer after confirming', async () => {
    vi.mocked(useHasAnyRole).mockReturnValue(true)
    mockListEndpoints([DRAFT_TRANSFER])
    vi.mocked(apiClient.post).mockResolvedValueOnce({ data: undefined })

    renderPage()
    await screen.findByText('Ankara Depo')

    const user = userEvent.setup()
    await user.click(screen.getByRole('button', { name: 'Detay' }))
    await user.click(await screen.findByRole('button', { name: 'Gönder' }))
    await user.click(await screen.findByRole('button', { name: 'Gönder' }))

    await waitFor(() =>
      expect(apiClient.post).toHaveBeenCalledWith('/stock-transfers/st1/ship'),
    )
  })

  it('receives a shipped transfer after confirming', async () => {
    vi.mocked(useHasAnyRole).mockReturnValue(true)
    mockListEndpoints([SHIPPED_TRANSFER])
    vi.mocked(apiClient.post).mockResolvedValueOnce({ data: undefined })

    renderPage()
    await screen.findByText('Gönderildi')

    const user = userEvent.setup()
    await user.click(screen.getByRole('button', { name: 'Detay' }))
    await user.click(await screen.findByRole('button', { name: 'Teslim Al' }))
    await user.click(await screen.findByRole('button', { name: 'Teslim Al' }))

    await waitFor(() =>
      expect(apiClient.post).toHaveBeenCalledWith(
        '/stock-transfers/st2/receive',
      ),
    )
  })
})

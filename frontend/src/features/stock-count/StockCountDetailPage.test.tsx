import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
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
import type { ProductDto } from '@/features/products/types'
import { apiClient } from '@/lib/axios'

import { StockCountDetailPage } from './StockCountDetailPage'
import type { StockCountDto } from './types'

const PRODUCT_A: ProductDto = {
  id: 'p1',
  sku: 'SKU-A',
  name: 'Ürün A',
  unitOfMeasureId: 'u1',
  unitOfMeasureCode: 'AD',
  categoryId: null,
  categoryName: null,
  minStockQuantity: 0,
}

function draftCount(): StockCountDto {
  return {
    id: 'sc1',
    warehouseId: 'w1',
    warehouseName: 'Ankara Depo',
    status: 'Draft',
    createdByUserId: 'u1',
    createdAtUtc: '2026-01-01T10:00:00',
    closedAtUtc: null,
    lines: [],
  }
}

function inProgressCount(): StockCountDto {
  return { ...draftCount(), status: 'InProgress' }
}

function completedCount(): StockCountDto {
  return {
    ...draftCount(),
    status: 'Completed',
    closedAtUtc: '2026-01-02T10:00:00',
    lines: [
      {
        productId: 'p1',
        productSku: 'SKU-A',
        productName: 'Ürün A',
        systemQuantity: 10,
        countedQuantity: 8,
        difference: -2,
      },
    ],
  }
}

function mockEndpoints(stockCount: StockCountDto) {
  vi.mocked(apiClient.get).mockImplementation((url: string) => {
    if (url === '/stock-counts/sc1') {
      return Promise.resolve({ data: stockCount })
    }
    if (url === '/products') {
      return Promise.resolve({
        data: { items: [PRODUCT_A], totalCount: 1, page: 1, pageSize: 20 },
      })
    }
    return Promise.reject(new Error(`Unexpected GET ${url}`))
  })
}

function renderPage() {
  const queryClient = new QueryClient()
  render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={['/stock-counts/sc1']}>
        <Routes>
          <Route path="/stock-counts/:id" element={<StockCountDetailPage />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('StockCountDetailPage', () => {
  beforeEach(() => {
    vi.mocked(apiClient.get).mockReset()
    vi.mocked(apiClient.post).mockReset()
    vi.mocked(useHasAnyRole).mockReset()
  })

  it('starts a draft stock count for a manager role', async () => {
    vi.mocked(useHasAnyRole).mockReturnValue(true)
    mockEndpoints(draftCount())
    vi.mocked(apiClient.post).mockResolvedValueOnce({ data: undefined })

    renderPage()

    const user = userEvent.setup()
    await user.click(await screen.findByRole('button', { name: 'Başlat' }))

    await waitFor(() =>
      expect(apiClient.post).toHaveBeenCalledWith('/stock-counts/sc1/start'),
    )
  })

  it('hides Başlat for a non-manager role', async () => {
    vi.mocked(useHasAnyRole).mockReturnValue(false)
    mockEndpoints(draftCount())

    renderPage()

    expect(await screen.findByText('Ankara Depo')).toBeInTheDocument()
    expect(
      screen.queryByRole('button', { name: 'Başlat' }),
    ).not.toBeInTheDocument()
  })

  it('adds a counted line via the product lookup dialog while in progress', async () => {
    vi.mocked(useHasAnyRole).mockReturnValue(true)
    mockEndpoints(inProgressCount())
    vi.mocked(apiClient.post).mockResolvedValueOnce({ data: undefined })

    renderPage()

    const user = userEvent.setup()
    await user.click(await screen.findByRole('button', { name: 'Ürün seçin' }))
    await user.click(await screen.findByRole('button', { name: 'Seç' }))
    await user.type(screen.getByRole('spinbutton'), '5')
    await user.click(screen.getByRole('button', { name: 'Satır Ekle' }))

    await waitFor(() =>
      expect(apiClient.post).toHaveBeenCalledWith('/stock-counts/sc1/lines', {
        productId: 'p1',
        countedQuantity: 5,
      }),
    )
  })

  it('shows a read-only completed count with a link to the adjustments page', async () => {
    vi.mocked(useHasAnyRole).mockReturnValue(true)
    mockEndpoints(completedCount())

    renderPage()

    expect(await screen.findByText('SKU-A')).toBeInTheDocument()
    expect(
      screen.getByRole('link', { name: 'Düzeltmeleri Görüntüle' }),
    ).toBeInTheDocument()
    expect(
      screen.queryByRole('button', { name: 'Sayımı Tamamla' }),
    ).not.toBeInTheDocument()
  })
})

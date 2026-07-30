import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

vi.mock('@/lib/axios', () => ({
  apiClient: {
    get: vi.fn(),
  },
}))

import { apiClient } from '@/lib/axios'

import { ReportsPage } from './ReportsPage'
import type {
  StockCountVarianceReportRowDto,
  StockItemDto,
  StockMovementDto,
} from './types'

const WAREHOUSE = { id: 'w1', code: 'ANK-01', name: 'Ankara Depo', address: null }

const STOCK_ITEM: StockItemDto = {
  warehouseId: 'w1',
  warehouseCode: 'ANK-01',
  warehouseName: 'Ankara Depo',
  productId: 'p1',
  productSku: 'SKU-100',
  productName: 'Test Urun',
  unitOfMeasureCode: 'ADET',
  quantity: 12,
}

const MOVEMENT: StockMovementDto = {
  id: 'm1',
  warehouseId: 'w1',
  warehouseCode: 'ANK-01',
  warehouseName: 'Ankara Depo',
  productId: 'p1',
  productSku: 'SKU-100',
  productName: 'Test Urun',
  type: 'Decrease',
  quantity: 3,
  reason: 'Stock count adjustment approved',
  occurredAtUtc: '2026-01-01T10:00:00',
}

const VARIANCE_ROW: StockCountVarianceReportRowDto = {
  stockCountId: 'sc1',
  warehouseId: 'w1',
  warehouseName: 'Ankara Depo',
  productId: 'p1',
  productSku: 'SKU-100',
  productName: 'Test Urun',
  systemQuantity: 15,
  countedQuantity: 12,
  difference: -3,
  closedAtUtc: '2026-01-01T10:05:00',
}

function mockEndpoints() {
  vi.mocked(apiClient.get).mockImplementation((url: string) => {
    if (url === '/warehouses') {
      return Promise.resolve({ data: [WAREHOUSE] })
    }
    if (url === '/stock') {
      return Promise.resolve({ data: [STOCK_ITEM] })
    }
    if (url === '/stock/movements') {
      return Promise.resolve({ data: [MOVEMENT] })
    }
    if (url === '/stock-counts/variance-report') {
      return Promise.resolve({ data: [VARIANCE_ROW] })
    }
    return Promise.reject(new Error(`Unexpected GET ${url}`))
  })
}

function renderPage() {
  const queryClient = new QueryClient()
  render(
    <QueryClientProvider client={queryClient}>
      <ReportsPage />
    </QueryClientProvider>,
  )
}

describe('ReportsPage', () => {
  beforeEach(() => {
    vi.mocked(apiClient.get).mockReset()
    mockEndpoints()
  })

  it('renders the stock levels tab by default', async () => {
    renderPage()

    expect(await screen.findByText('Ankara Depo')).toBeInTheDocument()
    expect(screen.getByText('SKU-100')).toBeInTheDocument()
    expect(screen.getByText('12')).toBeInTheDocument()
  })

  it('shows the stock movements ledger when that tab is selected', async () => {
    renderPage()
    await screen.findByText('Ankara Depo')

    const user = userEvent.setup()
    await user.click(screen.getByRole('tab', { name: 'Stok Hareketleri' }))

    expect(await screen.findByText('Azalış')).toBeInTheDocument()
    expect(
      screen.getByText('Stock count adjustment approved'),
    ).toBeInTheDocument()
  })

  it('shows the stock count variance report when that tab is selected', async () => {
    renderPage()
    await screen.findByText('Ankara Depo')

    const user = userEvent.setup()
    await user.click(screen.getByRole('tab', { name: 'Sayım Farkı' }))

    expect(await screen.findByText('-3')).toBeInTheDocument()
    expect(screen.getByText('15')).toBeInTheDocument()
  })
})

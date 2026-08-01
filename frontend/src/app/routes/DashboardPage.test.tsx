import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, within } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'

vi.mock('@/lib/axios', () => ({
  apiClient: {
    get: vi.fn(),
  },
}))

import { apiClient } from '@/lib/axios'

import { DashboardPage } from './DashboardPage'

const WAREHOUSES = [
  { id: 'w1', code: 'ANK-01', name: 'Ankara Depo', address: null },
  { id: 'w2', code: 'IST-01', name: 'İstanbul Ana Depo', address: null },
  { id: 'w3', code: 'IZM-01', name: 'İzmir Depo', address: null },
  { id: 'w4', code: 'BUR-01', name: 'Bursa Depo', address: null },
  { id: 'w5', code: 'ADN-01', name: 'Adana Depo', address: null },
]

const LOW_STOCK_ITEMS = [
  {
    warehouseId: 'w1',
    warehouseCode: 'ANK-01',
    warehouseName: 'Ankara Depo',
    productId: 'p1',
    productSku: 'SKU-100',
    productName: 'Kablosuz Mouse',
    unitOfMeasureCode: 'ADET',
    quantity: 4,
    minStockQuantity: 10,
  },
  {
    warehouseId: 'w2',
    warehouseCode: 'IST-01',
    warehouseName: 'İstanbul Ana Depo',
    productId: 'p2',
    productSku: 'SKU-200',
    productName: 'USB-C Şarj Kablosu',
    unitOfMeasureCode: 'ADET',
    quantity: 8,
    minStockQuantity: 15,
  },
]

function mockEndpoints() {
  vi.mocked(apiClient.get).mockImplementation((url: string, config) => {
    if (url === '/products') {
      return Promise.resolve({ data: { items: [], totalCount: 42, page: 1, pageSize: 1 } })
    }
    if (url === '/warehouses') {
      return Promise.resolve({ data: WAREHOUSES })
    }
    if (url === '/stock/low-stock') {
      return Promise.resolve({ data: LOW_STOCK_ITEMS })
    }
    if (url === '/goods-receipts') {
      return Promise.resolve({ data: { items: [], totalCount: 3, page: 1, pageSize: 1 } })
    }
    if (url === '/goods-issues') {
      return Promise.resolve({ data: { items: [], totalCount: 2, page: 1, pageSize: 1 } })
    }
    if (url === '/stock-transfers') {
      const status = (config?.params as { status?: string })?.status
      return Promise.resolve({
        data: { items: [], totalCount: status === 'Shipped' ? 1 : 1, page: 1, pageSize: 1 },
      })
    }
    if (url === '/stock-count-adjustments') {
      return Promise.resolve({ data: { items: [], totalCount: 4, page: 1, pageSize: 1 } })
    }
    if (url === '/stock/movements') {
      return Promise.resolve({ data: { items: [], totalCount: 0, page: 1, pageSize: 100 } })
    }
    return Promise.reject(new Error(`Unexpected GET ${url}`))
  })
}

function getStatCard(label: string) {
  const statCards = screen.getByTestId('stat-cards')
  const description = within(statCards).getByText(label)
  const card = description.closest('[data-slot="card"]')
  if (!card) throw new Error(`Could not find stat card for label "${label}"`)
  return card as HTMLElement
}

function renderPage() {
  const queryClient = new QueryClient()
  render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <DashboardPage />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('DashboardPage', () => {
  beforeEach(() => {
    vi.mocked(apiClient.get).mockReset()
    mockEndpoints()
  })

  it('renders summary stat cards with the correct totals', async () => {
    renderPage()

    expect(await within(getStatCard('Toplam Ürün')).findByText('42')).toBeInTheDocument()
    expect(await within(getStatCard('Toplam Depo')).findByText('5')).toBeInTheDocument()
    expect(await within(getStatCard('Bekleyen Onaylar')).findByText('11')).toBeInTheDocument()
    expect(await within(getStatCard('Düşük Stok Uyarısı')).findByText('2')).toBeInTheDocument()
    expect(screen.getByText('Son 7 Gün Stok Hareketleri')).toBeInTheDocument()
  })

  it('renders the low stock table rows', async () => {
    renderPage()

    expect(await screen.findByText('SKU-100')).toBeInTheDocument()
    expect(screen.getByText('Kablosuz Mouse')).toBeInTheDocument()
    expect(screen.getByText('SKU-200')).toBeInTheDocument()
  })

  it('renders the pending approvals list with links to each area', async () => {
    renderPage()

    expect(await screen.findByText('Mal Kabul (Taslak)')).toBeInTheDocument()
    expect(screen.getByText('Sevkiyat (Taslak)')).toBeInTheDocument()
    expect(screen.getByText('Transfer (Taslak + Gönderildi)')).toBeInTheDocument()
    expect(screen.getByText('Sayım Düzeltme (Bekliyor)')).toBeInTheDocument()

    const links = screen.getAllByText('Görüntüle')
    expect(links).toHaveLength(4)
  })
})

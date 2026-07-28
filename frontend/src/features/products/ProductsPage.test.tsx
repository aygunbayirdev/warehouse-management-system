import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

vi.mock('@/lib/axios', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}))

vi.mock('@/features/auth/api/useHasAnyRole', () => ({
  useHasAnyRole: vi.fn(),
}))

import { useHasAnyRole } from '@/features/auth/api/useHasAnyRole'
import { apiClient } from '@/lib/axios'

import { ProductsPage } from './ProductsPage'
import type { ProductDto } from './types'

const SAMPLE_PRODUCT: ProductDto = {
  id: 'p1',
  sku: 'SKU-1',
  name: 'Test Ürünü',
  unitOfMeasureId: 'u1',
  unitOfMeasureCode: 'AD',
  categoryId: null,
  categoryName: null,
  minStockQuantity: 5,
}

function mockListEndpoints() {
  vi.mocked(apiClient.get).mockImplementation((url: string) => {
    if (url === '/products') {
      return Promise.resolve({ data: [SAMPLE_PRODUCT] })
    }
    if (url === '/categories') {
      return Promise.resolve({ data: [] })
    }
    if (url === '/units-of-measure') {
      return Promise.resolve({ data: [] })
    }
    return Promise.reject(new Error(`Unexpected GET ${url}`))
  })
}

function renderProductsPage() {
  const queryClient = new QueryClient()
  render(
    <QueryClientProvider client={queryClient}>
      <ProductsPage />
    </QueryClientProvider>,
  )
}

describe('ProductsPage', () => {
  beforeEach(() => {
    vi.mocked(apiClient.get).mockReset()
    vi.mocked(apiClient.delete).mockReset()
    vi.mocked(useHasAnyRole).mockReset()
    mockListEndpoints()
  })

  it('renders the product list', async () => {
    vi.mocked(useHasAnyRole).mockReturnValue(true)

    renderProductsPage()

    expect(await screen.findByText('SKU-1')).toBeInTheDocument()
    expect(screen.getByText('Test Ürünü')).toBeInTheDocument()
  })

  it('hides the create button and row actions for a non-manager role', async () => {
    vi.mocked(useHasAnyRole).mockReturnValue(false)

    renderProductsPage()

    await screen.findByText('SKU-1')

    expect(
      screen.queryByRole('button', { name: /Yeni Ürün/ }),
    ).not.toBeInTheDocument()
    expect(screen.queryByLabelText('Sil')).not.toBeInTheDocument()
  })

  it('deletes a product after confirming the alert dialog', async () => {
    vi.mocked(useHasAnyRole).mockReturnValue(true)
    vi.mocked(apiClient.delete).mockResolvedValueOnce({ data: undefined })

    renderProductsPage()

    await screen.findByText('SKU-1')

    const user = userEvent.setup()
    await user.click(screen.getByLabelText('Sil'))
    await user.click(screen.getByRole('button', { name: 'Sil' }))

    await waitFor(() =>
      expect(apiClient.delete).toHaveBeenCalledWith('/products/p1'),
    )
  })
})

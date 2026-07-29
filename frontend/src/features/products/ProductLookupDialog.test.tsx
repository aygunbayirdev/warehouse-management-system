import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

vi.mock('@/lib/axios', () => ({
  apiClient: {
    get: vi.fn(),
  },
}))

import { apiClient } from '@/lib/axios'

import { ProductLookupDialog } from './ProductLookupDialog'
import type { ProductDto } from './types'

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

const PRODUCT_B: ProductDto = {
  id: 'p2',
  sku: 'SKU-B',
  name: 'Ürün B',
  unitOfMeasureId: 'u1',
  unitOfMeasureCode: 'AD',
  categoryId: null,
  categoryName: null,
  minStockQuantity: 0,
}

function mockProductsResponse(
  items: ProductDto[],
  totalCount: number = items.length,
) {
  vi.mocked(apiClient.get).mockImplementation((url: string, config) => {
    if (url === '/products') {
      const params = config?.params as { page?: number } | undefined
      return Promise.resolve({
        data: { items, totalCount, page: params?.page ?? 1, pageSize: 20 },
      })
    }
    return Promise.reject(new Error(`Unexpected GET ${url}`))
  })
}

function renderDialog(props: Partial<Parameters<typeof ProductLookupDialog>[0]> = {}) {
  const queryClient = new QueryClient()
  const onSelect = vi.fn()
  const onOpenChange = vi.fn()

  render(
    <QueryClientProvider client={queryClient}>
      <ProductLookupDialog
        open
        onOpenChange={onOpenChange}
        onSelect={onSelect}
        {...props}
      />
    </QueryClientProvider>,
  )

  return { onSelect, onOpenChange }
}

describe('ProductLookupDialog', () => {
  beforeEach(() => {
    vi.mocked(apiClient.get).mockReset()
  })

  it('renders the product results', async () => {
    mockProductsResponse([PRODUCT_A, PRODUCT_B])

    renderDialog()

    expect(await screen.findByText('SKU-A')).toBeInTheDocument()
    expect(screen.getByText('SKU-B')).toBeInTheDocument()
  })

  it('calls onSelect and closes when a result is chosen', async () => {
    mockProductsResponse([PRODUCT_A])

    const { onSelect, onOpenChange } = renderDialog()

    const user = userEvent.setup()
    await user.click(await screen.findByRole('button', { name: 'Seç' }))

    expect(onSelect).toHaveBeenCalledWith(PRODUCT_A)
    expect(onOpenChange).toHaveBeenCalledWith(false)
  })

  it('hides products listed in excludeProductIds', async () => {
    mockProductsResponse([PRODUCT_A, PRODUCT_B])

    renderDialog({ excludeProductIds: ['p2'] })

    expect(await screen.findByText('SKU-A')).toBeInTheDocument()
    expect(screen.queryByText('SKU-B')).not.toBeInTheDocument()
  })

  it('requests the next page when Sonraki is clicked', async () => {
    mockProductsResponse([PRODUCT_A], 25)

    renderDialog()

    await screen.findByText('1–20 / 25')
    const nextButton = screen.getByRole('button', { name: 'Sonraki' })
    expect(nextButton).not.toBeDisabled()

    const user = userEvent.setup()
    await user.click(nextButton)

    await waitFor(() =>
      expect(apiClient.get).toHaveBeenCalledWith(
        '/products',
        expect.objectContaining({ params: expect.objectContaining({ page: 2 }) }),
      ),
    )
  })

  it('searches with a debounced query and resets to page 1', async () => {
    mockProductsResponse([PRODUCT_A])

    renderDialog()

    const user = userEvent.setup()
    await user.type(screen.getByPlaceholderText('SKU veya ad ile ara...'), 'A')

    await waitFor(() =>
      expect(apiClient.get).toHaveBeenCalledWith(
        '/products',
        expect.objectContaining({
          params: expect.objectContaining({ search: 'A', page: 1 }),
        }),
      ),
    )
  })
})

import { useMemo, useState } from 'react'

import { Button } from '@/components/ui/button'
import { Dialog, DialogContent, DialogTitle } from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { useDebouncedValue } from '@/hooks/useDebouncedValue'

import { useProducts } from './api/products'
import type { ProductDto } from './types'

const PAGE_SIZE = 20

type ProductLookupDialogProps = {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSelect: (product: ProductDto) => void
  excludeProductIds?: string[]
}

export function ProductLookupDialog({
  open,
  onOpenChange,
  onSelect,
  excludeProductIds,
}: ProductLookupDialogProps) {
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const debouncedSearch = useDebouncedValue(search)

  const { data, isLoading } = useProducts({
    search: debouncedSearch || undefined,
    page,
    pageSize: PAGE_SIZE,
  })

  const excludeSet = useMemo(
    () => new Set(excludeProductIds ?? []),
    [excludeProductIds],
  )
  const products = (data?.items ?? []).filter(
    (product) => !excludeSet.has(product.id),
  )
  const totalCount = data?.totalCount ?? 0
  const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE))

  function handleSelect(product: ProductDto) {
    onSelect(product)
    onOpenChange(false)
  }

  function handleOpenChange(nextOpen: boolean) {
    if (nextOpen) {
      setSearch('')
      setPage(1)
    }
    onOpenChange(nextOpen)
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogTitle>Ürün Seç</DialogTitle>

        <Input
          placeholder="SKU veya ad ile ara..."
          value={search}
          onChange={(event) => {
            setSearch(event.target.value)
            setPage(1)
          }}
        />

        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>SKU</TableHead>
              <TableHead>Ad</TableHead>
              <TableHead>Birim</TableHead>
              <TableHead>Kategori</TableHead>
              <TableHead className="w-16" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {isLoading && (
              <TableRow>
                <TableCell colSpan={5}>Yükleniyor...</TableCell>
              </TableRow>
            )}
            {!isLoading && products.length === 0 && (
              <TableRow>
                <TableCell colSpan={5}>Sonuç bulunamadı.</TableCell>
              </TableRow>
            )}
            {products.map((product) => (
              <TableRow
                key={product.id}
                className="cursor-pointer"
                onClick={() => handleSelect(product)}
              >
                <TableCell>{product.sku}</TableCell>
                <TableCell>{product.name}</TableCell>
                <TableCell>{product.unitOfMeasureCode}</TableCell>
                <TableCell>{product.categoryName ?? '—'}</TableCell>
                <TableCell>
                  <Button
                    type="button"
                    size="sm"
                    onClick={(event) => {
                      event.stopPropagation()
                      handleSelect(product)
                    }}
                  >
                    Seç
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>

        <div className="flex items-center justify-between text-sm text-muted-foreground">
          <span>
            {totalCount === 0
              ? 'Kayıt yok'
              : `${(page - 1) * PAGE_SIZE + 1}–${Math.min(page * PAGE_SIZE, totalCount)} / ${totalCount}`}
          </span>
          <div className="flex items-center gap-2">
            <Button
              type="button"
              variant="outline"
              size="sm"
              disabled={page <= 1}
              onClick={() => setPage((current) => current - 1)}
            >
              Önceki
            </Button>
            <span>
              Sayfa {page} / {totalPages}
            </span>
            <Button
              type="button"
              variant="outline"
              size="sm"
              disabled={page >= totalPages}
              onClick={() => setPage((current) => current + 1)}
            >
              Sonraki
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  )
}

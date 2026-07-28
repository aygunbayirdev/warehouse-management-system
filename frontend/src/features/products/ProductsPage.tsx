import { Pencil, Plus, Trash2 } from 'lucide-react'
import { type FormEvent, useMemo, useState } from 'react'
import { toast } from 'sonner'

import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogTitle,
} from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { useHasAnyRole } from '@/features/auth/api/useHasAnyRole'
import { RoleNames } from '@/features/auth/types'
import { useDebouncedValue } from '@/hooks/useDebouncedValue'
import { getApiErrorMessage } from '@/lib/errors'

import { useCategories } from './api/categories'
import {
  useCreateProduct,
  useDeleteProduct,
  useProducts,
  useUpdateProduct,
} from './api/products'
import { useUnitsOfMeasure } from './api/unitsOfMeasure'
import type { ProductDto } from './types'

const NO_CATEGORY_VALUE = 'none'

export function ProductsPage() {
  const canManage = useHasAnyRole([RoleNames.Admin, RoleNames.WarehouseManager])

  const [search, setSearch] = useState('')
  const [categoryFilter, setCategoryFilter] = useState<string | undefined>()
  const debouncedSearch = useDebouncedValue(search)

  const { data: categories } = useCategories()
  const { data: products, isLoading } = useProducts({
    search: debouncedSearch || undefined,
    categoryId: categoryFilter,
  })

  const categoryFilterItems = useMemo(
    () => ({
      [NO_CATEGORY_VALUE]: 'Tüm kategoriler',
      ...Object.fromEntries(
        (categories ?? []).map((category) => [category.id, category.name]),
      ),
    }),
    [categories],
  )

  const [editingProduct, setEditingProduct] = useState<ProductDto | null>(
    null,
  )
  const [isFormOpen, setIsFormOpen] = useState(false)
  const [deletingProduct, setDeletingProduct] = useState<ProductDto | null>(
    null,
  )

  function openCreateForm() {
    setEditingProduct(null)
    setIsFormOpen(true)
  }

  function openEditForm(product: ProductDto) {
    setEditingProduct(product)
    setIsFormOpen(true)
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold">Ürünler</h2>
        {canManage && (
          <Button onClick={openCreateForm}>
            <Plus /> Yeni Ürün
          </Button>
        )}
      </div>

      <div className="flex gap-2">
        <Input
          placeholder="SKU veya ad ile ara..."
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          className="max-w-xs"
        />
        <Select
          items={categoryFilterItems}
          value={categoryFilter ?? NO_CATEGORY_VALUE}
          onValueChange={(value: string | null) =>
            setCategoryFilter(
              !value || value === NO_CATEGORY_VALUE ? undefined : value,
            )
          }
        >
          <SelectTrigger>
            <SelectValue placeholder="Tüm kategoriler" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={NO_CATEGORY_VALUE}>Tüm kategoriler</SelectItem>
            {categories?.map((category) => (
              <SelectItem key={category.id} value={category.id}>
                {category.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>SKU</TableHead>
            <TableHead>Ad</TableHead>
            <TableHead>Birim</TableHead>
            <TableHead>Kategori</TableHead>
            <TableHead>Min. Stok</TableHead>
            {canManage && <TableHead className="w-24">Aksiyonlar</TableHead>}
          </TableRow>
        </TableHeader>
        <TableBody>
          {isLoading && (
            <TableRow>
              <TableCell colSpan={6}>Yükleniyor...</TableCell>
            </TableRow>
          )}
          {products?.map((product) => (
            <TableRow key={product.id}>
              <TableCell>{product.sku}</TableCell>
              <TableCell>{product.name}</TableCell>
              <TableCell>{product.unitOfMeasureCode}</TableCell>
              <TableCell>{product.categoryName ?? '—'}</TableCell>
              <TableCell>{product.minStockQuantity}</TableCell>
              {canManage && (
                <TableCell>
                  <div className="flex gap-1">
                    <Button
                      variant="ghost"
                      size="icon-sm"
                      aria-label="Düzenle"
                      onClick={() => openEditForm(product)}
                    >
                      <Pencil />
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon-sm"
                      aria-label="Sil"
                      onClick={() => setDeletingProduct(product)}
                    >
                      <Trash2 />
                    </Button>
                  </div>
                </TableCell>
              )}
            </TableRow>
          ))}
        </TableBody>
      </Table>

      <ProductFormDialog
        key={editingProduct?.id ?? 'new'}
        open={isFormOpen}
        onOpenChange={setIsFormOpen}
        product={editingProduct}
      />

      <DeleteProductDialog
        product={deletingProduct}
        onOpenChange={(open) => {
          if (!open) setDeletingProduct(null)
        }}
      />
    </div>
  )
}

function ProductFormDialog({
  open,
  onOpenChange,
  product,
}: {
  open: boolean
  onOpenChange: (open: boolean) => void
  product: ProductDto | null
}) {
  const { data: categories } = useCategories()
  const { data: units } = useUnitsOfMeasure()

  const [sku, setSku] = useState(product?.sku ?? '')
  const [name, setName] = useState(product?.name ?? '')
  const [unitOfMeasureId, setUnitOfMeasureId] = useState(
    product?.unitOfMeasureId ?? '',
  )
  const [categoryId, setCategoryId] = useState(product?.categoryId ?? '')
  const [minStockQuantity, setMinStockQuantity] = useState(
    product?.minStockQuantity ?? 0,
  )

  const unitItems = useMemo(
    () =>
      Object.fromEntries(
        (units ?? []).map((unit) => [unit.id, `${unit.name} (${unit.code})`]),
      ),
    [units],
  )

  const categoryItems = useMemo(
    () => ({
      [NO_CATEGORY_VALUE]: 'Kategorisiz',
      ...Object.fromEntries(
        (categories ?? []).map((category) => [category.id, category.name]),
      ),
    }),
    [categories],
  )

  const createProduct = useCreateProduct()
  const updateProduct = useUpdateProduct()
  const mutation = product ? updateProduct : createProduct

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (product) {
      updateProduct.mutate(
        {
          id: product.id,
          payload: {
            name,
            unitOfMeasureId,
            categoryId: categoryId || null,
            minStockQuantity,
          },
        },
        {
          onSuccess: () => {
            toast.success('Ürün güncellendi.')
            onOpenChange(false)
          },
        },
      )
    } else {
      createProduct.mutate(
        {
          sku,
          name,
          unitOfMeasureId,
          categoryId: categoryId || null,
          minStockQuantity,
        },
        {
          onSuccess: () => {
            toast.success('Ürün oluşturuldu.')
            onOpenChange(false)
          },
        },
      )
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <form onSubmit={handleSubmit} className="space-y-4">
          <DialogTitle>{product ? 'Ürünü Düzenle' : 'Yeni Ürün'}</DialogTitle>

          <div className="space-y-1.5">
            <Label htmlFor="product-sku">SKU</Label>
            <Input
              id="product-sku"
              required
              maxLength={50}
              disabled={Boolean(product)}
              value={sku}
              onChange={(event) => setSku(event.target.value)}
            />
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="product-name">Ad</Label>
            <Input
              id="product-name"
              required
              maxLength={200}
              value={name}
              onChange={(event) => setName(event.target.value)}
            />
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="product-unit">Birim</Label>
            <Select
              items={unitItems}
              value={unitOfMeasureId}
              onValueChange={(value: string | null) =>
                setUnitOfMeasureId(value ?? '')
              }
            >
              <SelectTrigger id="product-unit" className="w-full">
                <SelectValue placeholder="Birim seçin" />
              </SelectTrigger>
              <SelectContent>
                {units?.map((unit) => (
                  <SelectItem key={unit.id} value={unit.id}>
                    {unit.name} ({unit.code})
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="product-category">Kategori</Label>
            <Select
              items={categoryItems}
              value={categoryId || NO_CATEGORY_VALUE}
              onValueChange={(value: string | null) =>
                setCategoryId(!value || value === NO_CATEGORY_VALUE ? '' : value)
              }
            >
              <SelectTrigger id="product-category" className="w-full">
                <SelectValue placeholder="Kategori seçin" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value={NO_CATEGORY_VALUE}>Kategorisiz</SelectItem>
                {categories?.map((category) => (
                  <SelectItem key={category.id} value={category.id}>
                    {category.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="product-min-stock">Min. Stok</Label>
            <Input
              id="product-min-stock"
              type="number"
              min={0}
              required
              value={minStockQuantity}
              onChange={(event) =>
                setMinStockQuantity(Number(event.target.value))
              }
            />
          </div>

          {mutation.isError && (
            <p className="text-sm text-destructive">
              {getApiErrorMessage(mutation.error)}
            </p>
          )}

          <DialogFooter>
            <Button
              type="submit"
              disabled={mutation.isPending || !unitOfMeasureId}
            >
              {mutation.isPending ? 'Kaydediliyor...' : 'Kaydet'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

function DeleteProductDialog({
  product,
  onOpenChange,
}: {
  product: ProductDto | null
  onOpenChange: (open: boolean) => void
}) {
  const deleteProduct = useDeleteProduct()

  function handleConfirm() {
    if (!product) return

    deleteProduct.mutate(product.id, {
      onSuccess: () => {
        toast.success('Ürün silindi.')
        onOpenChange(false)
      },
      onError: (error) => {
        toast.error(getApiErrorMessage(error))
        onOpenChange(false)
      },
    })
  }

  return (
    <AlertDialog open={product !== null} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogTitle>Ürünü sil</AlertDialogTitle>
        <AlertDialogDescription>
          &quot;{product?.name}&quot; ürününü silmek istediğinize emin misiniz?
        </AlertDialogDescription>
        <AlertDialogFooter>
          <AlertDialogCancel>Vazgeç</AlertDialogCancel>
          <AlertDialogAction
            onClick={handleConfirm}
            disabled={deleteProduct.isPending}
          >
            Sil
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}

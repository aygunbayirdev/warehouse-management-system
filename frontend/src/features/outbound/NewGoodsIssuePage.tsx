import { Plus, Trash2 } from 'lucide-react'
import { type FormEvent, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { useProducts } from '@/features/products/api/products'
import { useWarehouses } from '@/features/warehouses/api/warehouses'
import { getApiErrorMessage } from '@/lib/errors'

import { useCreateGoodsIssue } from './api/goodsIssues'

type LineState = {
  key: string
  productId: string
  quantity: string
}

function createEmptyLine(): LineState {
  return { key: crypto.randomUUID(), productId: '', quantity: '' }
}

export function NewGoodsIssuePage() {
  const navigate = useNavigate()
  const { data: warehouses } = useWarehouses()
  const { data: products } = useProducts({})
  const createGoodsIssue = useCreateGoodsIssue()

  const [warehouseId, setWarehouseId] = useState('')
  const [destination, setDestination] = useState('')
  const [lines, setLines] = useState<LineState[]>([createEmptyLine()])

  const warehouseItems = useMemo(
    () =>
      Object.fromEntries(
        (warehouses ?? []).map((warehouse) => [warehouse.id, warehouse.name]),
      ),
    [warehouses],
  )
  const productItems = useMemo(
    () =>
      Object.fromEntries(
        (products ?? []).map((product) => [
          product.id,
          `${product.name} (${product.sku})`,
        ]),
      ),
    [products],
  )

  function updateLine(key: string, patch: Partial<LineState>) {
    setLines((current) =>
      current.map((line) => (line.key === key ? { ...line, ...patch } : line)),
    )
  }

  function addLine() {
    setLines((current) => [...current, createEmptyLine()])
  }

  function removeLine(key: string) {
    setLines((current) => current.filter((line) => line.key !== key))
  }

  const selectedProductIds = lines
    .map((line) => line.productId)
    .filter(Boolean)
  const hasDuplicateProduct =
    new Set(selectedProductIds).size !== selectedProductIds.length

  const isValid =
    Boolean(warehouseId) &&
    Boolean(destination) &&
    lines.length > 0 &&
    lines.every((line) => line.productId && Number(line.quantity) > 0) &&
    !hasDuplicateProduct

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!isValid) return

    createGoodsIssue.mutate(
      {
        warehouseId,
        destination,
        lines: lines.map((line) => ({
          productId: line.productId,
          quantity: Number(line.quantity),
        })),
      },
      {
        onSuccess: () => {
          toast.success('Sevkiyat oluşturuldu.')
          navigate('/goods-issues')
        },
      },
    )
  }

  return (
    <div className="max-w-2xl space-y-4">
      <h2 className="text-xl font-semibold">Yeni Sevkiyat</h2>

      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="space-y-1.5">
          <Label htmlFor="warehouse">Depo</Label>
          <Select
            items={warehouseItems}
            value={warehouseId}
            onValueChange={(value: string | null) =>
              setWarehouseId(value ?? '')
            }
          >
            <SelectTrigger id="warehouse" className="w-full">
              <SelectValue placeholder="Depo seçin" />
            </SelectTrigger>
            <SelectContent>
              {warehouses?.map((warehouse) => (
                <SelectItem key={warehouse.id} value={warehouse.id}>
                  {warehouse.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="destination">Hedef</Label>
          <Input
            id="destination"
            required
            maxLength={200}
            value={destination}
            onChange={(event) => setDestination(event.target.value)}
          />
        </div>

        <div className="space-y-2">
          <Label>Satırlar</Label>
          {lines.map((line) => (
            <div key={line.key} className="flex items-center gap-2">
              <div className="flex-1">
                <Select
                  items={productItems}
                  value={line.productId}
                  onValueChange={(value: string | null) =>
                    updateLine(line.key, { productId: value ?? '' })
                  }
                >
                  <SelectTrigger className="w-full">
                    <SelectValue placeholder="Ürün seçin" />
                  </SelectTrigger>
                  <SelectContent>
                    {products?.map((product) => (
                      <SelectItem key={product.id} value={product.id}>
                        {product.name} ({product.sku})
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <Input
                type="number"
                min={0.01}
                step="any"
                placeholder="Miktar"
                className="w-28"
                value={line.quantity}
                onChange={(event) =>
                  updateLine(line.key, { quantity: event.target.value })
                }
              />
              <Button
                type="button"
                variant="ghost"
                size="icon-sm"
                aria-label="Satırı kaldır"
                onClick={() => removeLine(line.key)}
                disabled={lines.length === 1}
              >
                <Trash2 />
              </Button>
            </div>
          ))}
          <Button type="button" variant="outline" onClick={addLine}>
            <Plus /> Satır Ekle
          </Button>
          {hasDuplicateProduct && (
            <p className="text-sm text-destructive">
              Aynı ürün birden fazla satırda seçilemez.
            </p>
          )}
        </div>

        {createGoodsIssue.isError && (
          <p className="text-sm text-destructive">
            {getApiErrorMessage(createGoodsIssue.error)}
          </p>
        )}

        <div className="flex gap-2">
          <Button
            type="submit"
            disabled={!isValid || createGoodsIssue.isPending}
          >
            {createGoodsIssue.isPending ? 'Kaydediliyor...' : 'Kaydet'}
          </Button>
          <Button
            type="button"
            variant="outline"
            onClick={() => navigate('/goods-issues')}
          >
            İptal
          </Button>
        </div>
      </form>
    </div>
  )
}

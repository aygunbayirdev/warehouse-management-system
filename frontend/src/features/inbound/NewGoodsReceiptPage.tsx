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
import { ProductLookupDialog } from '@/features/products/ProductLookupDialog'
import { useWarehouses } from '@/features/warehouses/api/warehouses'
import { getApiErrorMessage } from '@/lib/errors'

import { useCreateGoodsReceipt } from './api/goodsReceipts'

type LineState = {
  key: string
  productId: string
  productLabel: string
  quantity: string
}

function createEmptyLine(): LineState {
  return { key: crypto.randomUUID(), productId: '', productLabel: '', quantity: '' }
}

export function NewGoodsReceiptPage() {
  const navigate = useNavigate()
  const { data: warehouses } = useWarehouses()
  const createGoodsReceipt = useCreateGoodsReceipt()

  const [warehouseId, setWarehouseId] = useState('')
  const [lines, setLines] = useState<LineState[]>([createEmptyLine()])
  const [pickingLineKey, setPickingLineKey] = useState<string | null>(null)

  const warehouseItems = useMemo(
    () =>
      Object.fromEntries(
        (warehouses ?? []).map((warehouse) => [warehouse.id, warehouse.name]),
      ),
    [warehouses],
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

  const isValid =
    Boolean(warehouseId) &&
    lines.length > 0 &&
    lines.every((line) => line.productId && Number(line.quantity) > 0)

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!isValid) return

    createGoodsReceipt.mutate(
      {
        warehouseId,
        lines: lines.map((line) => ({
          productId: line.productId,
          quantity: Number(line.quantity),
        })),
      },
      {
        onSuccess: () => {
          toast.success('Mal kabul oluşturuldu.')
          navigate('/goods-receipts')
        },
      },
    )
  }

  return (
    <div className="max-w-2xl space-y-4">
      <h2 className="text-xl font-semibold">Yeni Mal Kabul</h2>

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

        <div className="space-y-2">
          <Label>Satırlar</Label>
          {lines.map((line) => (
            <div key={line.key} className="flex items-center gap-2">
              <Button
                type="button"
                variant="outline"
                className="flex-1 justify-start font-normal"
                onClick={() => setPickingLineKey(line.key)}
              >
                {line.productLabel || 'Ürün seçin'}
              </Button>
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
        </div>

        {createGoodsReceipt.isError && (
          <p className="text-sm text-destructive">
            {getApiErrorMessage(createGoodsReceipt.error)}
          </p>
        )}

        <div className="flex gap-2">
          <Button
            type="submit"
            disabled={!isValid || createGoodsReceipt.isPending}
          >
            {createGoodsReceipt.isPending ? 'Kaydediliyor...' : 'Kaydet'}
          </Button>
          <Button
            type="button"
            variant="outline"
            onClick={() => navigate('/goods-receipts')}
          >
            İptal
          </Button>
        </div>
      </form>

      <ProductLookupDialog
        open={pickingLineKey !== null}
        onOpenChange={(open) => {
          if (!open) setPickingLineKey(null)
        }}
        onSelect={(product) => {
          if (pickingLineKey) {
            updateLine(pickingLineKey, {
              productId: product.id,
              productLabel: `${product.name} (${product.sku})`,
            })
          }
        }}
      />
    </div>
  )
}

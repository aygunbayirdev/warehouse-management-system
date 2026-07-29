import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
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
import { Button, buttonVariants } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
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
import { ProductLookupDialog } from '@/features/products/ProductLookupDialog'
import { formatUtcDateTime } from '@/lib/dates'
import { getApiErrorMessage } from '@/lib/errors'
import { cn } from '@/lib/utils'

import {
  useCompleteStockCount,
  useStartStockCount,
  useStockCount,
  useSubmitStockCountLine,
} from './api/stockCounts'
import type { StockCountStatus } from './types'

const SESSION_MANAGE_ROLES = [
  RoleNames.Admin,
  RoleNames.WarehouseManager,
  RoleNames.WarehouseSupervisor,
]

const STATUS_LABELS: Record<StockCountStatus, string> = {
  Draft: 'Taslak',
  InProgress: 'Devam Ediyor',
  Completed: 'Tamamlandı',
}

function DifferenceCell({ value }: { value: number }) {
  return (
    <span
      className={cn(
        value > 0 && 'text-green-600 dark:text-green-400',
        value < 0 && 'text-destructive',
      )}
    >
      {value > 0 ? `+${value}` : value}
    </span>
  )
}

export function StockCountDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const canManage = useHasAnyRole(SESSION_MANAGE_ROLES)

  const { data: stockCount, isLoading } = useStockCount(id ?? '')
  const startStockCount = useStartStockCount()
  const submitLine = useSubmitStockCountLine(id ?? '')

  const [isPickingProduct, setIsPickingProduct] = useState(false)
  const [pickedProduct, setPickedProduct] = useState<{
    id: string
    label: string
  } | null>(null)
  const [countedQuantity, setCountedQuantity] = useState('')
  const [isCompleteOpen, setIsCompleteOpen] = useState(false)

  if (isLoading || !stockCount) {
    return <p>Yükleniyor...</p>
  }

  const countedProductIds = stockCount.lines.map((line) => line.productId)

  function handleStart() {
    if (!id) return
    startStockCount.mutate(id, {
      onError: (error) => toast.error(getApiErrorMessage(error)),
    })
  }

  function handleAddLine() {
    if (!pickedProduct || !Number(countedQuantity)) return

    submitLine.mutate(
      { productId: pickedProduct.id, countedQuantity: Number(countedQuantity) },
      {
        onSuccess: () => {
          toast.success('Satır eklendi.')
          setPickedProduct(null)
          setCountedQuantity('')
        },
        onError: (error) => toast.error(getApiErrorMessage(error)),
      },
    )
  }

  return (
    <div className="max-w-3xl space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold">Sayım Detayı</h2>
        <Button variant="outline" onClick={() => navigate('/stock-counts')}>
          Listeye Dön
        </Button>
      </div>

      <div className="space-y-1 text-sm">
        <p>
          <span className="text-muted-foreground">Depo: </span>
          {stockCount.warehouseName}
        </p>
        <p>
          <span className="text-muted-foreground">Durum: </span>
          {STATUS_LABELS[stockCount.status]}
        </p>
        <p>
          <span className="text-muted-foreground">Oluşturulma Tarihi: </span>
          {formatUtcDateTime(stockCount.createdAtUtc)}
        </p>
        {stockCount.closedAtUtc && (
          <p>
            <span className="text-muted-foreground">Kapanış Tarihi: </span>
            {formatUtcDateTime(stockCount.closedAtUtc)}
          </p>
        )}
      </div>

      {stockCount.status === 'Draft' && canManage && (
        <Button onClick={handleStart} disabled={startStockCount.isPending}>
          {startStockCount.isPending ? 'Başlatılıyor...' : 'Başlat'}
        </Button>
      )}

      {stockCount.status === 'InProgress' && (
        <div className="flex items-end gap-2 rounded-md border p-3">
          <div className="flex-1 space-y-1.5">
            <p className="text-sm text-muted-foreground">Ürün</p>
            <Button
              type="button"
              variant="outline"
              className="w-full justify-start font-normal"
              onClick={() => setIsPickingProduct(true)}
            >
              {pickedProduct?.label ?? 'Ürün seçin'}
            </Button>
          </div>
          <div className="space-y-1.5">
            <p className="text-sm text-muted-foreground">Sayılan Miktar</p>
            <Input
              type="number"
              min={0}
              step="any"
              className="w-28"
              value={countedQuantity}
              onChange={(event) => setCountedQuantity(event.target.value)}
            />
          </div>
          <Button
            type="button"
            onClick={handleAddLine}
            disabled={
              !pickedProduct || !countedQuantity || submitLine.isPending
            }
          >
            Satır Ekle
          </Button>
        </div>
      )}

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>SKU</TableHead>
            <TableHead>Ürün</TableHead>
            <TableHead>Sistem Miktarı</TableHead>
            <TableHead>Sayılan Miktar</TableHead>
            <TableHead>Fark</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {stockCount.lines.length === 0 && (
            <TableRow>
              <TableCell colSpan={5}>Henüz satır girilmedi.</TableCell>
            </TableRow>
          )}
          {stockCount.lines.map((line) => (
            <TableRow key={line.productId}>
              <TableCell>{line.productSku}</TableCell>
              <TableCell>{line.productName}</TableCell>
              <TableCell>{line.systemQuantity}</TableCell>
              <TableCell>{line.countedQuantity}</TableCell>
              <TableCell>
                <DifferenceCell value={line.difference} />
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>

      {stockCount.status === 'InProgress' && canManage && (
        <Button
          variant="outline"
          disabled={stockCount.lines.length === 0}
          onClick={() => setIsCompleteOpen(true)}
        >
          Sayımı Tamamla
        </Button>
      )}

      {stockCount.status === 'Completed' && (
        <p className="text-sm text-muted-foreground">
          Sayım tamamlandı. Farkı olan satırlar için düzeltme kaydı
          oluşturuldu —{' '}
          <Link
            to="/stock-count-adjustments"
            className={buttonVariants({ variant: 'link', className: 'h-auto p-0' })}
          >
            Düzeltmeleri Görüntüle
          </Link>
          .
        </p>
      )}

      <ProductLookupDialog
        open={isPickingProduct}
        onOpenChange={setIsPickingProduct}
        excludeProductIds={countedProductIds}
        onSelect={(product) =>
          setPickedProduct({
            id: product.id,
            label: `${product.name} (${product.sku})`,
          })
        }
      />

      <AlertDialog open={isCompleteOpen} onOpenChange={setIsCompleteOpen}>
        <AlertDialogContent>
          <AlertDialogTitle>Sayımı tamamla</AlertDialogTitle>
          <AlertDialogDescription>
            Sayımı tamamlamak istediğinize emin misiniz? Tamamlandıktan sonra
            yeni satır giremezsiniz; farkı olan her satır için onay bekleyen
            bir düzeltme kaydı otomatik oluşturulacaktır.
          </AlertDialogDescription>
          <AlertDialogFooter>
            <AlertDialogCancel>Vazgeç</AlertDialogCancel>
            <CompleteAction id={id ?? ''} onDone={() => setIsCompleteOpen(false)} />
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  )
}

function CompleteAction({
  id,
  onDone,
}: {
  id: string
  onDone: () => void
}) {
  const completeStockCount = useCompleteStockCount()

  function handleConfirm() {
    completeStockCount.mutate(id, {
      onSuccess: () => {
        toast.success('Sayım tamamlandı.')
        onDone()
      },
      onError: (error) => {
        toast.error(getApiErrorMessage(error))
        onDone()
      },
    })
  }

  return (
    <AlertDialogAction
      onClick={handleConfirm}
      disabled={completeStockCount.isPending}
    >
      Tamamla
    </AlertDialogAction>
  )
}

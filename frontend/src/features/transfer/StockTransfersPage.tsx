import { Plus } from 'lucide-react'
import { useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
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
import { Dialog, DialogContent, DialogFooter, DialogTitle } from '@/components/ui/dialog'
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
import { useWarehouses } from '@/features/warehouses/api/warehouses'
import { cn } from '@/lib/utils'
import { formatUtcDateTime } from '@/lib/dates'
import { getApiErrorMessage } from '@/lib/errors'

import {
  useReceiveStockTransfer,
  useShipStockTransfer,
  useStockTransfers,
} from './api/stockTransfers'
import type { StockTransferDto, StockTransferStatus } from './types'

const ALL_VALUE = 'all'
const SHIP_RECEIVE_ROLES = [
  RoleNames.Admin,
  RoleNames.WarehouseManager,
  RoleNames.WarehouseSupervisor,
]

const STATUS_ITEMS: Record<string, string> = {
  [ALL_VALUE]: 'Tüm durumlar',
  Draft: 'Taslak',
  Shipped: 'Gönderildi',
  Received: 'Teslim Alındı',
  Cancelled: 'İptal Edildi',
}

function StatusBadge({ status }: { status: StockTransferStatus }) {
  const styles: Record<StockTransferStatus, string> = {
    Draft:
      'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/40 dark:text-yellow-300',
    Shipped:
      'bg-blue-100 text-blue-800 dark:bg-blue-900/40 dark:text-blue-300',
    Received:
      'bg-green-100 text-green-800 dark:bg-green-900/40 dark:text-green-300',
    Cancelled:
      'bg-gray-100 text-gray-800 dark:bg-gray-900/40 dark:text-gray-300',
  }
  const labels: Record<StockTransferStatus, string> = {
    Draft: 'Taslak',
    Shipped: 'Gönderildi',
    Received: 'Teslim Alındı',
    Cancelled: 'İptal Edildi',
  }

  return (
    <span
      className={cn(
        'rounded-full px-2 py-0.5 text-xs font-medium',
        styles[status],
      )}
    >
      {labels[status]}
    </span>
  )
}

export function StockTransfersPage() {
  const canShipReceive = useHasAnyRole(SHIP_RECEIVE_ROLES)

  const [sourceFilter, setSourceFilter] = useState<string | undefined>()
  const [destinationFilter, setDestinationFilter] = useState<
    string | undefined
  >()
  const [statusFilter, setStatusFilter] = useState<
    StockTransferStatus | undefined
  >()

  const { data: warehouses } = useWarehouses()
  const { data: transfers, isLoading } = useStockTransfers({
    sourceWarehouseId: sourceFilter,
    destinationWarehouseId: destinationFilter,
    status: statusFilter,
  })

  const warehouseItems = useMemo(
    () => ({
      [ALL_VALUE]: 'Tüm depolar',
      ...Object.fromEntries(
        (warehouses ?? []).map((warehouse) => [warehouse.id, warehouse.name]),
      ),
    }),
    [warehouses],
  )

  const [viewingTransfer, setViewingTransfer] =
    useState<StockTransferDto | null>(null)
  const [shippingTransfer, setShippingTransfer] =
    useState<StockTransferDto | null>(null)
  const [receivingTransfer, setReceivingTransfer] =
    useState<StockTransferDto | null>(null)

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold">Transfer</h2>
        <Link
          to="/stock-transfers/new"
          className={buttonVariants({ variant: 'default' })}
        >
          <Plus /> Yeni Transfer
        </Link>
      </div>

      <div className="flex gap-2">
        <Select
          items={warehouseItems}
          value={sourceFilter ?? ALL_VALUE}
          onValueChange={(value: string | null) =>
            setSourceFilter(!value || value === ALL_VALUE ? undefined : value)
          }
        >
          <SelectTrigger>
            <SelectValue placeholder="Kaynak depo" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={ALL_VALUE}>Tüm depolar (Kaynak)</SelectItem>
            {warehouses?.map((warehouse) => (
              <SelectItem key={warehouse.id} value={warehouse.id}>
                {warehouse.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Select
          items={warehouseItems}
          value={destinationFilter ?? ALL_VALUE}
          onValueChange={(value: string | null) =>
            setDestinationFilter(
              !value || value === ALL_VALUE ? undefined : value,
            )
          }
        >
          <SelectTrigger>
            <SelectValue placeholder="Hedef depo" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={ALL_VALUE}>Tüm depolar (Hedef)</SelectItem>
            {warehouses?.map((warehouse) => (
              <SelectItem key={warehouse.id} value={warehouse.id}>
                {warehouse.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Select
          items={STATUS_ITEMS}
          value={statusFilter ?? ALL_VALUE}
          onValueChange={(value: string | null) =>
            setStatusFilter(
              !value || value === ALL_VALUE
                ? undefined
                : (value as StockTransferStatus),
            )
          }
        >
          <SelectTrigger>
            <SelectValue placeholder="Tüm durumlar" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={ALL_VALUE}>Tüm durumlar</SelectItem>
            <SelectItem value="Draft">Taslak</SelectItem>
            <SelectItem value="Shipped">Gönderildi</SelectItem>
            <SelectItem value="Received">Teslim Alındı</SelectItem>
          </SelectContent>
        </Select>
      </div>

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Kaynak Depo</TableHead>
            <TableHead>Hedef Depo</TableHead>
            <TableHead>Durum</TableHead>
            <TableHead>Satır Sayısı</TableHead>
            <TableHead>Oluşturulma Tarihi</TableHead>
            <TableHead className="w-24">Detay</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {isLoading && (
            <TableRow>
              <TableCell colSpan={6}>Yükleniyor...</TableCell>
            </TableRow>
          )}
          {transfers?.map((transfer) => (
            <TableRow key={transfer.id}>
              <TableCell>{transfer.sourceWarehouseName}</TableCell>
              <TableCell>{transfer.destinationWarehouseName}</TableCell>
              <TableCell>
                <StatusBadge status={transfer.status} />
              </TableCell>
              <TableCell>{transfer.lines.length}</TableCell>
              <TableCell>{formatUtcDateTime(transfer.createdAtUtc)}</TableCell>
              <TableCell>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => setViewingTransfer(transfer)}
                >
                  Detay
                </Button>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>

      <Dialog
        open={viewingTransfer !== null}
        onOpenChange={(open) => {
          if (!open) setViewingTransfer(null)
        }}
      >
        <DialogContent>
          {viewingTransfer && (
            <div className="space-y-4">
              <DialogTitle>Transfer Detayı</DialogTitle>

              <div className="space-y-1 text-sm">
                <p>
                  <span className="text-muted-foreground">Kaynak Depo: </span>
                  {viewingTransfer.sourceWarehouseName}
                </p>
                <p>
                  <span className="text-muted-foreground">Hedef Depo: </span>
                  {viewingTransfer.destinationWarehouseName}
                </p>
                <p>
                  <span className="text-muted-foreground">Durum: </span>
                  <StatusBadge status={viewingTransfer.status} />
                </p>
                <p>
                  <span className="text-muted-foreground">
                    Oluşturulma Tarihi:{' '}
                  </span>
                  {formatUtcDateTime(viewingTransfer.createdAtUtc)}
                </p>
                {viewingTransfer.shippedAtUtc && (
                  <p>
                    <span className="text-muted-foreground">
                      Gönderim Tarihi:{' '}
                    </span>
                    {formatUtcDateTime(viewingTransfer.shippedAtUtc)}
                  </p>
                )}
                {viewingTransfer.receivedAtUtc && (
                  <p>
                    <span className="text-muted-foreground">
                      Teslim Tarihi:{' '}
                    </span>
                    {formatUtcDateTime(viewingTransfer.receivedAtUtc)}
                  </p>
                )}
              </div>

              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>SKU</TableHead>
                    <TableHead>Ürün</TableHead>
                    <TableHead>Miktar</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {viewingTransfer.lines.map((line) => (
                    <TableRow key={line.productId}>
                      <TableCell>{line.productSku}</TableCell>
                      <TableCell>{line.productName}</TableCell>
                      <TableCell>{line.quantity}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>

              {viewingTransfer.status === 'Draft' && canShipReceive && (
                <DialogFooter>
                  <Button
                    onClick={() => {
                      setShippingTransfer(viewingTransfer)
                      setViewingTransfer(null)
                    }}
                  >
                    Gönder
                  </Button>
                </DialogFooter>
              )}

              {viewingTransfer.status === 'Shipped' && canShipReceive && (
                <DialogFooter>
                  <Button
                    onClick={() => {
                      setReceivingTransfer(viewingTransfer)
                      setViewingTransfer(null)
                    }}
                  >
                    Teslim Al
                  </Button>
                </DialogFooter>
              )}
            </div>
          )}
        </DialogContent>
      </Dialog>

      <ShipStockTransferDialog
        transfer={shippingTransfer}
        onOpenChange={(open) => {
          if (!open) setShippingTransfer(null)
        }}
      />

      <ReceiveStockTransferDialog
        transfer={receivingTransfer}
        onOpenChange={(open) => {
          if (!open) setReceivingTransfer(null)
        }}
      />
    </div>
  )
}

function ShipStockTransferDialog({
  transfer,
  onOpenChange,
}: {
  transfer: StockTransferDto | null
  onOpenChange: (open: boolean) => void
}) {
  const shipStockTransfer = useShipStockTransfer()

  function handleConfirm() {
    if (!transfer) return

    shipStockTransfer.mutate(transfer.id, {
      onSuccess: () => {
        toast.success('Transfer gönderildi.')
        onOpenChange(false)
      },
      onError: (error) => {
        toast.error(getApiErrorMessage(error))
        onOpenChange(false)
      },
    })
  }

  return (
    <AlertDialog open={transfer !== null} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogTitle>Transferi gönder</AlertDialogTitle>
        <AlertDialogDescription>
          Bu transferi göndermek istediğinize emin misiniz? Gönderildiğinde
          kaynak depodaki stok miktarları otomatik olarak azaltılacaktır.
        </AlertDialogDescription>
        <AlertDialogFooter>
          <AlertDialogCancel>Vazgeç</AlertDialogCancel>
          <AlertDialogAction
            onClick={handleConfirm}
            disabled={shipStockTransfer.isPending}
          >
            Gönder
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}

function ReceiveStockTransferDialog({
  transfer,
  onOpenChange,
}: {
  transfer: StockTransferDto | null
  onOpenChange: (open: boolean) => void
}) {
  const receiveStockTransfer = useReceiveStockTransfer()

  function handleConfirm() {
    if (!transfer) return

    receiveStockTransfer.mutate(transfer.id, {
      onSuccess: () => {
        toast.success('Transfer teslim alındı.')
        onOpenChange(false)
      },
      onError: (error) => {
        toast.error(getApiErrorMessage(error))
        onOpenChange(false)
      },
    })
  }

  return (
    <AlertDialog open={transfer !== null} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogTitle>Transferi teslim al</AlertDialogTitle>
        <AlertDialogDescription>
          Bu transferi teslim almak istediğinize emin misiniz? Teslim
          alındığında hedef depodaki stok miktarları otomatik olarak
          artırılacaktır.
        </AlertDialogDescription>
        <AlertDialogFooter>
          <AlertDialogCancel>Vazgeç</AlertDialogCancel>
          <AlertDialogAction
            onClick={handleConfirm}
            disabled={receiveStockTransfer.isPending}
          >
            Teslim Al
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}

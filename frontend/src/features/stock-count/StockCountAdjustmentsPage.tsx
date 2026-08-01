import { useMemo, useState } from 'react'
import { toast } from 'sonner'

import { PaginationControls } from '@/components/PaginationControls'
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
import { formatUtcDateTime } from '@/lib/dates'
import { getApiErrorMessage } from '@/lib/errors'
import { cn } from '@/lib/utils'

import {
  useApproveStockCountAdjustment,
  useRejectStockCountAdjustment,
  useStockCountAdjustments,
} from './api/stockCountAdjustments'
import type { StockCountAdjustmentDto, StockCountAdjustmentStatus } from './types'

const ALL_VALUE = 'all'
const PAGE_SIZE = 20
const APPROVE_ROLES = [RoleNames.Admin, RoleNames.WarehouseManager]

const STATUS_ITEMS: Record<string, string> = {
  [ALL_VALUE]: 'Tüm durumlar',
  Pending: 'Bekliyor',
  Approved: 'Onaylandı',
  Rejected: 'Reddedildi',
}

function StatusBadge({ status }: { status: StockCountAdjustmentStatus }) {
  const styles: Record<StockCountAdjustmentStatus, string> = {
    Pending:
      'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/40 dark:text-yellow-300',
    Approved:
      'bg-green-100 text-green-800 dark:bg-green-900/40 dark:text-green-300',
    Rejected:
      'bg-gray-100 text-gray-800 dark:bg-gray-900/40 dark:text-gray-300',
  }
  const labels: Record<StockCountAdjustmentStatus, string> = {
    Pending: 'Bekliyor',
    Approved: 'Onaylandı',
    Rejected: 'Reddedildi',
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

export function StockCountAdjustmentsPage() {
  const canApprove = useHasAnyRole(APPROVE_ROLES)

  const [warehouseFilter, setWarehouseFilter] = useState<string | undefined>()
  const [statusFilter, setStatusFilter] = useState<
    StockCountAdjustmentStatus | undefined
  >()
  const [page, setPage] = useState(1)

  const { data: warehouses } = useWarehouses()
  const { data, isLoading } = useStockCountAdjustments({
    warehouseId: warehouseFilter,
    status: statusFilter,
    page,
    pageSize: PAGE_SIZE,
  })
  const adjustments = data?.items
  const totalCount = data?.totalCount ?? 0
  const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE))

  const warehouseItems = useMemo(
    () => ({
      [ALL_VALUE]: 'Tüm depolar',
      ...Object.fromEntries(
        (warehouses ?? []).map((warehouse) => [warehouse.id, warehouse.name]),
      ),
    }),
    [warehouses],
  )

  const [approvingAdjustment, setApprovingAdjustment] =
    useState<StockCountAdjustmentDto | null>(null)
  const [rejectingAdjustment, setRejectingAdjustment] =
    useState<StockCountAdjustmentDto | null>(null)

  return (
    <div className="space-y-4">
      <h2 className="text-xl font-semibold">Sayım Düzeltmeleri</h2>

      <div className="flex gap-2">
        <Select
          items={warehouseItems}
          value={warehouseFilter ?? ALL_VALUE}
          onValueChange={(value: string | null) => {
            setWarehouseFilter(!value || value === ALL_VALUE ? undefined : value)
            setPage(1)
          }}
        >
          <SelectTrigger>
            <SelectValue placeholder="Depo" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={ALL_VALUE}>Tüm depolar</SelectItem>
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
          onValueChange={(value: string | null) => {
            setStatusFilter(
              !value || value === ALL_VALUE
                ? undefined
                : (value as StockCountAdjustmentStatus),
            )
            setPage(1)
          }}
        >
          <SelectTrigger>
            <SelectValue placeholder="Tüm durumlar" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={ALL_VALUE}>Tüm durumlar</SelectItem>
            <SelectItem value="Pending">Bekliyor</SelectItem>
            <SelectItem value="Approved">Onaylandı</SelectItem>
            <SelectItem value="Rejected">Reddedildi</SelectItem>
          </SelectContent>
        </Select>
      </div>

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Depo</TableHead>
            <TableHead>SKU</TableHead>
            <TableHead>Ürün</TableHead>
            <TableHead>Fark</TableHead>
            <TableHead>Durum</TableHead>
            <TableHead>Oluşturulma Tarihi</TableHead>
            {canApprove && <TableHead className="w-40">Aksiyonlar</TableHead>}
          </TableRow>
        </TableHeader>
        <TableBody>
          {isLoading && (
            <TableRow>
              <TableCell colSpan={canApprove ? 7 : 6}>Yükleniyor...</TableCell>
            </TableRow>
          )}
          {adjustments?.map((adjustment) => (
            <TableRow key={adjustment.id}>
              <TableCell>{adjustment.warehouseName}</TableCell>
              <TableCell>{adjustment.productSku}</TableCell>
              <TableCell>{adjustment.productName}</TableCell>
              <TableCell>
                <DifferenceCell value={adjustment.differenceQuantity} />
              </TableCell>
              <TableCell>
                <StatusBadge status={adjustment.status} />
              </TableCell>
              <TableCell>
                {formatUtcDateTime(adjustment.createdAtUtc)}
              </TableCell>
              {canApprove && (
                <TableCell>
                  {adjustment.status === 'Pending' && (
                    <div className="flex gap-1">
                      <Button
                        size="sm"
                        onClick={() => setApprovingAdjustment(adjustment)}
                      >
                        Onayla
                      </Button>
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => setRejectingAdjustment(adjustment)}
                      >
                        Reddet
                      </Button>
                    </div>
                  )}
                </TableCell>
              )}
            </TableRow>
          ))}
        </TableBody>
      </Table>

      <PaginationControls
        page={page}
        totalPages={totalPages}
        totalCount={totalCount}
        pageSize={PAGE_SIZE}
        onPageChange={setPage}
      />

      <ApproveAdjustmentDialog
        adjustment={approvingAdjustment}
        onOpenChange={(open) => {
          if (!open) setApprovingAdjustment(null)
        }}
      />

      <RejectAdjustmentDialog
        adjustment={rejectingAdjustment}
        onOpenChange={(open) => {
          if (!open) setRejectingAdjustment(null)
        }}
      />
    </div>
  )
}

function ApproveAdjustmentDialog({
  adjustment,
  onOpenChange,
}: {
  adjustment: StockCountAdjustmentDto | null
  onOpenChange: (open: boolean) => void
}) {
  const approveAdjustment = useApproveStockCountAdjustment()

  function handleConfirm() {
    if (!adjustment) return

    approveAdjustment.mutate(adjustment.id, {
      onSuccess: () => {
        toast.success('Düzeltme onaylandı.')
        onOpenChange(false)
      },
      onError: (error) => {
        toast.error(getApiErrorMessage(error))
        onOpenChange(false)
      },
    })
  }

  return (
    <AlertDialog open={adjustment !== null} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogTitle>Düzeltmeyi onayla</AlertDialogTitle>
        <AlertDialogDescription>
          Bu düzeltmeyi onaylamak istediğinize emin misiniz? Onaylandığında
          depodaki stok miktarı fark kadar otomatik olarak
          güncellenecektir.
        </AlertDialogDescription>
        <AlertDialogFooter>
          <AlertDialogCancel>Vazgeç</AlertDialogCancel>
          <AlertDialogAction
            onClick={handleConfirm}
            disabled={approveAdjustment.isPending}
          >
            Onayla
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}

function RejectAdjustmentDialog({
  adjustment,
  onOpenChange,
}: {
  adjustment: StockCountAdjustmentDto | null
  onOpenChange: (open: boolean) => void
}) {
  const rejectAdjustment = useRejectStockCountAdjustment()

  function handleConfirm() {
    if (!adjustment) return

    rejectAdjustment.mutate(adjustment.id, {
      onSuccess: () => {
        toast.success('Düzeltme reddedildi.')
        onOpenChange(false)
      },
      onError: (error) => {
        toast.error(getApiErrorMessage(error))
        onOpenChange(false)
      },
    })
  }

  return (
    <AlertDialog open={adjustment !== null} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogTitle>Düzeltmeyi reddet</AlertDialogTitle>
        <AlertDialogDescription>
          Bu düzeltmeyi reddetmek istediğinize emin misiniz? Reddedildiğinde
          depodaki stok miktarı değişmeyecektir.
        </AlertDialogDescription>
        <AlertDialogFooter>
          <AlertDialogCancel>Vazgeç</AlertDialogCancel>
          <AlertDialogAction
            onClick={handleConfirm}
            disabled={rejectAdjustment.isPending}
          >
            Reddet
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}

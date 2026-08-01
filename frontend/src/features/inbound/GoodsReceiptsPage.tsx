import { Plus } from 'lucide-react'
import { useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
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

import { useApproveGoodsReceipt, useGoodsReceipts } from './api/goodsReceipts'
import type { GoodsReceiptDto, GoodsReceiptStatus } from './types'

const ALL_VALUE = 'all'
const APPROVE_ROLES = [
  RoleNames.Admin,
  RoleNames.WarehouseManager,
  RoleNames.WarehouseSupervisor,
]

const PAGE_SIZE = 20

const STATUS_ITEMS: Record<string, string> = {
  [ALL_VALUE]: 'Tüm durumlar',
  Draft: 'Taslak',
  Approved: 'Onaylandı',
}

function StatusBadge({ status }: { status: GoodsReceiptStatus }) {
  const isApproved = status === 'Approved'
  return (
    <span
      className={cn(
        'rounded-full px-2 py-0.5 text-xs font-medium',
        isApproved
          ? 'bg-green-100 text-green-800 dark:bg-green-900/40 dark:text-green-300'
          : 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/40 dark:text-yellow-300',
      )}
    >
      {isApproved ? 'Onaylandı' : 'Taslak'}
    </span>
  )
}

export function GoodsReceiptsPage() {
  const canApprove = useHasAnyRole(APPROVE_ROLES)

  const [warehouseFilter, setWarehouseFilter] = useState<string | undefined>()
  const [statusFilter, setStatusFilter] = useState<
    GoodsReceiptStatus | undefined
  >()
  const [page, setPage] = useState(1)

  const { data: warehouses } = useWarehouses()
  const { data, isLoading } = useGoodsReceipts({
    warehouseId: warehouseFilter,
    status: statusFilter,
    page,
    pageSize: PAGE_SIZE,
  })
  const receipts = data?.items
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

  const [viewingReceipt, setViewingReceipt] = useState<GoodsReceiptDto | null>(
    null,
  )
  const [approvingReceipt, setApprovingReceipt] =
    useState<GoodsReceiptDto | null>(null)

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold">Mal Kabul</h2>
        <Link
          to="/goods-receipts/new"
          className={buttonVariants({ variant: 'default' })}
        >
          <Plus /> Yeni Mal Kabul
        </Link>
      </div>

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
            <SelectValue placeholder="Tüm depolar" />
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
                : (value as GoodsReceiptStatus),
            )
            setPage(1)
          }}
        >
          <SelectTrigger>
            <SelectValue placeholder="Tüm durumlar" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={ALL_VALUE}>Tüm durumlar</SelectItem>
            <SelectItem value="Draft">Taslak</SelectItem>
            <SelectItem value="Approved">Onaylandı</SelectItem>
          </SelectContent>
        </Select>
      </div>

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Depo</TableHead>
            <TableHead>Durum</TableHead>
            <TableHead>Satır Sayısı</TableHead>
            <TableHead>Oluşturulma Tarihi</TableHead>
            <TableHead className="w-24">Detay</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {isLoading && (
            <TableRow>
              <TableCell colSpan={5}>Yükleniyor...</TableCell>
            </TableRow>
          )}
          {receipts?.map((receipt) => (
            <TableRow key={receipt.id}>
              <TableCell>{receipt.warehouseName}</TableCell>
              <TableCell>
                <StatusBadge status={receipt.status} />
              </TableCell>
              <TableCell>{receipt.lines.length}</TableCell>
              <TableCell>{formatUtcDateTime(receipt.createdAtUtc)}</TableCell>
              <TableCell>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => setViewingReceipt(receipt)}
                >
                  Detay
                </Button>
              </TableCell>
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

      <Dialog
        open={viewingReceipt !== null}
        onOpenChange={(open) => {
          if (!open) setViewingReceipt(null)
        }}
      >
        <DialogContent>
          {viewingReceipt && (
            <div className="space-y-4">
              <DialogTitle>Mal Kabul Detayı</DialogTitle>

              <div className="space-y-1 text-sm">
                <p>
                  <span className="text-muted-foreground">Depo: </span>
                  {viewingReceipt.warehouseName}
                </p>
                <p>
                  <span className="text-muted-foreground">Durum: </span>
                  <StatusBadge status={viewingReceipt.status} />
                </p>
                <p>
                  <span className="text-muted-foreground">
                    Oluşturulma Tarihi:{' '}
                  </span>
                  {formatUtcDateTime(viewingReceipt.createdAtUtc)}
                </p>
                {viewingReceipt.approvedAtUtc && (
                  <p>
                    <span className="text-muted-foreground">
                      Onay Tarihi:{' '}
                    </span>
                    {formatUtcDateTime(viewingReceipt.approvedAtUtc)}
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
                  {viewingReceipt.lines.map((line) => (
                    <TableRow key={line.productId}>
                      <TableCell>{line.productSku}</TableCell>
                      <TableCell>{line.productName}</TableCell>
                      <TableCell>{line.quantity}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>

              {viewingReceipt.status === 'Draft' && canApprove && (
                <DialogFooter>
                  <Button
                    onClick={() => {
                      setApprovingReceipt(viewingReceipt)
                      setViewingReceipt(null)
                    }}
                  >
                    Onayla
                  </Button>
                </DialogFooter>
              )}
            </div>
          )}
        </DialogContent>
      </Dialog>

      <ApproveGoodsReceiptDialog
        receipt={approvingReceipt}
        onOpenChange={(open) => {
          if (!open) setApprovingReceipt(null)
        }}
      />
    </div>
  )
}

function ApproveGoodsReceiptDialog({
  receipt,
  onOpenChange,
}: {
  receipt: GoodsReceiptDto | null
  onOpenChange: (open: boolean) => void
}) {
  const approveGoodsReceipt = useApproveGoodsReceipt()

  function handleConfirm() {
    if (!receipt) return

    approveGoodsReceipt.mutate(receipt.id, {
      onSuccess: () => {
        toast.success('Mal kabul onaylandı.')
        onOpenChange(false)
      },
      onError: (error) => {
        toast.error(getApiErrorMessage(error))
        onOpenChange(false)
      },
    })
  }

  return (
    <AlertDialog open={receipt !== null} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogTitle>Mal kabulü onayla</AlertDialogTitle>
        <AlertDialogDescription>
          Bu mal kabulü onaylamak istediğinize emin misiniz? Onaylandığında
          ilgili depodaki stok miktarları otomatik olarak artırılacaktır.
        </AlertDialogDescription>
        <AlertDialogFooter>
          <AlertDialogCancel>Vazgeç</AlertDialogCancel>
          <AlertDialogAction
            onClick={handleConfirm}
            disabled={approveGoodsReceipt.isPending}
          >
            Onayla
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}
